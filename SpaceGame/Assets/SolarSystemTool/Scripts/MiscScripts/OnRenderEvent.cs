using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OnRenderEvent : MonoBehaviour
{
    public delegate void RenderImageEventHandler(RenderTexture src, RenderTexture dest);
    public static event RenderImageEventHandler OnRenderImageEvent;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
        OnRenderImageEvent?.Invoke(src, dest);
    }
}
