using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Avatar2;
using UnityEngine;

public class AvatarRecordingHandler : MonoBehaviour
{
    [TextArea]
    public string description;
    public RemoteRecorder remoteRecorder;
    public SelfRecorder selfRecorder;
    
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
        Debug.Log("ADD TO SELF RECORDER");
        
        SampleAvatarEntity sae = GetComponent<SampleAvatarEntity>();
        selfRecorder.tfs.Add(sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Head));
        selfRecorder.tfs.Add(sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Hips));
        selfRecorder.tfs.Add(sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Chest));
        selfRecorder.tfs.Add(sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.LeftHandWrist));
        selfRecorder.tfs.Add(sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightHandWrist));

        // selfRecorder.tfs.Add(transform.Find("Joint Head"));
        // selfRecorder.tfs.Add(transform.Find("Joint Hips"));
        // selfRecorder.tfs.Add(transform.Find("Joint Chest"));
        // selfRecorder.tfs.Add(transform.Find("Joint LeftHandWrist"));
        // selfRecorder.tfs.Add(transform.Find("Joint RightHandWrist"));
    }
}
