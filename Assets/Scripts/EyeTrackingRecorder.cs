using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class EyeTrackingRecorder : ExpRecorder
{
    public EyeTrackingWithSdf[] ets;

    private Recorder recorder;
    [SerializeField]
    string fileName;

    // Start is called before the first frame update
    void Start () {
        ets = FindObjectsOfType<EyeTrackingWithSdf>();
        // recorder = new Recorder("Data/" + fileName + ".tsv");
        recorder = new Recorder("Data/" + GameManager.instance.sessionId + "_" + "eyetracking" + ".tsv");
    }

    // Update is called once per frame
    void LateUpdate () {
        Sample();
    }
    
    public string GetHierarchyPath(GameObject go)
    {
        if (go.transform.parent == null) {
            return go.name;
        } else {
            return GetHierarchyPath(go.transform.parent.gameObject) + "/" + go.name;
        }
    }

    void Sample () {

        string timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        Dictionary<string, object> result = new Dictionary<string, object>();
        result["timestamp"] = timestamp;
        
        foreach (var et in ets)
        {
            result[$"dist/{et.gameObject.name}"] = et.value.ToString("F6");
        }
        
        recorder.Add<object>(result);

    }

    private void OnDisable()
    {
        recorder.Save();
    }
    
    private void OnDestroy()
    {
        recorder.Save();
    }
}
