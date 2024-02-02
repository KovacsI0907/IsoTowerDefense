using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using System.Linq;

public class Texturer : MonoBehaviour
{

    //Set these Textures in the Inspector
    public Texture2D heightMap;
    public float probabilityMultiplier;
    public float levelHeight;
    public int crossGridSpacing;
    public int minEmptySpaces;
    public int emptyGridSpacing;
    public bool showTriangulation;
    Renderer renderer;
    private IEnumerable<IEdge> edges;

    // Use this for initialization
    void Start()
    {



        //Fetch the Renderer from the GameObject
        renderer = GetComponent<Renderer>();

        //Set the Texture you assign in the Inspector as the main texture (Or Albedo)
        Texture2D displayTexture = dotsMap(heightLevelMap(normalizedMap(heightMap), levelHeight), crossGridSpacing, minEmptySpaces, emptyGridSpacing);
        Debug.Log(GetNumDots(displayTexture));
        renderer.material.mainTexture = displayTexture;
    }

    private void OnDrawGizmos()
    {
        if(edges != null && showTriangulation)
        {
            foreach (var edge in edges)
            {
                Vector3 start = new Vector3((float)edge.Q.X, 0, (float)edge.Q.Y);
                Vector3 end = new Vector3((float)edge.P.X, 0, (float)edge.P.Y);

                Gizmos.DrawLine(start, end);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CalculateAndApplyTexture()
    {
        // Initialize renderer if not already done
        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
        }

        // Your existing texture calculation and application code
        Texture2D displayTexture = dotsMap(heightLevelMap(normalizedMap(heightMap), levelHeight), crossGridSpacing, minEmptySpaces, emptyGridSpacing);
        Debug.Log(GetNumDots(displayTexture));
        renderer.sharedMaterial.mainTexture = displayTexture;
        triangulate(displayTexture);
    }

    void triangulate(Texture2D dotsMap)
    {
        Delaunator delaunator = new Delaunator(GetPoints(dotsMap));
        edges = delaunator.GetEdges();
        Debug.Log("Edges: " + edges.Count());
        Debug.Log("Triangles: " + delaunator.GetTriangles().Count());
    }

    public class Point : IPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    Texturer.Point[] GetPoints(Texture2D dotsMap)
    {
        float scaleX = -100f / dotsMap.width;
        float scaleY = -100f / dotsMap.height;
        List<Point> pointList = new List<Point>();
        for(int y = 0; y < dotsMap.height; y++)
        {
            for(int x = 0;x<dotsMap.width; x++)
            {
                if(dotsMap.GetPixel(x,y).r > 0.5)
                {
                    pointList.Add(new Point(scaleX*x + 50f,scaleY*y+50f));
                }
            }
        }

        return pointList.ToArray();
    }

    int GetNumDots(Texture2D texture)
    {
        int sum = 0;

        for(int y = 0;y< texture.height;y++)
        {
            for(int x = 0; x < texture.width; x++)
            {
                if(texture.GetPixel(x,y).r > 0.5)
                {
                    sum++;
                }
            }
        }

        return sum;
    }

    Texture2D dotsMap(Texture2D heightLevelMap, int crossGridSpacing, int minEmptySpaces, int emptyGridSpacing)
    {
        Texture2D dots = new Texture2D(heightLevelMap.width, heightLevelMap.height);
        Color[] blacks = new Color[heightLevelMap.height*heightLevelMap.width];
        Array.Fill(blacks, Color.black);
        dots.SetPixels(blacks);
        for(int y = 0; y < heightLevelMap.height; y+=crossGridSpacing)
        {
            int pixelsSinceLastPoint = 0;
            for(int x = 0; x < heightLevelMap.width; x++)
            {
                if(heightLevelMap.GetPixel(x, y).r > 0.5)
                {
                    if (pixelsSinceLastPoint > minEmptySpaces)
                    {
                        dots.SetPixel(x, y, Color.white);
                        pixelsSinceLastPoint = -1;
                    }
                }

                pixelsSinceLastPoint++;
            }
        }

        for (int x = 0; x < heightLevelMap.width; x += crossGridSpacing)
        {
            int pixelsSinceLastPoint = 0;
            for (int y = 0; y < heightLevelMap.width; y++)
            {
                if (heightLevelMap.GetPixel(x, y).r > 0.5)
                {
                    if (pixelsSinceLastPoint > minEmptySpaces)
                    {
                        dots.SetPixel(x, y, Color.white);
                        pixelsSinceLastPoint = -1;
                    }
                }

                pixelsSinceLastPoint++;
            }
        }

        for (int y = 0; y < heightLevelMap.height; y += emptyGridSpacing)
        {
            int pixelsSinceLastPoint = emptyGridSpacing;
            for(int x = 0;x<heightLevelMap.width; x++)
            {
                if(dots.GetPixel(x,y).r > 0.5)
                {
                    pixelsSinceLastPoint = 0;
                    continue;
                }
                if(pixelsSinceLastPoint >= emptyGridSpacing)
                {
                    dots.SetPixel(x,y,Color.white);
                    pixelsSinceLastPoint = 0;
                }
                else
                {
                    pixelsSinceLastPoint++;
                }
            }
        }

        dots.Apply();

        return dots;
    }

    Texture2D heightLevelMap(Texture2D heightMap, float levelHeight)
    {
        Texture2D levelmap = new Texture2D(heightMap.width-1, heightMap.height-1);
        levelmap.filterMode = FilterMode.Point;
        Range[] ranges = generateRanges(levelHeight);

        for(int y = 0; y < heightMap.height-1; y++)
        {
            for(int x = 0; x < heightMap.width-1; x++)
            {
                float current = heightMap.GetPixel(x, y).r;
                float right = heightMap.GetPixel(x+1, y).r;
                float bottom = heightMap.GetPixel(x, y+1).r;

                if(hasLevelChange(ranges, current, right) || hasLevelChange(ranges, current, bottom))
                {
                    levelmap.SetPixel(x, y, Color.white);
                }
                else
                {
                    levelmap.SetPixel(x, y, Color.black);
                }
            }
        }

        levelmap.Apply();

        return levelmap;
    }


    private struct Range
    {
        public float start, end;
        public Range(float Start, float End)
        {
            start = Start;
            end = End;
        }

        public bool inRange(float val)
        {
            return val >= start && val < end;
        }
    }

    Range[] generateRanges(float levelHeight)
    {
        Range[] ranges = new Range[(int)Math.Ceiling(1 / levelHeight)];
        for(int i = 0;i< ranges.Length; i++)
        {
            ranges[i] = new Range(i*levelHeight, (i+1)*levelHeight);
        }

        ranges[ranges.Length - 1].end = 1.1f;

        return ranges;
    }

    bool hasLevelChange(Range[] ranges, float val1, float val2)
    {
        int range1 = 0;
        int range2 = 0;

        while (!ranges[range1].inRange(val1))
        {
            range1++;
        }

        while (!ranges[range2].inRange(val2))
        {
            range2++;
        }

        return !(range1 == range2);
    }

    Texture2D randomPointsMap(Texture2D map, float probabilityMultiplier)
    {
        int counter = 0;
        Texture2D randmap = new Texture2D(map.width, map.height);
        randmap.filterMode = FilterMode.Point;
        System.Random random = new System.Random();
        for (int y = 0; y < heightMap.height; y++)
        {
            for (int x = 0; x < heightMap.width; x++)
            {
                if(y>1 && randmap.GetPixel(x,y-1).r > 0.1)
                {
                    randmap.SetPixel(x, y, Color.black);
                    continue;
                }
                if (random.NextDouble() < map.GetPixel(x, y).r * probabilityMultiplier)
                {
                    randmap.SetPixel(x,y,Color.white);
                    counter++;
                    if(x < heightMap.width - 2)
                    {
                        x += 1;
                        randmap.SetPixel(x, y, Color.black);
                    }
                }
                else
                {
                    randmap.SetPixel(x, y, Color.black);
                }
            }
        }

        randmap.Apply();

        Debug.Log("Vertices: " + counter);

        return randmap;
    }

    void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, int lineWidth, Color a_Color)
    {
        float xPix = x1;
        float yPix = y1;

        float width = x2 - x1;
        float height = y2 - y1;
        float length = Mathf.Abs(width);
        if (Mathf.Abs(height) > length) length = Mathf.Abs(height);
        int intLength = (int)length;
        float dx = width / (float)length;
        float dy = height / (float)length;
        for (int i = 0; i <= intLength; i++)
        {
            a_Texture.SetPixel((int)xPix, (int)yPix, a_Color);

            xPix += dx;
            yPix += dy;
        }
    }

    Texture2D steepnessMap(Texture2D map)
    {
        Texture2D steepness = new Texture2D(map.width - 1, map.height - 1);

        steepness.filterMode = FilterMode.Point;

        for(int y = 1; y < heightMap.height; y++)
        {
            for(int x =  1; x < heightMap.width; x++)
            {
                float xdiff = map.GetPixel(x, y).r - map.GetPixel(x-1,y).r;
                float ydiff = map.GetPixel(x, y).r - map.GetPixel(x, y-1).r;
                float val = (xdiff + ydiff) / 2;

                steepness.SetPixel(x, y, new Color(val, val, val));
            }
        }

        steepness.Apply();

        return steepness;
    }

    Texture2D normalizedMap(Texture2D map)
    {
        float max = getMax(map);
        float min = getMin(map);

        Texture2D normalized = new Texture2D(map.width, map.height);

        for(int y = 0; y < heightMap.height;y++)
        {
            for(int x = 0;x < heightMap.width; x++)
            {
                float val = (map.GetPixel(x, y).r - min) / (max - min);
                normalized.SetPixel(x, y, new Color(val, val, val));
            }
        }

        normalized.Apply(); 
        
        return normalized;
    }

    Texture2D diffMap(Texture2D map)
    {
        Texture2D diffMap = new Texture2D(map.width-2, map.height - 2);
        diffMap.filterMode = FilterMode.Point;
        float angles = 0;
        for(int y = 1;y < heightMap.height-1; y++)
        {
            for(int x = 1; x < heightMap.width - 1; x++)
            {
                //lower
                float lxdiff = map.GetPixel(x,y).r - map.GetPixel(x-1,y).r;
                float lydiff = map.GetPixel(x,y).r - map.GetPixel(x, y-1).r;
                //upper
                float uxdiff = map.GetPixel(x + 1, y).r - map.GetPixel(x, y).r;
                float uydiff = map.GetPixel(x, y + 1).r - map.GetPixel(x, y).r;

                Vector2 lx = new Vector2(1, lxdiff);
                Vector2 ux = new Vector2(1, uxdiff);

                Vector2 ly = new Vector2(1, lydiff);
                Vector2 uy = new Vector2(1, uydiff);

                float val = (Vector2.Angle(lx, ux) + Vector2.Angle(ly, uy))/360f;
                angles++;
                if(angles%50 == 0)
                {
                    Debug.Log(Vector2.Angle(lx, ux) + "    " + Vector2.Angle(ly, uy));
                }

                diffMap.SetPixel(x, y, new Color(val, val, val));
            }
        }

        diffMap.Apply();
        return diffMap;
    }

    float getMax(Texture2D map)
    {
        float max = 0;
        for(int y = 0; y < heightMap.height; ++y)
        {
            for (int x = 0;x < heightMap.width; ++x)
            {
                if (map.GetPixel(x, y).r > max)
                    max = map.GetPixel(x, y).r;
            }
        }

        return max;
    }

    float getMin(Texture2D map)
    {
        float min = 255; //it's probably [0, 1] range but just in case
        for (int y = 0; y < heightMap.height; ++y)
        {
            for (int x = 0; x < heightMap.width; ++x)
            {
                if (map.GetPixel(x, y).r < min)
                    min = map.GetPixel(x, y).r;
            }
        }

        return min;
    }

    float normalize(float val, float min, float max)
    {
        return (val - min) / (max - min);
    }
}
