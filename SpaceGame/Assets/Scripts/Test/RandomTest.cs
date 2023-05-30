using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTest : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;

    public float rot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forceDirection = p1.transform.position - p2.transform.position;
        Vector3 unitPerpVector = -forceDirection;
        unitPerpVector = RotateVectorByAngle(unitPerpVector, rot, Vector3.up);
        //unitPerpVector = Quaternion.AngleAxis(90, Vector3.up) * unitPerpVector;

        Debug.DrawLine(p2.transform.position, p2.transform.position + forceDirection);
        Debug.DrawLine(p2.transform.position, p2.transform.position + unitPerpVector);
        Debug.Log(unitPerpVector);
    }

    Vector3 RotateVectorByAngle(Vector3 inVector, float angle, Vector3 axis)
    {
        float halfAngle = angle * Mathf.Deg2Rad * 0.5f;
        float sinHalfAngle = Mathf.Sin(halfAngle);
        Vector3 normalizedAxis = axis.normalized;

        Vector3 q = new Vector3(
        normalizedAxis.x * sinHalfAngle,
        normalizedAxis.y * sinHalfAngle,
        normalizedAxis.z * sinHalfAngle
    );

        Vector3 t = 2.0f * Vector3.Cross(q, inVector);
        Vector3 rotatedVector = inVector + (q.y * t - Vector3.Cross(q, t));

        return rotatedVector;
    }
}
