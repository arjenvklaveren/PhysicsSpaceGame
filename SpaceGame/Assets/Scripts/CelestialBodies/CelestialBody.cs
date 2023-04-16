using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public Vector3 initialVelocity;
    public float mass;

    [HideInInspector] public Vector3 velocity;

    void Start()
    {
        velocity = initialVelocity;
    }

    private void OnValidate()
    {
        transform.localScale = new Vector3(0.02f, 0.02f, 0.02f) * mass;
    }

    public void AddForce(Vector3 force)
    {        
        velocity += force;
    }

    public void UpdatePosition()
    {
        transform.position += velocity * Universe.timeStep;
    }
}
