using UnityEngine;
using UnityEngine.UI;

public class FOVTextureGenerator : MonoBehaviour
{
    public Camera mainCamera;
    public Texture2D texture;
    public int textureWidth = 512;
    public int textureHeight = 512;

    void Start()
    {
        GenerateFOVTexture();
    }

    void GenerateFOVTexture()
    {
        texture = new Texture2D(textureWidth, textureHeight);

        float halfFOV = mainCamera.fieldOfView * 0.5f;
        float aspectRatio = (float)textureWidth / textureHeight;

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                float normalizedX = x / (float)textureWidth;
                float normalizedY = y / (float)textureHeight;

                float angleX = Mathf.Lerp(-halfFOV, halfFOV, normalizedX);
                float angleY = Mathf.Lerp(-halfFOV / aspectRatio, halfFOV / aspectRatio, normalizedY);

                Ray ray = mainCamera.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0));
                Vector3 direction = ray.direction.normalized;

                float angleFromCenter = Vector3.Angle(Vector3.forward, direction);

                Color pixelColor = Color.red * (angleFromCenter / halfFOV);
                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();

        GetComponent<RawImage>().texture = texture;
    }
}