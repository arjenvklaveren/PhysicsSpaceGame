using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColSphere : MonoBehaviour
{
    public float radius;
    public Vector3 velocity = new Vector3(10, 0, 0);

    void Start()
    {
        radius = transform.lossyScale.x / 2;
    }

    void FixedUpdate()
    {
        GetComponent<Rigidbody>().velocity = velocity;
    }
}
