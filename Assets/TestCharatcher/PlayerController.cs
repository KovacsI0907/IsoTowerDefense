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
        input.x = -Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        return input;
    }
    private float AngleConverter(float angle) //-180 180
    {
        if(angle < 0)
        {
            angle = 360 + angle;
        }
        return angle;
    }
    private float GetRotationSign(Vector2 input)
    {
        float angle = AngleConverter(Vector2.SignedAngle(Vector2.one, input));
        float characterRotationY =rigidbody.transform.rotation.eulerAngles.y;
        float diff =  angle - characterRotationY ;
        float rotationSign = (angle - characterRotationY) / Mathf.Abs(angle - characterRotationY);
        
        if(diff > 180 && angle > characterRotationY)
        {
            rotationSign *=-1;
        }
        Debug.Log(angle + " " + characterRotationY + " " + diff + " "+rotationSign);
        return rotationSign;
    }
    
    private void Update()
    {
       Vector2 input= GetInput();
        if (input.magnitude != 0)
        {
            rigidbody.transform.rotation=(Quaternion.Euler(0, rigidbody.transform.rotation.eulerAngles.y + GetRotationSign(input) *angularVelocity * Time.deltaTime, 0));
        }

    }

    

}
    