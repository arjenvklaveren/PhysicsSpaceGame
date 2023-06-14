using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextureDrawWindow : EditorWindow
{
    static Material alphaMat;
    static Texture2D ringTexture;
    Texture2D prevRingTexture;
    static Texture arrowTexture;
    static TextureDrawWindow window;
    int barWidth = 3;
    static int selectXPos;
    static BodyRings currentRing;
    Color drawColor = Color.red;

    public static void Open(BodyRings ring)
    {
        ringTexture = ring.GetRingTextureFromData();
        arrowTexture = Resources.Load<Texture>("Images/arrow2");
        window = GetWindow<TextureDrawWindow>("Ring editor");
        alphaMat = Resources.Load<Material>("Materials/Alpha");
        window.maxSize = new Vector2(1040f, 500f);
        selectXPos = 500;
        currentRing = ring;
    }

    void OnGUI()
    {
        EditorGUI.LabelField(new Rect(5, 0, 200, 20), "Start");
        EditorGUI.LabelField(new Rect(1010, 0, 200, 20), "End");
        EditorGUI.DrawPreviewTexture(new Rect(20, 20, 1000, 100), ringTexture, alphaMat);
        selectXPos = (int)GUI.HorizontalSlider(new Rect(15, 122, 1010, 10), selectXPos, 0, 1000);
        //EditorGUI.DrawPreviewTexture(new Rect(selectXPos + 20, 120, 23, 29), arrowTexture, alphaMat);

        EditorGUI.LabelField(new Rect(20, 160, 200, 20), "Color");
        drawColor = EditorGUI.ColorField(new Rect(20, 180, 300, 20), drawColor);

        EditorGUI.LabelField(new Rect(400, 160, 200, 20), "Size");
        barWidth = EditorGUI.IntSlider(new Rect(400, 180, 200, 20), barWidth, 3, 99);

        if (barWidth % 2 == 0) barWidth += 1;

        //bar
        int barPos = (selectXPos + 20) - Mathf.FloorToInt(barWidth / 2);

        EditorGUI.DrawRect(new Rect(barPos, 20, barWidth, 100), drawColor);

        if (selectXPos > 1000 - barWidth / 2) selectXPos = 1000 - barWidth / 2;
        if (selectXPos < 0 + barWidth / 2) selectXPos = 0 + barWidth / 2;

        this.Repaint();

        if (Event.current.shift)
        {
            for (int i = 0; i < barWidth; i++)
            {
                ringTexture.SetPixel((selectXPos - Mathf.FloorToInt(barWidth / 2)) + i, 1, drawColor);
            }
            ringTexture.SetPixel(0, 1, new Color(0,0,0,0));
            ringTexture.Apply();
            currentRing.SetRingTextureData(ringTexture);    
            this.Repaint();
        }

        //Temp reset
        if(Event.current.control)
        {
            currentRing.ResetRingTextureData();
            ringTexture = currentRing.GetRingTextureFromData();
            this.Repaint();
        }


    }
}
