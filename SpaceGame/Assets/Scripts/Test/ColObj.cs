using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColObj : MonoBehaviour
{
    private float radius;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private Rigidbody rb;

    void Start()
    {
        radius = transform.lossyScale.x / 2;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float colDist = Vector3.Distance(transform.position, GenColBlock.colSphere.transform.position) - GenColBlock.colSphere.radius;
        if (radius > colDist)
        {
            Vector3 diffVec = transform.position - GenColBlock.colSphere.transform.position;

            float colAngle = Vector3.Angle(diffVec, GenColBlock.colSphere.velocity);
            float pushPower = 1.5f - ((colAngle / 2) / 90);

            //Debug.Log(pushPower);
            if (pushPower > 1) pushPower = 1;
            transform.position += diffVec.normalized * (radius - colDist);

            Vector3 randomOffsetVec = transform.position - (GenColBlock.colSphere.transform.position + new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)));
            Vector3 pushDir = diffVec + randomOffsetVec;

            rb.velocity = pushDir.normalized * (GenColBlock.colSphere.velocity.magnitude * pushPower);
        }
        transform.position += velocity;
    }
}
