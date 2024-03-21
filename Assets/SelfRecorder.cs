using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RTC;

public class SelfRecorder : ExpRecorder
{
    public Recorder recorder;
    [SerializeField]
    public string fileName;

    public AvatarRecordingHandler arh;
    
    public AvatarPack avatarpack;
    public List<Transform> tfs;

    public Dictionary<string, Transform> joints;
    
    // Start is called before the first frame update
    public TMPro.TMP_Text text;
    
    void Start()
    {
        avatarpack = new AvatarPack();
        recorder = new Recorder("Data/" + GameManager.instance.sessionId + "_" + fileName + ".tsv");
        tfs = new List<Transform>();
        joints = new Dictionary<string, Transform>();
        // arh.AddToSelfRecorder();
    }

    public void UpdateRemoteInfo(AvatarPack ap)
    {
        if (!this.enabled) return;
        
        avatarpack.isValidFaceExpressions = ap.isValidFaceExpressions;
        avatarpack.faceExpressions = ap.faceExpressions;
        avatarpack.eyeData = ap.eyeData;
    }
    
    public string GetHierarchyPath(GameObject go)
    {
        if (go.transform.parent == null) {
            return go.name;
        } else {
            return GetHierarchyPath(go.transform.parent.gameObject) + "/" + go.name;
        }
    }
    

    void LateUpdate()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result["POSE.timestamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        foreach (KeyValuePair<string, Transform> pair in joints)
        {
            result[$"POSE.{pair.Key}.position.x"] = pair.Value.position.x.ToString("F6"); // global
            result[$"POSE.{pair.Key}.position.y"] = pair.Value.position.y.ToString("F6"); // global
            result[$"POSE.{pair.Key}.position.z"] = pair.Value.position.z.ToString("F6"); // global
            result[$"POSE.{pair.Key}.rotation.x"] = pair.Value.eulerAngles.x.ToString("F6"); // global
            result[$"POSE.{pair.Key}.rotation.y"] = pair.Value.eulerAngles.y.ToString("F6"); // global
            result[$"POSE.{pair.Key}.rotation.z"] = pair.Value.eulerAngles.z.ToString("F6"); // global
        }
        
        if (avatarpack.isValidFaceExpressions)
        {
            foreach (var pair in avatarpack.faceExpressions)
            {
                result[pair.Key] = pair.Value.ToString("F6");
            }
        }

        if (avatarpack.eyeData != null)
        {
            Debug.Log(avatarpack.eyeData.Count);
            foreach (var pair in avatarpack.eyeData)
            {
                result[pair.Key] = pair.Value.ToString("F6");
            }
        }
        
        

        recorder.Add<object>(result);
    }
  
    private void OnDisable()
    {
        if (recorder != null)
            recorder.Save();
    }
    
    private void OnDestroy()
    {
        if (recorder != null)
            recorder.Save();
    }
}
