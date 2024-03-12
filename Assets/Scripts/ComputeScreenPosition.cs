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

        // 카메라 위치에서 마우스 클릭 지점으로 Ray를 쏩니다.
        // 카메라 위치에서 rayDirection 방향으로 Ray를 쏩니다.
        Ray ray = new Ray(cam.transform.position, rayDir.normalized);

        // Raycast 정보를 저장할 변수
        RaycastHit hitInfo;

        // Ray가 충돌한 경우
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
        {
            // 충돌한 대상 오브젝트의 정보 출력
            Debug.Log("Hit object: " + hitInfo.collider.gameObject.name);
            Debug.Log("Hit point: " + hitInfo.point);
        }
        else
        {
            // Ray가 아무 오브젝트와 충돌하지 않은 경우
            Debug.Log("No object hit.");
        }
        
        // Ray를 Debug.DrawRay를 사용하여 그립니다.
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
        
        screenPosition = cam.WorldToViewportPoint(hitInfo.point);
        
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        color = tex.GetPixel(Mathf.RoundToInt(screenPosition.x * rt.width), Mathf.RoundToInt(screenPosition.y * rt.height));


    }
}
