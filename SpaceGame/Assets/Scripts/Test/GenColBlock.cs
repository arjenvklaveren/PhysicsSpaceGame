using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenColBlock : MonoBehaviour
{
    public int boxSize = 25;
    public float sphereSpacing = 1.0f;
    public float sphereSize = 1;

    public static ColSphere colSphere;

    void Start()
    {
        GenerateSphereBox();
        colSphere = GameObject.Find("MoveSphere").GetComponent<ColSphere>();
    }

    void GenerateSphereBox()
    {
        for (int x = 0; x < boxSize; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < boxSize; z++)
                {
                    Vector3 position = new Vector3(x * sphereSpacing, y * sphereSpacing, z * sphereSpacing);
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.AddComponent<ColObj>();
                    sphere.AddComponent<Rigidbody>();
                    sphere.GetComponent<Rigidbody>().mass = 1;
                    sphere.GetComponent<Rigidbody>().angularDrag = 0;
                    sphere.GetComponent<Rigidbody>().drag = 0;
                    sphere.GetComponent<Rigidbody>().useGravity = false;

                    Destroy(sphere.GetComponent<SphereCollider>());
                    sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
                    sphere.transform.parent = this.transform;
                    sphere.transform.localPosition = position;
                }
            }
        }
        float offsetValue = (boxSize * sphereSpacing) / 2;
        transform.localPosition -= new Vector3(offsetValue, offsetValue, offsetValue);
    }
}

