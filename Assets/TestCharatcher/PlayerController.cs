using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float velocity;
    [SerializeField] private float angularVelocity; //    fok / masodperc
    private Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private Vector2 GetInput()
    {
        Vector2 input = Vector2.zero;
        //Azért getAxisRaw kell getAxis helyett, mert a getaxis simítja a bemenetet, ami elrontja a túllövés megakadályozó mûködését
        input.x = -Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        return input;
    }
    private float AngleConverter(float angle) //-180 180 -> 0 360
    {
        if(angle < 0)
        {
            angle = 360 + angle;
        }
        return angle;
    }

    private float GetDesiredRotation(Vector2 input)
    {
        return AngleConverter(Vector2.SignedAngle(Vector2.one, input));
    }
    private float GetRotationSign(Vector2 input, float characterRotationY)
    {

        float desiredRotation = GetDesiredRotation(input);
        float diff =  desiredRotation - characterRotationY ;
        if(diff == 0)
        {
            return 0;
        }

        float rotationSign = (diff) / Mathf.Abs(diff);

        if(Mathf.Abs(diff) > 180)
        {
            rotationSign *= -1;
        }
        return rotationSign;
    }

    private bool DetectOvershoot(float currentRotation, float desiredRotation, float rotationToAdd)
    {
        if(currentRotation < desiredRotation && currentRotation + rotationToAdd > desiredRotation)
        {
            return true;
        }

        if(currentRotation > desiredRotation && currentRotation + rotationToAdd < desiredRotation)
        {
            return true;
        }

        return false;
    }
    private void Update()
    {
        Vector2 input= GetInput();
        if (input.magnitude != 0)
        {
            float currentRotationY = rigidbody.transform.rotation.eulerAngles.y;
            float desiredRotationY = GetDesiredRotation(input);
            float rotationToAdd = GetRotationSign(input, currentRotationY) * angularVelocity * Time.deltaTime;

            float finalRotationY = currentRotationY + rotationToAdd;

            //megnézzük, hogy túllõne-e a célon, ha igen, akkor csak beállítjuk a célra, ez megakadálylozza a rezgést
            if(DetectOvershoot(currentRotationY, desiredRotationY, rotationToAdd))
            {
                finalRotationY = desiredRotationY; 
            }

            rigidbody.transform.rotation = Quaternion.Euler(0, finalRotationY, 0);
            currentRotationY = rigidbody.transform.rotation.eulerAngles.y;
            Vector3 direction = new Vector3(Mathf.Cos(currentRotationY * Mathf.Deg2Rad + Mathf.PI/2), 0, Mathf.Sin(currentRotationY * Mathf.Deg2Rad - Mathf.PI/2));
            Debug.Log(direction.ToString());
            rigidbody.MovePosition(transform.position  + Time.deltaTime * velocity * direction * -1);
        }


    }

    

}
    