using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Texturer))]
public class TexturerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Texturer texturer = (Texturer)target;

        // Add a button to the inspector
        if (GUILayout.Button("Calculate and Apply Texture"))
        {
            // Call the function to calculate and apply the texture
            texturer.CalculateAndApplyTexture();
        }
    }
}