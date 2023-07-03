using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemView2D : MonoBehaviour
{
    Rect rect;

    float gridSize = 200;
    float camSizeScale;

    float scrollSpeed;
    float panSpeed;

    SolarSystemCreateData systemData;

    Camera cam2D;
    Vector3 clampCamPos = new Vector3(0, 0, 0);

    bool isPanning = false;
    Vector3 startPanPosCam;
    Vector2 startPanPosMouse;
    Vector2 panOffset;

    Vector2 gridCamOffset = new Vector2(0, 0);
    Vector2 gridOffset = new Vector2(0,0);
    Vector2 gridCamMult = new Vector2(0, 0);

    bool spawnBoxActive = false;
    Vector2 spawnBoxPos;

    CelestialBody dragBody = null;

    enum BodyTypes { Planet, Moon, Ring, Sun }

    public SolarSystemView2D(Rect rect, SolarSystemCreateData data)
    {
        this.rect = rect;
        systemData = data;
        cam2D = data.Get2DCam();
        scrollSpeed = Mathf.Lerp(10, 100, cam2D.transform.localPosition.y / 10000);
        panSpeed = scrollSpeed / 5;
    }

    public void GuiUpdate()
    {
        EditorGUI.DrawRect(rect, Color.grey);
        Rect innerRect = new Rect(rect.x + 2, rect.y + 2, rect.width - (2 * 2), rect.height - (2 * 2));
        EditorGUI.DrawRect(innerRect, Color.black);
        DrawGrid();
        Handles.DrawCamera(innerRect, cam2D);

        ClampCam();
        OnCameraPan();
        OnCameraScroll();
        DrawOnScreenUI();
        DrawVelocity();
        EnableSpawnBox();
        ClickDragSelectBody();
    }

    void DrawGrid()
    {
        float gridRectSize = gridSize / camSizeScale;
        float gridCount = rect.width / gridRectSize;

        Color lineColor = new Color(1, 1, 1, 0.25f);
        int lineWidth = 1;

        panOffset = new Vector2(gridCamMult.x * (rect.width % gridRectSize), gridCamMult.y * (rect.width % gridRectSize));
        Vector2 panOffsetNorm = panOffset.normalized * (gridRectSize / 2);

        Vector2 linePos = new Vector2(rect.x + (int)rect.height / 2, rect.y + (int)rect.height / 2) + gridOffset + panOffset;
        Vector2 startPos = new Vector2(linePos.x, linePos.y);
        Vector2 centreOffset = new Vector2(rect.width + rect.x - linePos.x, rect.height + rect.x - linePos.y);

        EditorGUI.DrawRect(new Rect(linePos.x, rect.y, lineWidth, rect.height), lineColor);
        EditorGUI.DrawRect(new Rect(rect.x, linePos.y, rect.width, lineWidth), lineColor);

        for (int i = 0; i < Mathf.CeilToInt(gridCount) + 1; i++)
        {
            Vector2 lineSpace = new Vector2(rect.x + rect.width - linePos.x, rect.y + rect.height - linePos.y) + panOffsetNorm;
            linePos += new Vector2(gridRectSize, gridRectSize);

            if (lineSpace.x > gridRectSize + panOffsetNorm.x && linePos.x < startPos.x + centreOffset.x) EditorGUI.DrawRect(new Rect(linePos.x, rect.y, lineWidth, rect.height), lineColor);
            else if (lineSpace.x < gridRectSize + panOffsetNorm.x) { linePos.x = rect.x - gridRectSize + ((linePos.x - rect.x) % gridRectSize); centreOffset.x = -5; };
            if (lineSpace.y > gridRectSize + panOffsetNorm.y && linePos.y < startPos.y + centreOffset.y) EditorGUI.DrawRect(new Rect(rect.x, linePos.y, rect.width, lineWidth), lineColor);
            else if (lineSpace.y < gridRectSize + panOffsetNorm.y) { linePos.y = rect.y - gridRectSize + ((linePos.y - rect.y) % gridRectSize); centreOffset.y = -5; }
        }
    }

    void DrawOnScreenUI()
    {
        //scale bar
        int width = 1;
        Rect mainRect = new Rect(rect.x + 510, rect.y + 585, 75, width);

        float scaleValue = (int)(camSizeScale * mainRect.width) * 10;
        GUIStyle style = new GUIStyle();
        style.fontSize = 10;
        style.normal.textColor = Color.white;

        EditorGUI.DrawRect(mainRect, Color.white);
        EditorGUI.DrawRect(new Rect(mainRect.x, mainRect.y, width + 1, -6), Color.white);
        EditorGUI.DrawRect(new Rect(mainRect.x + mainRect.width - width, mainRect.y, width + 1, -6), Color.white);
        EditorGUI.LabelField(new Rect((mainRect.x + mainRect.width - (mainRect.width / 1.9f)) - scaleValue.ToString().Length * 3, mainRect.y - 16, 100, 10), scaleValue.ToString(), style);

        //predict box
        bool status = EditorGUI.Toggle(new Rect(rect.x + 15, rect.height - 27, 20, 20), systemData.GetPredictor().GetActive());
        systemData.GetPredictor().SetActive(status);
        EditorGUI.LabelField(new Rect(rect.x + 35, rect.height - 23, 300, 20), "Draw Trajectory (Relative)", style);
    }

    void DrawVelocity()
    {
        CelestialBody selectedBody = systemData.GetSelectedBody();
        Vector2 vecStart = CamToRectPos(selectedBody.transform.localPosition);
        vecStart += new Vector2(rect.width / 2, rect.height / 2);

        Vector3 initialVelBody = selectedBody.GetInitialVelocity();
        Vector2 bodyVel = new Vector2(initialVelBody.x, -initialVelBody.z) * 1.5f / camSizeScale;
        Vector2 perpBodyVel = new Vector2(bodyVel.y, -bodyVel.x);

        GUI.BeginClip(rect);
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(Texture2D.whiteTexture, 2.5f, vecStart, vecStart + bodyVel);
        Handles.DrawAAPolyLine(Texture2D.whiteTexture, 2.5f, vecStart + bodyVel, vecStart + (bodyVel / 2) + new Vector2(perpBodyVel.x, perpBodyVel.y) / 5);
        Handles.DrawAAPolyLine(Texture2D.whiteTexture, 2.5f, vecStart + bodyVel, vecStart + (bodyVel / 2) + new Vector2(-perpBodyVel.x, -perpBodyVel.y) / 5);
        GUI.EndClip();
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
                startPanPosCam = cam2D.transform.localPosition;
            }
        }
        if (e.type == EventType.MouseUp || IsPosOutOfBounds(e.mousePosition))
        {
            if (e.button == 2) isPanning = false;
        }
        if (isPanning)
        {
            Vector2 camDiffVector = e.mousePosition - startPanPosMouse;
            cam2D.transform.localPosition = startPanPosCam + (new Vector3(-camDiffVector.x, 0, camDiffVector.y) * panSpeed);
        }

        Vector2 halfSize = new Vector2(rect.width / 2, rect.height / 2);
        float camRectEdgeWidth = (RectToCamPos(new Vector2(rect.width / 2, rect.height / 2)).x - cam2D.transform.localPosition.x) / halfSize.x;
        camSizeScale = camRectEdgeWidth;
        Vector2 camPosRect = new Vector2(-clampCamPos.x / camRectEdgeWidth, clampCamPos.z / camRectEdgeWidth);

        Vector2 outBoundsVector = new Vector2(0, 0);
        gridCamMult = new Vector2(0, 0);

        float xDifBounds = camPosRect.x - (new Vector2(camPosRect.x, 0).normalized.x * halfSize.x);
        float yDifBounds = camPosRect.y - (new Vector2(0, camPosRect.y).normalized.y * halfSize.y);

        if (Mathf.Abs(camPosRect.x) > halfSize.x)
        {
            outBoundsVector.x = xDifBounds;
            gridCamMult.x = (int)((outBoundsVector.x + panOffset.x) / rect.width) + Mathf.CeilToInt(new Vector2(outBoundsVector.x, 0).normalized.x);
        }
        if (Mathf.Abs(camPosRect.y) > halfSize.y)
        {
            outBoundsVector.y = yDifBounds;
            gridCamMult.y = (int)((outBoundsVector.y + panOffset.y) / rect.height) + Mathf.CeilToInt(new Vector2(0, outBoundsVector.y).normalized.y);
        }

        gridCamOffset = new Vector2(rect.width * gridCamMult.x, rect.height * gridCamMult.y);
        gridOffset = camPosRect - new Vector2(gridCamOffset.x, gridCamOffset.y);
    }
    void OnCameraScroll()
    {
        Event e = Event.current;
        if(e.type == EventType.ScrollWheel)
        {
            float yPosScrollDelta = e.delta.y * scrollSpeed;
            cam2D.transform.localPosition += new Vector3(0, yPosScrollDelta, 0);
            scrollSpeed = Mathf.Lerp(10, 100, cam2D.transform.localPosition.y / 10000);
            panSpeed = scrollSpeed / 5;
        }
    }

    void ClickDragSelectBody()
    {
        Event e = Event.current;
        Vector2 rectClickPos = ToCentreCoords(e.mousePosition);
        Vector3 camClickPos = RectToCamPos(rectClickPos);
        CelestialBody hitBody = new CelestialBody();
        CelestialBody selectedBody = systemData.GetSelectedBody();

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Vector3 rayDirection = camClickPos - cam2D.transform.localPosition;
            RaycastHit hit;
            if (Physics.Raycast(cam2D.transform.localPosition, rayDirection, out hit, Mathf.Infinity))
            {
                if (hit.transform.GetComponentInParent<CelestialBody>()) hitBody = hit.transform.GetComponentInParent<CelestialBody>();
                if (hitBody != selectedBody) systemData.SetSelectedBody(hitBody);
                else { dragBody = hitBody; }
            }
            else if(Vector3.Distance(camClickPos, selectedBody.transform.localPosition) < selectedBody.GetRadius() * 3)
            {
                hitBody = selectedBody;
                systemData.SetSelectedBody(hitBody);
                dragBody = hitBody;
            }
        }
        if (e.type == EventType.MouseUp && dragBody != null)
        {
            if (e.button == 0)
            {
                dragBody = null;
            }
        }
        if (dragBody != null) dragBody.transform.localPosition = camClickPos;
    }

    void EnableSpawnBox()
    {
        Event e = Event.current;
        if (!IsPosOutOfBounds(e.mousePosition) && e.type == EventType.MouseDown)
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
            GUIStyle style = new GUIStyle(); style.fontSize = 10; style.normal.textColor = Color.white;
            if (GUI.Button(new Rect(spawnBoxPos.x + 90, spawnBoxPos.y + 2, 15, 15), new GUIContent("x"), style)) { spawnBoxActive = false; }
            EditorGUI.LabelField(new Rect(spawnBoxPos.x + 30, spawnBoxPos.y + 3, 90, 25), "Create:");
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 30, 90, 25), new GUIContent("Planet"))) { SpawnBody(BodyTypes.Planet); }
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 60, 90, 25), new GUIContent("Ring"))) { SpawnBody(BodyTypes.Ring); }
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 90, 90, 25), new GUIContent("Sun"))) {  }
            if (GUI.Button(new Rect(spawnBoxPos.x + 5, spawnBoxPos.y + 120, 90, 25), new GUIContent("Moon"))) {  }
        }
    }

    void SpawnBody(BodyTypes type)
    {
        GameObject newBody = Instantiate(Resources.Load<GameObject>("Prefabs/Body"), systemData.GetSystemObject().transform.GetChild(0).transform);
        newBody.transform.position = RectToCamPos(ToCentreCoords(spawnBoxPos));
        newBody.transform.name = "New" + type.ToString();
        systemData.GetManager().AddBody(newBody.GetComponent<CelestialBody>());

        if (type == BodyTypes.Planet) { }
        if (type == BodyTypes.Ring) { GameObject rings = Instantiate(Resources.Load<GameObject>("Prefabs/Ring"), newBody.transform); }
        if (type == BodyTypes.Sun) { }
        if (type == BodyTypes.Moon) { }

        systemData.SetSelectedBody(newBody);
        spawnBoxActive = false;
    }

    void ClampCam()
    {
        float clampMult = cam2D.farClipPlane - 10;
        float clampX = Mathf.Clamp(cam2D.transform.localPosition.x, -clampMult * 2, clampMult * 2);
        float clampY = Mathf.Clamp(cam2D.transform.localPosition.y, cam2D.nearClipPlane, clampMult);
        float clampZ = Mathf.Clamp(cam2D.transform.localPosition.z, -clampMult * 2, clampMult * 2);
        clampCamPos = new Vector3(clampX, clampY, clampZ);
        cam2D.transform.localPosition = clampCamPos;
    }

    Vector2 ToCentreCoords(Vector2 vector)
    {
        return vector - new Vector2(rect.width / 2, rect.height / 2);
    }

    Vector2 CamToRectPos(Vector3 position)
    {
        float scaleValue = 0.0019374444444444f * cam2D.transform.localPosition.y;
        Vector3 screenPos = (position - cam2D.transform.localPosition) / scaleValue;
        return new Vector2(screenPos.x, -screenPos.z); 
    }
    Vector3 RectToCamPos(Vector2 position)
    {
        float scaleValue = 0.5812333333333333f * cam2D.transform.localPosition.y / (rect.width / 2);
        Vector2 camPos = position * scaleValue;
        return new Vector3(camPos.x + cam2D.transform.localPosition.x, 0, -camPos.y + cam2D.transform.localPosition.z);
    }

    bool IsPosOutOfBounds(Vector2 pos)
    {
        bool value = false;
        if (pos.x < rect.x || pos.x > rect.x + rect.width || pos.y < rect.y || pos.y > rect.height + rect.y) value = true;
        return value;
    }
}

//1743.7f

