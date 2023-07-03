using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorValueCurve
{
    //draw area
    public Rect rect;

    //value bounds
    public readonly int minX;
    public readonly int maxX;
    public readonly int minY;
    public readonly int maxY;

    //list data
    List<float> values = new List<float>();
    List<CurveAnchor> anchors = new List<CurveAnchor>();
    int anchorCount;

    //current drag anchor
    CurveAnchor dragAnchor;
    
    //drag controllers
    bool hasStartDrag = false;
    bool isDragging = false;

    public EditorValueCurve(Rect rect, int minX, int maxX, int minY, int maxY, int anchorCount)
    {
        //set variables
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        this.anchorCount = anchorCount + 2;
        this.rect = rect;
        isDragging = false;

        //calculate value between min and max
        float middleValue = (maxY - minY) / 2 + minY;

        //initiate and set anchors
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

        //draw anchors and lines
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

    //set value of anchor clamped between min and max
    public void SetAnchorValue(int index, float value) 
    {
        if (value > maxY) value = maxY; 
        if (value < minY) value = minY; 
        if(index <= anchors.Count - 1 && index >= 0) anchors[index].pos.y = (maxY - (value - minY)) / maxY * rect.height; 
    }

    public int GetAnchorCount() { return anchorCount; }
    public int GetValueCount() { return values.Count; }
    public float GetAnchorMargin() { return rect.width / (anchorCount - 1); }

    //get value at index of specified point normalized with scale
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

    //set all values based on anchor positions
    public void SetValuesBasedOnAnchors()
    {
        values.Clear();
        values.Add(anchors[0].pos.y / rect.height * maxY);

        //for every anchor, lerp the correct values between indexes with the difference of next position
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

    //calculate if mouse is outside rect position
    bool MouseIsOutBounds()
    {
        bool value = false;
        Vector2 mousePos = Event.current.mousePosition;
        if (mousePos.x < 0 || mousePos.x > 1040 || mousePos.y < 0 || mousePos.y > 300) value = true;
        return value;
    }

    //move anchor according to mouse position on hold click
    void OnDragAnchors()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 0)
            {
                dragAnchor = new CurveAnchor(666, 666, this);
                float closestDistance = 20;

                //get closest anchor to mouse
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
                //set draganchor if mouse if close enough and not null
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
        //check if drag should stop and (re)set data accordingly
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
        //drag if draganchor is set and is dragging
        if (isDragging && dragAnchor.pos.x != 666)
        {
            DragAnchor(dragAnchor);
        }
    }

    void DragAnchor(CurveAnchor anchor)
    {
        //get range of affected anchors
        int elasticRange = Mathf.CeilToInt(anchorCount / 50);
        int mainAnchorIndex = anchors.IndexOf(anchor);
        anchor.drawColor = Color.green;

        //set drag distance based on mousepos
        float realYPosMouse = Event.current.mousePosition.y - rect.y;
        float dragDist = realYPosMouse - anchor.dragStartY;

        //change positions of affected anchors, based on distance from main anchor while clamping them to the rect
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

//anchor class used for drawing and setting its data
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
        //draw relative to rectpos
        relativePos = new Vector2(pos.x + parentRect.x, pos.y + parentRect.y);
        EditorGUI.DrawRect(new Rect(relativePos.x - (circleSize / 2), relativePos.y - (circleSize / 2), circleSize, circleSize), drawColor);      
    }
}

//line class for drawing lines between anchors
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

