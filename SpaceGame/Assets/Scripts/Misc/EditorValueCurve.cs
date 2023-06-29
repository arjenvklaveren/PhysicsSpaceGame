using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorValueCurve
{
    public Rect rect;

    public readonly int minX;
    public readonly int maxX;
    public readonly int minY;
    public readonly int maxY;

    List<float> values = new List<float>();
    List<CurveAnchor> anchors = new List<CurveAnchor>();
    int anchorCount;

    CurveAnchor dragAnchor;

    public static Material alphaMaterial;
    public static Texture circleImage;

    bool hasStartDrag = false;
    bool isDragging = false;

    public EditorValueCurve(Rect rect, int minX, int maxX, int minY, int maxY, int anchorCount)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        this.anchorCount = anchorCount + 2;
        this.rect = rect;
        isDragging = false;

        float middleValue = (maxY - minY) / 2 + minY;

        CurveAnchor leftAnchor = new CurveAnchor(minX, middleValue, this);
        anchors.Add(leftAnchor);
        for (int i = 0; i < (this.anchorCount - 2); i++)
        {
            float anchorXPos = (i + 1) * (rect.width / (anchorCount + 1));
            float anchorYPos = middleValue;
            CurveAnchor anchor = new CurveAnchor(anchorXPos, anchorYPos, this);
            anchors.Add(anchor);
        }
        CurveAnchor rightAnchor = new CurveAnchor(maxX, middleValue, this);
        anchors.Add(rightAnchor);
        SetValuesBasedOnAnchors();
    }

    RingTextureDrawWindow window;
    public void Draw(RingTextureDrawWindow window)
    {
        this.window = window;

        if (anchors.Count == 0) return;
        foreach(CurveAnchor anchor in anchors)
        {
            anchor.Draw();

            if(anchors.IndexOf(anchor) != anchors.Count - 1)
            {
                CurveLine line = new CurveLine(rect);
                line.Draw(anchor.pos, anchors[anchors.IndexOf(anchor) + 1].pos);
            }
        }
        OnDragAnchors();
    }

    public void SetAnchorValue(int index, float value) 
    {
        if (value > maxY) value = maxY; 
        if (value < minY) value = minY; 
        if(index <= anchors.Count - 1 && index >= 0) anchors[index].pos.y = (maxY - (value - minY)) / maxY * rect.height; 
    }

    public int GetAnchorCount() { return anchorCount; }
    public int GetValueCount() { return values.Count; }
    public float GetAnchorMargin() { return rect.width / (anchorCount - 1); }

    public float GetValueAtPoint(float point)
    {
        point = point / (float)maxX * values.Count;
        int valueIndex = Mathf.RoundToInt(point);
        if (valueIndex < 0) valueIndex = 0;
        if (valueIndex == values.Count) valueIndex = values.Count - 1;

        float value = values[valueIndex];
        value = (maxY - (value - minY));
        return value;
    }

    public void SetValuesBasedOnAnchors()
    {
        values.Clear();
        values.Add(anchors[0].pos.y / rect.height * maxY);

        for (int i = 0; i < anchors.Count - 1; i++)
        {
            int valueAmount = (maxX / anchorCount);

            for (int j = 0; j < valueAmount; j++)
            {
                float currentAnchorValue = anchors[i].pos.y;
                float nextAnchorValue = anchors[i + 1].pos.y;

                float addValue = Mathf.Lerp(currentAnchorValue, nextAnchorValue, (float)j / valueAmount);
                values.Add(addValue / rect.height * maxY);
            }
        }
    }

    public bool HasStopDrag()
    {
        bool value = false;
        if(hasStartDrag && !isDragging)
        {
            hasStartDrag = false;
            value = true;
        }
        return value;
    }

    bool MouseIsOutBounds()
    {
        bool value = false;
        Vector2 mousePos = Event.current.mousePosition;
        if (mousePos.x < 0 || mousePos.x > 1040 || mousePos.y < 0 || mousePos.y > 300) value = true;
        return value;
    }

    void OnDragAnchors()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 0)
            {
                dragAnchor = new CurveAnchor(666, 666, this);
                float closestDistance = 20;

                foreach (CurveAnchor anchor in anchors)
                {
                    Vector2 toMouseVector = Event.current.mousePosition - anchor.relativePos;
                    float distance = toMouseVector.magnitude;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        dragAnchor = anchor;
                        dragAnchor.dragStartY = dragAnchor.pos.y;
                    }
                }
                if (closestDistance < 10 && !isDragging && dragAnchor.pos.x != 666)
                {
                    isDragging = true;
                    dragAnchor.dragStartY = dragAnchor.pos.y;
                }
                else
                {
                    return;
                }
            }
        }
        if (!isDragging) return;
        if (e.type == EventType.MouseUp || MouseIsOutBounds())
        {
            if (dragAnchor == null) return;
            if (e.button == 0 || MouseIsOutBounds())
            {
                dragAnchor.drawColor = Color.white;
                isDragging = false;
                hasStartDrag = false;
                SetValuesBasedOnAnchors();
                window.OnStopDragCurve();
            }
        }
        if (isDragging && dragAnchor.pos.x != 666)
        {
            DragAnchor(dragAnchor);
        }
    }
    void DragAnchor(CurveAnchor anchor)
    {
        //Determine all anchors
        int elasticRange = Mathf.CeilToInt(anchorCount / 50);
        int mainAnchorIndex = anchors.IndexOf(anchor);
        anchor.drawColor = Color.green;

        //Set position of main anchor
        float realYPosMouse = Event.current.mousePosition.y - rect.y;
        float dragDist = realYPosMouse - anchor.dragStartY;

        for (int i = -elasticRange; i < elasticRange + 1; i++)
        {
            int anchorIndex = mainAnchorIndex + i;
            if (anchorIndex < 0 || anchorIndex > anchorCount - 1) continue;
            CurveAnchor currentAnchor = anchors[anchorIndex];
            if (!hasStartDrag)
            {
                currentAnchor.dragStartY = currentAnchor.pos.y;
            }
            float divideValue = (float)(elasticRange + 1) / (float)((elasticRange + 1) - Mathf.Abs(i));
            currentAnchor.pos.y = currentAnchor.dragStartY + (dragDist / divideValue);

            if (currentAnchor.pos.y > rect.height) currentAnchor.pos.y = rect.height;
            if (currentAnchor.pos.y < 0) currentAnchor.pos.y = 0;
        }
        if (!hasStartDrag) hasStartDrag = true;
    }
}

