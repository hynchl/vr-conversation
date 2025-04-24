using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManageRT : MonoBehaviour
{
    public RenderTexture inputRT;
    public RenderTexture outputRT;
    public Texture2D tex;


    public ComputeShader cs;
    public Mode mode;
    
    public enum Mode
    {
        Loop, ComputeShader
    }
    
    // Start is called before the first frame update
    void Start()
    {
        outputRT = new RenderTexture(inputRT.width, inputRT.height, 0, RenderTextureFormat.ARGB32);
        outputRT.enableRandomWrite = true; // Enable UAV usage
        outputRT.Create();

    }


    private void UsePixelLoop()
    {
        RenderTexture.active = inputRT;
        
        tex.ReadPixels(new Rect(0, 0, inputRT.width, inputRT.height), 0, 0);
        tex.Apply();

        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Check brightness and modify pixel color accordingly
            if (pixels[i].a > 0.0001f)
            {
                pixels[i] = Color.white; // Set bright pixels to white
            }
            else
            {
                pixels[i] = Color.black; // Set dark pixels to black
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        RenderTexture.active = null;
    }

    private void UseComputeShader()
    {
        int kernelHandle = cs.FindKernel("CSMain");

        cs.SetTexture(kernelHandle, "InputTexture", inputRT);
        cs.SetTexture(kernelHandle, "ResultTexture", outputRT);

        uint threadGroupX, threadGroupY, threadGroupZ;
        cs.GetKernelThreadGroupSizes(kernelHandle, out threadGroupX, out threadGroupY, out threadGroupZ);

        cs.Dispatch(kernelHandle, inputRT.width / (int)threadGroupX, inputRT.height / (int)threadGroupY, 1);
        
    }
    
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //
        // You can apply additional image effects here if needed
        int kernelHandle = cs.FindKernel("CSMain");
        
        cs.SetTexture(kernelHandle, "InputTexture", src);
        cs.SetTexture(kernelHandle, "ResultTexture", outputRT);
        
        uint threadGroupX, threadGroupY, threadGroupZ;
        cs.GetKernelThreadGroupSizes(kernelHandle, out threadGroupX, out threadGroupY, out threadGroupZ);
        
        cs.Dispatch(kernelHandle, inputRT.width / (int)threadGroupX, inputRT.height / (int)threadGroupY, 1);
        
        Graphics.Blit(outputRT, dest);
    }
}
