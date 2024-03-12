using UnityEngine;
using UnityEngine.UI;

public class ModifyBrightness: MonoBehaviour
{
    public Camera renderCamera;
    // public Material materialToRender;

    public RenderTexture renderTexture;
    public RawImage ri;
    
    private void Start()
    {
        // Create a render texture
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        // Set the camera's target texture to the render texture
        renderCamera.targetTexture = renderTexture;

        // Assign the render texture to the material's main texture
        // materialToRender.mainTexture = renderTexture;
    }

    private void Update()
    {
        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Check brightness and modify pixel color accordingly
            if (pixels[i].grayscale > 0.5f) // Check brightness (0.0 is black, 1.0 is white)
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

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // You can apply additional image effects here if needed
        Graphics.Blit(src, dest);
    }
}