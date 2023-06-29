using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [RangeEx(100, 50000, 100)] public int mass = 10000;
    [RangeEx(1, 100), SerializeField] public int scaleMass = 100;

    [SerializeField] private Vector3 initialVelocity;
    private Vector3 velocity;

    public enum BodyTypes { Planet, Moon, Ring, Sun }
    BodyTypes type = BodyTypes.Planet;

    void Start()
    {
        velocity = initialVelocity;
    }

    private void OnValidate()
    {
        SetSize();
    }

    public void AddForce(Vector3 force)
    {        
        velocity += force;
    }
    public void UpdatePosition()
    {      
        transform.position += velocity * Universe.timeStep;
    }
    public Vector3 GetVelocity()
    {
        return velocity;
    }
    public Vector3 GetInitialVelocity()
    {
        return initialVelocity;
    }
    public void SetSize()
    {
        float scaleTemp = 0.01f * scaleMass;
        float scale = mass * Universe.scaleMultiplier * scaleTemp;
        if (scale > Universe.maxScale) scale = Universe.maxScale;
        if (scale < Universe.minScale) scale = Universe.minScale;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
