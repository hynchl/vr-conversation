using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
public class AvatarRecorder : ExpRecorder
{
    public List<Transform> tfs;
    // Start is called before the first frame update
    
    private Recorder recorder;
    [SerializeField]
    string fileName;
    public OVRFaceExpressions _faceExpressions;
    
    void Start () {
        // recorder = new Recorder("Data/" + fileName + ".tsv");
        recorder = new Recorder("Data/" + GameManager.instance.sessionId + "_" + "self" + ".tsv");
    }

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
        
        foreach (var tf in tfs)
        {
            result[$"dist/{tf.gameObject.name}/position"] = tf.position.ToString("F6"); // global
            result[$"dist/{tf.gameObject.name}/rotation"] = tf.eulerAngles.ToString("F6"); // global
        }

        // Logging Facial Expression
        
        if (_faceExpressions.FaceTrackingEnabled && _faceExpressions.ValidExpressions)
        {
            for (int i = 0; i < 70; i++)
            {
                OVRFaceExpressions.FaceExpression fe = (OVRFaceExpressions.FaceExpression)i;
                result[fe.ToString()] = _faceExpressions.GetWeight(fe);
            }
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
