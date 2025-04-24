using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeScreenPosition : MonoBehaviour
{
    public Vector3 worldPosition;
    public Vector2 screenPosition;
    public Vector3 rayDir;
    public GameObject wall;
    
    private Camera cam;
    private RenderTexture rt;
    public Color color;
    public LayerMask layerMask;
    private void Start()
    {
        cam = GetComponent<Camera>();
        rt = cam.targetTexture;
    }
    
    void Update()
    {


        Ray ray = new Ray(cam.transform.position, rayDir.normalized);

        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
        {
            Debug.Log("Hit object: " + hitInfo.collider.gameObject.name);
            Debug.Log("Hit point: " + hitInfo.point);
        }
        else
        {
            Debug.Log("No object hit.");
        }
        
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
        
        screenPosition = cam.WorldToViewportPoint(hitInfo.point);
        
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        color = tex.GetPixel(Mathf.RoundToInt(screenPosition.x * rt.width), Mathf.RoundToInt(screenPosition.y * rt.height));


    }
}
