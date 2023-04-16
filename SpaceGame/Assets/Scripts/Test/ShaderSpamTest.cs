using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderSpamTest : MonoBehaviour
{
    public ComputeShader spamShader;
    public int spamAmount;
    int[] spamArray;
    ComputeBuffer spamBuffer;
    uint threadGroupSize;
    int threadGroups;

    private void Start()
    {
        spamArray = new int[spamAmount];
        spamBuffer = new ComputeBuffer(spamAmount, sizeof(int));
        spamBuffer.SetData(spamArray);
        spamShader.GetKernelThreadGroupSizes(0, out threadGroupSize, out _, out _);
        threadGroups = (int)((spamAmount + (threadGroupSize - 1)) / threadGroupSize);
        
    }

    void FixedUpdate()
    {      
        spamShader.SetBuffer(0, "test", spamBuffer);
        spamShader.Dispatch(0, threadGroups, 1, 1);
        spamBuffer.GetData(spamArray);
        Debug.Log(spamArray[3]);
    }

    private void OnDestroy()
    {
        //spamBuffer.Dispose();
    }
}
