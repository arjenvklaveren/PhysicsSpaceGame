using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [RangeEx(100, 50000, 100)] public int mass = 10000;
    [RangeEx(1, 100), SerializeField] private int scaleMass = 100;
    public Vector3 initialVelocity;

    [HideInInspector] public Vector3 velocity;

    void Start()
    {
        velocity = initialVelocity;
    }

    private void OnValidate()
    {
        float scaleTemp = 0.01f * scaleMass;
        float scale = mass * Universe.scaleMultiplier * scaleTemp;
        if (scale > Universe.maxScale) scale = Universe.maxScale;
        if (scale < Universe.minScale) scale = Universe.minScale;
        transform.localScale = new Vector3(scale, scale, scale);
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