class CurveAnchor
{
    public Vector2 pos;
    public Vector2 relativePos;

    public float dragStartY;

    Rect parentRect;

    float circleSize = 3;

    public Color drawColor = Color.white;

    public CurveAnchor(float posX, float posY, EditorValueCurve curve)
    {
        posY = posY / (curve.maxY + curve.minY) * curve.rect.height; 
        posX = posX / (curve.maxX + curve.minX) * curve.rect.width; 

        pos = new Vector2(posX, posY);
        relativePos = new Vector2(pos.x + parentRect.x, pos.y + parentRect.y);
        parentRect = curve.rect;
    }

    public void Draw()
    {
        relativePos = new Vector2(pos.x + parentRect.x, pos.y + parentRect.y);
        EditorGUI.DrawRect(new Rect(relativePos.x - (circleSize / 2), relativePos.y - (circleSize / 2), circleSize, circleSize), drawColor);      
    }
}

class CurveLine
{
    Rect parentRect;
    public CurveLine(Rect parentRect) { this.parentRect = parentRect; }

    public void Draw(Vector2 lineStart, Vector2 lineEnd)
    {
        Handles.BeginGUI();
        Handles.color = Color.white;
        Handles.DrawLine(lineStart + new Vector2(parentRect.x, parentRect.y), lineEnd + new Vector2(parentRect.x, parentRect.y));
        Handles.EndGUI();  
    }
}

