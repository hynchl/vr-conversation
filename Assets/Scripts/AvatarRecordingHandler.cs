using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AvatarRecordingHandler : MonoBehaviour
{
    [TextArea]
    public string description;
    public RemoteRecorder remoteRecorder;
    public AvatarRecorder selfRecorder;
    
    public void AddToRecorder()
    {
        remoteRecorder.tfs.Add(transform.Find("Joint Head"));
        remoteRecorder.tfs.Add(transform.Find("Joint Hips"));
        remoteRecorder.tfs.Add(transform.Find("Joint Chest"));
        remoteRecorder.tfs.Add(transform.Find("Joint LeftHandWrist"));
        remoteRecorder.tfs.Add(transform.Find("Joint RightHandWrist"));
        
    }
    
    public void AddToSelfRecorder()
    {
        selfRecorder.tfs.Add(transform.Find("Joint Head"));
        selfRecorder.tfs.Add(transform.Find("Joint Hips"));
        selfRecorder.tfs.Add(transform.Find("Joint Chest"));
        selfRecorder.tfs.Add(transform.Find("Joint LeftHandWrist"));
        selfRecorder.tfs.Add(transform.Find("Joint RightHandWrist"));
        
    }
}
