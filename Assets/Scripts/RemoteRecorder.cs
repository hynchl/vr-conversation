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

    public AvatarPack avatarpack;
    public List<Transform> tfs;
    // Start is called before the first frame update

    
    // Start is called before the first frame update
    void Start()
    {
        avatarpack = new AvatarPack();
        // recorder = new Recorder("Data/" + fileName + ".tsv");
        recorder = new Recorder("Data/" + GameManager.instance.sessionId + "_" + "remote" + "_" + fileName + ".tsv");
        tfs = new List<Transform>();
    }

    public void SetAvatar(AvatarPack ap)
    {
        avatarpack.isValidFaceExpressions = ap.isValidFaceExpressions;
        avatarpack.faceExpressions = ap.faceExpressions;
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
        
        string timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        result["timestamp"] = timestamp;
        
        foreach (var tf in tfs)
        {
            result[$"dist/{GetHierarchyPath(tf.gameObject)}/position"] = tf.position.ToString("F6"); // global
            result[$"dist/{GetHierarchyPath(tf.gameObject)}/rotation"] = tf.eulerAngles.ToString("F6"); // global
        }
        
        if (avatarpack.isValidFaceExpressions)
        {
            foreach (var pair in avatarpack.faceExpressions)
            {
                result[pair.Key] = pair.Value;
            }
        }

        recorder.Add<object>(result);
    }
}
