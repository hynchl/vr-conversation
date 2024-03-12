using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetTexturePixel : MonoBehaviour
{
    public RawImage rawImage;

    private void Update()
    {
        FindMinMaxR(GetComponent<RawImage>().texture as RenderTexture);
    }

    void FindMinMaxR(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        Color[] pixels = tex.GetPixels();

        float minR = float.MaxValue;
        float maxR = float.MinValue;

        foreach (Color pixel in pixels)
        {
            if (pixel.r < minR)
                minR = pixel.r;
            if (pixel.r > maxR)
                maxR = pixel.r;
        }

        Debug.Log("Min R Value: " + minR);
        Debug.Log("Max R Value: " + maxR);
    }
}
