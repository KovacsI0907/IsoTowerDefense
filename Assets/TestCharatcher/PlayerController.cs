using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 1;
    [SerializeField] private float turnSpeed = 1000;
    [SerializeField] Camera camera;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private Vector3 input;

    private void Update(){
        GatherInput();
        Look();
    }

    private void FixedUpdate(){
        Move();
    }

    private void GatherInput(){
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    private void Look(){
        if (input == Vector3.zero) { return; } 
        var rot = Quaternion.LookRotation(input.ToIso(), Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, turnSpeed * Time.deltaTime);
    }

    private void Move(){
        Vector3 position = transform.position + (transform.forward * input.normalized.magnitude * speed * 0.25f * Time.deltaTime);
        rb.MovePosition(position);
        camera.transform.SetPositionAndRotation(transform.position + offset , camera.transform.rotation);
    }
}

public static class Helpers{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
