using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class TextureDrawWindow : EditorWindow
{
    static TextureDrawWindow window;

    static Material alphaMat;

    static Texture2D ringImage;
    static Texture alphaImage;

    static BodyRings currentRing;

    int barWidth = 3;
    static int selectXPos;
    Color drawColor = Color.red;

    static RingTexture ringTex;

    static Rect drawRect = new Rect(20, 20, 1000, 100);

    static EditorValueCurve alphaCurve = new EditorValueCurve(drawRect, 0, 1000, 0, 255, 99);
    static TestCurveRider curveRider = new TestCurveRider(alphaCurve, 0.05f);

    bool isDrawing = false;
    static Texture2D improvedImage;
    static List<Texture2D> texHistory = new List<Texture2D>();

    [System.Serializable]
    public struct PixelBlock
    {
        public List<Color> pixels;
        public Color color;
        public int startPos;
        public int width;
    }

    public static void Open(RingTexture ringTex_, BodyRings ring)
    {
        window = GetWindow<TextureDrawWindow>("Ring editor");
        window.maxSize = new Vector2(1040f, 500f);
        window.minSize = new Vector2(1040f, 500f);

        ringImage = ringTex_.GetRawTex();
        currentRing = ring;

        improvedImage = ringTex_.GetRingTextureFromData();
        //improvedImage = Resources.Load<Texture2D>("Images/ringStrip");
        alphaImage = Resources.Load<Texture>("Images/alphaBack");      
        alphaMat = Resources.Load<Material>("Materials/Alpha");

        ringTex = ringTex_;

        selectXPos = 500;

        SetAlphaFromTex();
        alphaCurve.SetValuesBasedOnAnchors();

        texHistory.Clear();
        AddTextureToHistory();
        
    }

    void OnGUI()
    {
        EditorGUI.LabelField(new Rect(5, 0, 200, 20), "Start");
        EditorGUI.LabelField(new Rect(1010, 0, 200, 20), "End");
        EditorGUI.DrawRect(drawRect, Color.black);
        EditorGUI.DrawPreviewTexture(drawRect, ringImage, alphaMat);
        selectXPos = (int)GUI.HorizontalSlider(new Rect(15, 122, 1010, 10), selectXPos, 0, 1000);

        EditorGUI.LabelField(new Rect(20, 160, 200, 20), "Draw Color");
        drawColor = EditorGUI.ColorField(new Rect(20, 180, 300, 20), drawColor);
        drawColor.a = 255;

        EditorGUI.LabelField(new Rect(350, 160, 200, 20), "Size");
        barWidth = EditorGUI.IntSlider(new Rect(350, 180, 200, 20), barWidth, 3, 99);

        if (barWidth % 2 == 0) barWidth += 1;

        //bar
        int barPos = (selectXPos + 20) - Mathf.FloorToInt(barWidth / 2);

        EditorGUI.DrawRect(new Rect(barPos, 20, barWidth, 100), drawColor);

        if (selectXPos > 1000 - barWidth / 2) selectXPos = 1000 - barWidth / 2;
        if (selectXPos < 0 + barWidth / 2) selectXPos = 0 + barWidth / 2;

        this.Repaint();

        if (GUI.Button(new Rect(575, 180, 110, 20), new GUIContent("Undo")))
        {
            if (texHistory.Count > 1)
            {
                texHistory.RemoveAt(texHistory.Count - 1);
                ringImage.SetPixels(texHistory[texHistory.Count - 1].GetPixels());
                ringImage.Apply();
                RenderTextureDetail();
            }
        }
        if (GUI.Button(new Rect(700, 180, 110, 20), new GUIContent("Apply")))
        {
            ringTex.SetRingTextureData(improvedImage);
            currentRing.SetPlaneTexture();
        }
        if (GUI.Button(new Rect(825, 180, 110, 20), new GUIContent("Reset")))
        {
            ResetToDefault();
        }
        if (GUI.Button(new Rect(950, 180, 65, 20), new GUIContent("Ringify")))
        {
            RenderTextureDetail();           
        }

        EditorGUI.LabelField(new Rect(5, 340, 200, 20), "Result:");
        EditorGUI.DrawRect(new Rect(20, 365, 1000, 100), Color.black);
        EditorGUI.DrawPreviewTexture(new Rect(20, 365, 1000, 100), alphaImage);
        EditorGUI.DrawPreviewTexture(new Rect(20, 365, 1000, 100), improvedImage, alphaMat);

        alphaCurve.Draw(window);

        //curveRider.RideCurve();

        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.LeftShift)
            {
                isDrawing = true;
            }
        }

        if (isDrawing)
        {
            for (int i = 0; i < barWidth; i++)
            {
                int pixelX = selectXPos - Mathf.FloorToInt(barWidth / 2) + i;
                if (pixelX > 999) continue;
                ringImage.SetPixel(pixelX, 1, drawColor);
            }
            ringImage.Apply();
            ringTex.SetRawTex(ringImage);
            RenderTextureDetail();
            this.Repaint();
        }

        if (e.type == EventType.KeyUp)
        {
            if (e.keyCode == KeyCode.LeftShift)
            {
                isDrawing = false;
                ringImage.Apply();
                AddTextureToHistory();
            }
        }
    }

    static void SetAlphaFromTex()
    {
        for (int i = 0; i < alphaCurve.GetAnchorCount(); i++)
        {
            float samplePos = improvedImage.GetPixel((int)alphaCurve.GetAnchorMargin() * i, 1).a * 255;
            alphaCurve.SetAnchorValue(i, samplePos);
        }
    }
    static void SetAlphaFromCurve()
    {
        //Set alpha
        for (int i = 0; i < 1000; i++)
        {
            Color pixel = improvedImage.GetPixel(i, 1);
            float alphaValue = alphaCurve.GetValueAtPoint(i);
            Color alphaColor = new Color(pixel.r, pixel.g, pixel.b, alphaValue / 255);
            improvedImage.SetPixel(i, 1, alphaColor);
        }
        improvedImage.Apply();
    }
    static void SetAlphaToFull()
    {
        for (int i = 0; i < alphaCurve.GetAnchorCount(); i++)
        {
            alphaCurve.SetAnchorValue(i, 255);
        }
        alphaCurve.SetValuesBasedOnAnchors();
    }

    static void AddTextureToHistory()
    {
        Texture2D texClone = new Texture2D(1000,1);
        texClone.SetPixels(ringImage.GetPixels());
        texClone.Apply();
        texHistory.Add(texClone);
    }

    static void ResetToDefault()
    {
        //Reset raw image
        Texture2D resetTex = new Texture2D(1000, 1);
        resetTex.SetPixels(Resources.Load<Texture2D>("Images/RingStripRaw").GetPixels());
        resetTex.Apply();
        ringTex.SetRawTex(resetTex);
        ringImage = ringTex.GetRawTex();
        ringImage.Apply();

        //Set image data based on raw image
        RenderTextureDetail();
        SetAlphaFromTex();
        AddTextureToHistory();
        selectXPos = 500;
    }

    static void RenderTextureDetail()
    {  

        List<PixelBlock> blocks = new List<PixelBlock>();

        improvedImage.SetPixels(ringImage.GetPixels());
        improvedImage.Apply();

        Color prevPixel = improvedImage.GetPixel(0, 1);
        int prevPos = 0;

        Random.InitState(10);

        //Instantiate pixel blocks
        for (int i = 0; i < 1000; i++)
        {
            Color pixel = improvedImage.GetPixel(i, 1);
            if (pixel != prevPixel || i == 999)
            {
                PixelBlock block = new PixelBlock();
                block.pixels = improvedImage.GetPixels(prevPos, 0, i - prevPos, 1).ToList();
                block.startPos = prevPos;
                block.width = i - prevPos;
                block.color = prevPixel;
                prevPos = i;
                prevPixel = pixel;
                blocks.Add(block);
            }
        }

        //Add block details
        foreach (PixelBlock block in blocks)
        {
            float randomLerp = 0;

            int minWidth = 10;
            int maxWidth = 20;
            int randWidth = Random.Range(block.width / minWidth, block.width / maxWidth);
            int prevI = 0;
            float cx = 0.8f;
            bool loop = false;

            for (int i = 0; i < block.width; i++)
            {
                Color lerpColor = new Color(block.color.r * cx, block.color.g * cx, block.color.b * cx, block.color.a);
                block.pixels[i] = Color.Lerp(block.color, lerpColor, randomLerp);

                if (i >= randWidth + prevI)
                {
                    randomLerp = Random.Range(0.0f, 1.0f);
                    randWidth = Random.Range(block.width / minWidth, block.width / maxWidth);
                    prevI = i;
                }

                if (i == block.width - 1 && !loop)
                {
                    i = 0;
                    prevI = 0;
                    minWidth *= 2;
                    maxWidth *= 2;
                    loop = true;
                }
            }

            improvedImage.SetPixels(block.startPos, 0, block.width, 1, block.pixels.ToArray());
            improvedImage.Apply();
        }
        currentRing.blockTest = blocks;

        SetAlphaFromCurve();

        Texture2D blurredTex = GaussianBlur.Apply(improvedImage, 10, 10);
        improvedImage.SetPixels(blurredTex.GetPixels());
        improvedImage.Apply();
    }
}
