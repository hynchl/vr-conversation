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
        // remote에도 적용해야지
        remoteRecorder.tfs.Add(transform.Find("Joint Head"));
        remoteRecorder.tfs.Add(transform.Find("Joint Hips"));
        remoteRecorder.tfs.Add(transform.Find("Joint Chest"));
        remoteRecorder.tfs.Add(transform.Find("Joint LeftHandWrist"));
        remoteRecorder.tfs.Add(transform.Find("Joint RightHandWrist"));
        
    }

    public void AddToSelfRecorder()
    {
        SampleAvatarEntity sae = GetComponent<SampleAvatarEntity>();
        if (selfRecorder.joints == null)
        {
            selfRecorder.joints = new Dictionary<string, Transform>();
        }

        selfRecorder.joints = new Dictionary<string, Transform>();
        selfRecorder.joints["Head"] = sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Head);
        selfRecorder.joints["Hips"] = sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Hips);
        selfRecorder.joints["Chest"] = sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Chest);
        selfRecorder.joints["LeftHandWrist"] = sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.LeftHandWrist);
        selfRecorder.joints["RightHandWrist"] = sae.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightHandWrist);

    }
}
