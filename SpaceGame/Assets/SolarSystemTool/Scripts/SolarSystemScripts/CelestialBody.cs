using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    private Vector3 velocity;
    private float radius;

    [RangeEx(100.0f, 1000000.0f, 100.0f)] public float mass = 10000;
    [Range(-10, 10), SerializeField] public int scaleOffset = 0;
    [SerializeField] private Vector3 initialVelocity;

    private void OnValidate()
    {
        SetSize();
    }
    void Start()
    {
        velocity = initialVelocity;
    }

    public void AddForce(Vector3 force)
    {        
        velocity += force;
    }
    public void UpdatePosition()
    {      
        transform.position += velocity * Universe.timeStep;
    }

    public void SetSize()
    {      
        float scale = ((mass) / 1000000) * Universe.maxScale;
        scale = Mathf.Clamp(scale, Universe.minScale, Universe.maxScale);
        if (scaleOffset != 0) scale += 50 * scaleOffset;
        scale = Mathf.Clamp(scale, Universe.minScale, Universe.maxScale);
        transform.localScale = new Vector3(scale, scale, scale);
        radius = transform.lossyScale.x / 2;
    }

    public LineRenderer SetPathLine()
    {
        LineRenderer pathLine;
        pathLine = GetComponentInChildren<LineRenderer>();
        pathLine.startColor = GetComponent<MeshRenderer>().sharedMaterial.color;
        pathLine.endColor = GetComponent<MeshRenderer>().sharedMaterial.color;
        pathLine.widthMultiplier =  Mathf.Max(10, transform.lossyScale.x / 3);
        pathLine.widthMultiplier =  Mathf.Min(100, pathLine.widthMultiplier);
        return pathLine;
    }

    //getters
    public Vector3 GetVelocity() { return velocity; }
    public Vector3 GetInitialVelocity() { return initialVelocity; }
    public float GetRadius() { return radius; }

    public void SetInitialVelocity(Vector3 velocity) { initialVelocity = velocity; }
}
