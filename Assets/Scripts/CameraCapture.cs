using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    public Camera cameraToCapture; // Reference to the camera you want to capture from
    public string savePath = "Assets/Screenshots/"; // Path to save the captured image

    void Update()
    {
        // Check if Space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Capture the screenshot from the camera
            CaptureScreenshot();
        }
    }

    void CaptureScreenshot()
    {
        // Ensure the camera to capture from is not null
        if (cameraToCapture != null)
        {
            // Create a new texture with the size of the screen
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

            // Set the target texture of the camera to the created render texture
            cameraToCapture.targetTexture = renderTexture;

            // Create a new Texture2D with the size of the screen
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            // Render the camera's view into the texture
            cameraToCapture.Render();

            // Read the pixels from the render texture into the texture
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            // Reset the camera's target texture
            cameraToCapture.targetTexture = null;
            RenderTexture.active = null;

            // Encode the texture into a PNG
            byte[] bytes = texture.EncodeToPNG();

            // Define the filename with a timestamp
            string filename = savePath + "screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            // Write the PNG data to a file
            System.IO.File.WriteAllBytes(filename, bytes);

            // Output a message to the console
            Debug.Log("Screenshot saved to: " + filename);
        }
        else
        {
            // Output a warning if cameraToCapture is null
            Debug.LogWarning("Camera to capture from is not assigned!");
        }
    }
}
