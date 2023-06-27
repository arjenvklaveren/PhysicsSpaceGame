using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemView2D : MonoBehaviour
{
    Rect rect;

    float borderSize = 2;
    int gridSize = 400;

    float scrollSpeed;
    float panSpeed;

    SolarSystemCreateData systemData;
    Camera cam2D;

    bool isPanning = false;
    Vector3 startPanPosCam;
    Vector2 startPanPosMouse;
    Vector2 gridOffset = new Vector2(0,0);

    bool spawnBoxActive = false;
    Vector2 spawnBoxPos;

    public SolarSystemView2D(Rect rect, SolarSystemCreateData data)
    {
        this.rect = rect;
        systemData = data;
        cam2D = data.Get2DCam();
        scrollSpeed = 20;
        panSpeed = scrollSpeed / 5;
    }

    public void Draw()
    {
        EditorGUI.DrawRect(rect, Color.grey);
        Rect innerRect = new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - (borderSize * 2), rect.height - (borderSize * 2));
        EditorGUI.DrawRect(innerRect, Color.black);
        DrawGrid();
        Handles.DrawCamera(innerRect, cam2D);
        OnCameraPan();
        OnCameraScroll();
        EnableSpawnBox();
    }

    void DrawGrid()
    {
        int gridCount = Mathf.FloorToInt(cam2D.transform.position.y / gridSize);
        float gridRectSize = rect.height / gridCount;

        Color lineColor = new Color(1, 1, 1, 0.25f);
        int lineWidth = 1;

        Vector2 linePos = new Vector2(rect.x + (int)rect.height / 2, rect.y + (int)rect.height / 2) + gridOffset;

        EditorGUI.DrawRect(new Rect(linePos.x, rect.y, lineWidth, rect.height), lineColor);
        EditorGUI.DrawRect(new Rect(rect.x, linePos.y, rect.width, lineWidth), lineColor);
        for (int i = 0; i < gridCount; i++)
        {
            Vector2 lineSpace = new Vector2(rect.x + rect.width - linePos.x, rect.y + rect.height - linePos.y);
            if (lineSpace.x > gridRectSize) EditorGUI.DrawRect(new Rect(linePos.x += gridRectSize, rect.y, lineWidth, rect.height), lineColor);
            else linePos.x = rect.x - gridRectSize + (gridRectSize - lineSpace.x);
            if (lineSpace.y > gridRectSize) EditorGUI.DrawRect(new Rect(rect.x, linePos.y += gridRectSize, rect.width, lineWidth), lineColor);
            else linePos.y = rect.y - gridRectSize + (gridRectSize - lineSpace.y);
        }
    }

    void EnableSpawnBox()
    {
        Event e = Event.current;
        if (!IsMouseOutOfBounds(e.mousePosition) && e.type == EventType.MouseDown)
        {
            if (e.button == 1)
            {
                spawnBoxActive = true;
                spawnBoxPos = e.mousePosition;
            }        
        }
        if (spawnBoxActive)
        {
            EditorGUI.DrawRect(new Rect(spawnBoxPos.x, spawnBoxPos.y, 100, 150), Color.grey);
            EditorGUI.DrawRect(new Rect(spawnBoxPos.x + 2, spawnBoxPos.y + 2, 96, 146), Color.black);
            EditorGUI.LabelField(new Rect(spawnBoxPos.x + 30, spawnBoxPos.y + 5, 90, 25), "Create:");
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 30, 90, 25), new GUIContent("Planet")))
            {
                spawnBoxActive = false;
                GameObject newBody = Instantiate(Resources.Load<GameObject>("Prefabs/Body"), systemData.GetSystemObject().transform);
                newBody.transform.position = RectToCamPos(ToCentreCoords(spawnBoxPos));
                systemData.GetManager().AddBody(newBody.GetComponent<CelestialBody>());
            }
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 60, 90, 25), new GUIContent("Ring"))) { }
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 90, 90, 25), new GUIContent("Sun"))) { }
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 120, 90, 25), new GUIContent("Moon"))) { }
        }
    }

    void OnCameraPan()
    {       
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 2 && !isPanning)
            {
                isPanning = true; 
                startPanPosMouse = e.mousePosition;
                startPanPosCam = cam2D.transform.position;
            }
        }
        if (e.type == EventType.MouseUp || IsMouseOutOfBounds(e.mousePosition))
        {
            if (e.button == 2) isPanning = false;
        }
        if (isPanning)
        {
            Vector2 camDiffVector = e.mousePosition - startPanPosMouse;
            cam2D.transform.position = startPanPosCam + (new Vector3(-camDiffVector.x, 0, camDiffVector.y) * panSpeed);
        }

        gridOffset = CamToRectPos(Vector3.zero);

        int halfSize = (int)rect.height / 2;

        if (gridOffset.x > halfSize) gridOffset.x = -halfSize;
        if (gridOffset.x < -halfSize) gridOffset.x = halfSize;
        if (gridOffset.y > halfSize) gridOffset.y = -halfSize;
        if (gridOffset.y < -halfSize) gridOffset.y = halfSize;
    }

    void OnCameraScroll()
    {
        Event e = Event.current;
        if(e.type == EventType.ScrollWheel)
        {
            cam2D.transform.position += new Vector3(0, e.delta.y * scrollSpeed, 0);
            scrollSpeed = Mathf.Lerp(10, 100, cam2D.transform.position.y / 10000);
            panSpeed = scrollSpeed / 5;
        }
        if(cam2D.transform.position.y > cam2D.farClipPlane - 10) cam2D.transform.position = new Vector3(0, cam2D.farClipPlane - 10, 0);
        if(cam2D.transform.position.y < cam2D.nearClipPlane) cam2D.transform.position = new Vector3(0, cam2D.nearClipPlane, 0);
    }

    Vector2 ToCentreCoords(Vector2 vector)
    {
        return vector - new Vector2(rect.width / 2, rect.height / 2);
    }

    Vector2 CamToRectPos(Vector3 position)
    {
        //Vector3 offsetVector = new Vector3(cam2D.transform.position.x + position.x, 0, cam2D.transform.position.z + position.z) * 0.2f;
        //Vector3 screenPos = cam2D.WorldToScreenPoint(offsetVector) - new Vector3(381.50f, 375.50f, 3000);

        float scaleValue = 0.0019374444444444f * cam2D.transform.position.y;
        Vector3 screenPos = (position - cam2D.transform.position) / scaleValue;

        return new Vector2(screenPos.x, -screenPos.z); 
    }
    Vector3 RectToCamPos(Vector2 position)
    {
        float scaleValue = 0.5812333333333333f * cam2D.transform.position.y / (rect.width / 2);
        Vector2 camPos = position * scaleValue;
        return new Vector3(camPos.x + cam2D.transform.position.x, 0, -camPos.y + cam2D.transform.position.z);
    }

    bool IsMouseOutOfBounds(Vector2 mousePos)
    {
        bool value = false;
        if (mousePos.x < rect.x || mousePos.x > rect.x + rect.width || mousePos.y < rect.y || mousePos.y > rect.height + rect.y) value = true;
        return value;
    }
}

