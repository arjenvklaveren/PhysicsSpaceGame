using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RingTexture : MonoBehaviour
{
    public List<Color32> texData;
    public Texture2D rawTex;

    public RingTexture()
    {

    }

    public void SetRingTextureData(List<Color32> data)
    {
        texData = data;
    }
    public void SetRingTextureData(Texture2D texture)
    {
        texData.Clear();
        List<Color32> temp = new List<Color32>();
        for (int i = 0; i < texture.width; i++)
        {
            temp.Add(texture.GetPixel(i, 1));
        }
        SetRingTextureData(temp);
    }

    public Texture2D GetRingTextureFromData()
    {
        Texture2D texture = new Texture2D(1000, 1);
        for (int i = 0; i < texData.Count; i++)
        {
            texture.SetPixel(i, 1, texData[i]);
        }
        texture.Apply();
        return texture;
    }
    public List<Color32> GetRingTextureData()
    {
        return texData;
    }
    public int Length()
    {
        return texData.Count;
    }

    public void ResetTex()
    {
        Texture2D resetTex = new Texture2D(1000, 1);
        rawTex = resetTex;
        resetTex.SetPixels(Resources.Load<Texture2D>("Images/ResetRingTex").GetPixels());
        for (int i = 0; i < 1000; i++)
        {
            Color newColor = resetTex.GetPixel(i, 1);
            texData[i] = newColor;
            newColor.a = 1;
            rawTex.SetPixel(i, 1, newColor);
        }
        rawTex.Apply();
    }

    public void SetRawTex(Texture2D rawTex)
    {
        this.rawTex = rawTex;
        this.rawTex.Apply();
    }

    public Texture2D GetRawTex()
    {
        return rawTex;
    }

    public bool IsEmpty()
    {
        if (texData.Count == 0)
        {
            return true;
        }
        return false;
    }
}
