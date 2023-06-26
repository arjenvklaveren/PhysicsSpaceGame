using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemView2D : MonoBehaviour
{
    Rect rect;
    SolarSystemCreateWindow window;

    float borderSize = 2;
    int gridSize = 100;
    int scrollSpeed = 10;

    Camera cam2D;

    public SolarSystemView2D(Rect rect, SolarSystemCreateWindow window)
    {
        this.rect = rect;
        this.window = window;
        cam2D = window.GetSystemData().Get2DCam();
    }
    public void Draw()
    {
        EditorGUI.DrawRect(rect, Color.grey);
        Rect innerRect = new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - (borderSize * 2), rect.height - (borderSize * 2));
        EditorGUI.DrawRect(innerRect, Color.black);
        DrawGrid();
        Handles.DrawCamera(innerRect, cam2D);
        OnCameraScroll();
    }

    void OnCameraPan()
    {

    }

    void OnCameraScroll()
    {
        Event e = Event.current;
        if(e.type == EventType.ScrollWheel)
        {
            cam2D.transform.position += new Vector3(0, e.delta.y * scrollSpeed, 0);
            window.Repaint();
        }
        if(cam2D.transform.position.y > cam2D.farClipPlane) cam2D.transform.position = new Vector3(0, cam2D.farClipPlane, 0);
        if(cam2D.transform.position.y < cam2D.nearClipPlane) cam2D.transform.position = new Vector3(0, cam2D.nearClipPlane, 0);
    }

    void DrawGrid()
    {
        float gridCount = cam2D.transform.position.y / gridSize;
        float gridOffset = rect.height / gridCount;

        int lineWidth = 1;

        float halfRect = rect.height / 2;
        Color lineColor = new Color(1, 1, 1, 0.25f);

        Vector2 linePos = new Vector2(rect.x + halfRect, rect.y + halfRect);

        EditorGUI.DrawRect(new Rect(linePos.x, rect.y, lineWidth, rect.height), lineColor);
        EditorGUI.DrawRect(new Rect(rect.x, linePos.y, rect.width, lineWidth), lineColor);

        float linePosOffset = 0;

        for (int i = 0; i < gridCount / 2; i++)
        {
            linePosOffset += gridOffset;
            EditorGUI.DrawRect(new Rect(linePos.x + linePosOffset, rect.y, lineWidth, rect.height), lineColor);
            EditorGUI.DrawRect(new Rect(linePos.x - linePosOffset, rect.y, lineWidth, rect.height), lineColor);
            EditorGUI.DrawRect(new Rect(rect.x, linePos.y + linePosOffset, rect.width, lineWidth), lineColor);
            EditorGUI.DrawRect(new Rect(rect.x, linePos.y - linePosOffset, rect.width, lineWidth), lineColor);
        }
    }
}

