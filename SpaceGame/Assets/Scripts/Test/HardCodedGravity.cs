using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardCodedGravity : MonoBehaviour
{
    public Vector3 initialVelocity;
    public GameObject anglePlanet;
    Vector3 velocity;
    GameObject bigPlanet;

    private void Start()
    {
        velocity = initialVelocity;
        bigPlanet = GameObject.Find("BigPlanet");
    }

    void FixedUpdate()
    {
        Newton();
        Test();
    }

    void Newton()
    {       
        Vector3 forceDirection = bigPlanet.transform.position - transform.position;
        float forceDirectionLength = forceDirection.magnitude;
        forceDirection.Normalize();

        //force magnitude
        float force = Universe.G * (1000 * 1) / (forceDirectionLength * forceDirectionLength);
        //Debug.Log(transform.name + ": forceLength = " + forceDirectionLength + " | force = " + force);

        Vector3 forceVector = forceDirection * force;

        //Debug.DrawLine(transform.position, transform.position + forceVector * 50, GetComponent<MeshRenderer>().sharedMaterial.color);
        //Debug.DrawLine(transform.position, transform.position + initialVelocity, GetComponent<MeshRenderer>().sharedMaterial.color);

        //Debug.Log(Vector3.Angle(forceVector, initialVelocity));

        //force vector
        velocity += forceVector / 1;

        transform.position += velocity * 0.02f;
    }

    void Test()
    {
        Vector3 line1 = bigPlanet.transform.position - transform.position;
        Vector3 line2 = anglePlanet.transform.position - bigPlanet.transform.position;
        Debug.Log(Vector3.Angle(line1, line2));
    }
}
