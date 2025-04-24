using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Simplex.Procedures;

[RequireComponent(typeof(Camera))]
public class EyeTrackingWithSdf : MonoBehaviour
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


    private Camera cam;
    private RenderTexture rt;
    private RenderTexture rtCubemap;
    

    public float value;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        rt = cam.targetTexture;
        
        tempRt = new RenderTexture(rt.width, rt.height, 0, RenderTextureFormat.ARGB32);
        tempRt.enableRandomWrite = true;
        tempRt.Create();
        value = 1f;
    }
    
    private void Start()
    {
        cam = GetComponent<Camera>();
        rt = cam.targetTexture;
    }
    
    private void OnEnable()
    {
        sdfGenerator?.Release();
        sdfGenerator = new ScalarTextureToSdfTextureProcedure();
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
        
        int kernelHandle = binaryImageShader.FindKernel("CSMain");
        
        binaryImageShader.SetTexture(kernelHandle, "InputTexture", rt);
        binaryImageShader.SetTexture(kernelHandle, "ResultTexture", tempRt);
        
        uint threadGroupX, threadGroupY, threadGroupZ;
        binaryImageShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupX, out threadGroupY, out threadGroupZ);
        
        binaryImageShader.Dispatch(kernelHandle, rt.width / (int)threadGroupX, rt.height / (int)threadGroupY, 1);
        

        sdfGenerator.Update( tempRt, _sourceValueThreshold, _downSampling, _precision, _addBorders, _showSource );
        _sdfTextureEvent.Invoke(sdfGenerator.sdfTexture );


        RenderTexture current = RenderTexture.active;
        
        RenderTexture.active = sdfGenerator.sdfTexture;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        value = tex.GetPixel(rt.width/2, rt.height/2).r; //get center value, center is the direction of gaze.
        
        RenderTexture.active = current;
        Destroy(tex);
        tex = null;

    }

    private void LateUpdate()
    {

    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
    }
}
