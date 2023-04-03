using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LagTest : MonoBehaviour
{
    public GameObject c1;
    public GameObject c2;

    void Update()
    {
        List<LineRenderer> lines = new List<LineRenderer>();
        
        lines.Add(c1.GetComponent<LineRenderer>());
        lines.Add(c2.GetComponent<LineRenderer>());

        Vector3[][] array = new Vector3[lines.Count][];

        for (int i = 0; i < lines.Count; i++)
        {
            array[i] = new Vector3[10000];

            for (int j = 0; j < 10000; j++)
            {
                array[i][j] = new Vector3(j + i, j + i, j + i);
            }
        }

        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].positionCount = array[i].Length;
            lines[i].SetPositions(array[i]);
        }
    } 
}
