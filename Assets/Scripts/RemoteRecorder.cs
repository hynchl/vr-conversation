using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RTC;


public class RemoteRecorder : ExpRecorder
{
    public Recorder recorder;
    [SerializeField]
    public string fileName;

    public AvatarRecordingHandler arh;
    
    public AvatarPack avatarpack;
    public List<Transform> tfs;
    // Start is called before the first frame update

    void Start()
    {
        avatarpack = new AvatarPack();
        recorder = new Recorder("Data/" + PlayerPrefs.GetString("Name", "test") + "_" + fileName + ".tsv");
        tfs = new List<Transform>();
        arh.AddToRecorder();
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
        

        foreach (var tf in tfs)
        {
            result[$"POSE.{tf.gameObject.name.Split(" ")[1]}.position.x"] = tf.position.x.ToString("F6"); // global
            result[$"POSE.{tf.gameObject.name.Split(" ")[1]}.position.y"] = tf.position.y.ToString("F6"); // global
            result[$"POSE.{tf.gameObject.name.Split(" ")[1]}.position.z"] = tf.position.z.ToString("F6"); // global
            result[$"POSE.{tf.gameObject.name.Split(" ")[1]}.rotation.x"] = tf.eulerAngles.x.ToString("F6"); // global
            result[$"POSE.{tf.gameObject.name.Split(" ")[1]}.rotation.x"] = tf.eulerAngles.y.ToString("F6"); // global
            result[$"POSE.{tf.gameObject.name.Split(" ")[1]}.rotation.x"] = tf.eulerAngles.z.ToString("F6"); // global
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
            foreach (var pair in avatarpack.eyeData)
            {
                result[pair.Key] = pair.Value.ToString("F6");
            }
        }
        
        result["timestamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

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
