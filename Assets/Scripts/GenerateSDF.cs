using System;
using System.Collections;
using System.Collections.Generic;
using Simplex.Procedures;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class GenerateSDF : MonoBehaviour
{
    
    
    private RenderTexture rt;
    private RenderTexture tempRt;
    
    public ComputeShader binaryImageShader;
    private Camera cam;
    
    [SerializeField] float _sourceValueThreshold = 0.5f;
    [SerializeField] ScalarTextureToSdfTextureProcedure.DownSampling _downSampling = ScalarTextureToSdfTextureProcedure.DownSampling.None;
    [SerializeField] ScalarTextureToSdfTextureProcedure.Precision _precision = ScalarTextureToSdfTextureProcedure.Precision._32;
    [SerializeField] bool _addBorders = false;
    [SerializeField] bool _showSource = false;
    
    [SerializeField] UnityEvent<RenderTexture> _sdfTextureEvent = new UnityEvent<RenderTexture>();
    Simplex.Procedures.ScalarTextureToSdfTextureProcedure sdfGenerator;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        rt = cam.targetTexture;
        
        tempRt = new RenderTexture(rt.width, rt.height, 0, RenderTextureFormat.ARGB32);
        tempRt.enableRandomWrite = true; // Enable UAV usage
        tempRt.Create();

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

    private void Update()
    {

    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        int kernelHandle = binaryImageShader.FindKernel("CSMain");
        
        binaryImageShader.SetTexture(kernelHandle, "InputTexture", src);
        binaryImageShader.SetTexture(kernelHandle, "ResultTexture", tempRt);
        
        uint threadGroupX, threadGroupY, threadGroupZ;
        binaryImageShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupX, out threadGroupY, out threadGroupZ);
        
        binaryImageShader.Dispatch(kernelHandle, src.width / (int)threadGroupX, src.height / (int)threadGroupY, 1);
        
        Graphics.Blit(tempRt, dest);
        
        // 이게 결국 On RenderImage로 옮겨가야함 
        sdfGenerator.Update( tempRt, _sourceValueThreshold, _downSampling, _precision, _addBorders, _showSource );
        _sdfTextureEvent.Invoke(sdfGenerator.sdfTexture );
    }
}
