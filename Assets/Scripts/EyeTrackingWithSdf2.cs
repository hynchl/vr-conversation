using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Simplex.Procedures;

[RequireComponent(typeof(Camera))]
public class EyeTrackingWithSdf2 : MonoBehaviour
{
    public enum ImageType
    {
        Rectangle, Spherical
    }

    public ImageType imageType = ImageType.Rectangle;
    private RenderTexture tempRt;
    
    public ComputeShader binaryImageShader;
    
    [SerializeField] float _sourceValueThreshold = 0.5f;
    [SerializeField] ScalarTextureToSdfTextureProcedure.DownSampling _downSampling = ScalarTextureToSdfTextureProcedure.DownSampling.None;
    [SerializeField] ScalarTextureToSdfTextureProcedure.Precision _precision = ScalarTextureToSdfTextureProcedure.Precision._32;
    [SerializeField] bool _addBorders = false;
    [SerializeField] bool _showSource = false;
    
    [SerializeField] UnityEvent<RenderTexture> _sdfTextureEvent = new UnityEvent<RenderTexture>();
    ScalarTextureToSdfTextureProcedure sdfGenerator;

    public ComputeShader cs;
    
    
    public Vector2 screenPosition;
    public GameObject wall;
    
    private Camera cam;
    private RenderTexture rt;
    public RenderTexture rtCubemap;
    public RenderTexture equirect;
    
    public LayerMask layerMask;
    public Color color;
    public Transform eye;
    public float value;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        rt = cam.targetTexture;
        
        tempRt = new RenderTexture(rt.width*2, rt.height, 0, RenderTextureFormat.ARGB32);
        tempRt.enableRandomWrite = true; // Enable UAV usage
        tempRt.Create();

    }
    
    private void Start()
    {
        cam = GetComponent<Camera>();
        rt = cam.targetTexture;
    }
    
    private void OnEnable()
    {
        sdfGenerator?.Release();
        sdfGenerator = new ScalarTextureToSdfTextureProcedure(cs);
    }

    private void OnDisable()
    {
        sdfGenerator?.Release();
    }
    
    private void Reset()
    {
        sdfGenerator?.Release();
    }

    
    void Update()
    {
        // if (imageType == ImageType.Spherical)
        // {
        //
        // }
        
        cam.RenderToCubemap(rtCubemap);
        rtCubemap.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
        rt = equirect;
        
        int kernelHandle = binaryImageShader.FindKernel("CSMain");
        
        binaryImageShader.SetTexture(kernelHandle, "InputTexture", rt);
        binaryImageShader.SetTexture(kernelHandle, "ResultTexture", tempRt);
        
        uint threadGroupX, threadGroupY, threadGroupZ;
        binaryImageShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupX, out threadGroupY, out threadGroupZ);
        
        binaryImageShader.Dispatch(kernelHandle, rt.width / (int)threadGroupX, rt.height / (int)threadGroupY, 1);
        
        // 이게 결국 On RenderImage로 옮겨가야함 
        sdfGenerator.Update( tempRt, _sourceValueThreshold, _downSampling, _precision, _addBorders, _showSource );
        _sdfTextureEvent.Invoke(sdfGenerator.sdfTexture );
        

        // 카메라 위치에서 마우스 클릭 지점으로 Ray를 쏩니다.
        // 카메라 위치에서 rayDirection 방향으로 Ray를 쏩니다.
        Ray ray = new Ray(cam.transform.position, eye.forward);

        // Raycast 정보를 저장할 변수
        RaycastHit hitInfo;

        // Ray가 충돌한 경우
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
        {
            // 충돌한 대상 오브젝트의 정보 출력
            // Debug.Log("Hit object: " + hitInfo.collider.gameObject.name);
            // Debug.Log("Hit point: " + hitInfo.point);
        }
        
        // Ray를 Debug.DrawRay를 사용하여 그립니다.
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
        
        screenPosition = cam.WorldToViewportPoint(hitInfo.point);

        RenderTexture current = RenderTexture.active;
        
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        color = tex.GetPixel(Mathf.RoundToInt(screenPosition.x * rt.width), Mathf.RoundToInt(screenPosition.y * rt.height));

        // RenderTexture.active = current;
        value = color.r;
    }


    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
Graphics.Blit(src, dest);
        // Graphics.Blit(sdfGenerator.sdfTexture, dest);
    }
}
