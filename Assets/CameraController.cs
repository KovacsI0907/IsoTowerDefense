using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    void Update()
    {
        Vector3 camerapos = new Vector3(rb.transform.position.x-7, rb.transform.position.y+7, rb.transform.position.z-7);
        this.transform.position = camerapos;
    }
}
