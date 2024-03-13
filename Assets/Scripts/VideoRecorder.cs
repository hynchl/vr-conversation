using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class VideoRecorder : MonoBehaviour
{
    public VideoPlayer vp;
    public Slider scoreSocialConnection;
    public Slider scoreSocialPresence;
    
    public float[] scoresSC;
    public bool[] SCsManipulated;
    public float[] scoresSP;
    [FormerlySerializedAs("SPManipulated")] public bool[] SPsManipulated;

    public Recorder recorder;
    [SerializeField]
    string fileName;
    
    public void Start()
    {
        vp = GetComponent<VideoPlayer>();
        recorder = new Recorder("Data/" + fileName + ".tsv");
        vp.Prepare();
        vp.prepareCompleted += OnVideoPrepared;
    }

    public void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log($"frameCount : {vp.frameCount}");


        if (vp.frameCount < 1)
        {
            Debug.LogError("Video Clip is not loaded.");  
        }
        
        scoresSC = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        scoresSP = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        SCsManipulated = new bool[(int)vp.frameCount];
        SPsManipulated = new bool[(int)vp.frameCount];
        
        
    }

    // public void Update()
    // {
    //     Debug.Log($"frameCount : {vp.frameCount}");
    //
    // }


    public void Complete()
    {
        for (int i = 0; i < scoresSC.Length; i++)
        {
            Dictionary<string, string> row = new Dictionary<string, string>();
            row["Frame"] = i.ToString();
            row["TimeStamp"] = (i / vp.frameRate).ToString("F6");
            row["SocialPresence"] = scoresSP[i].ToString("F6");
            row["SocialConnection"] = scoresSC[i].ToString("F6");
            row["SocialPresenceTimePoint"] = SPsManipulated[i].ToString();
            row["SocialConnectionTimePoint"] = SCsManipulated[i].ToString();
            recorder.Add(row);
            
            vp.StepForward();
        }
        
        recorder.Save();
        
        // 종료 화면으로 넘어가면됨
    }
    
    public void Add()
    {
        int currentFrame = (int)vp.frame;
        
        scoresSC[currentFrame] = scoreSocialConnection.value;
        SCsManipulated[currentFrame] = true;
        scoresSP[currentFrame] = scoreSocialPresence.value;
        SPsManipulated[currentFrame] = true;
        
        
        // 뒤로만 전파하도록 구현
        int currentFrame_ = currentFrame+1;
        while (!SCsManipulated[currentFrame_]) {
            scoresSC[currentFrame_] = scoreSocialConnection.value;
            currentFrame_ += 1;
            if (currentFrame_ >= SCsManipulated.Length) break;
        }
        
        // 뒤로만 전파하도록 구현
        currentFrame_ = currentFrame+1; // re-initialize
        
        while (!SPsManipulated[currentFrame_]) {
            scoresSP[currentFrame_] = scoreSocialPresence.value;
            currentFrame_ += 1;
            if (currentFrame_ >= SPsManipulated.Length) break;
        }
        
        // Debug.Log($"Time: {vp.time}, Score ({scoreSocialConnection.value}, {scoreSocialPresence.value})");
        
        vp.Pause();
        // if (!vp.isPlaying)
        // {
        //     scoreSocialConnection.value = scoresSC[currentFrame];
        //     scoreSocialPresence.value = scoresSP[currentFrame]; 
        // }
    }

    public void SetInteractable(bool val)
    {
        scoreSocialPresence.interactable = val;
        scoreSocialConnection.interactable = val;
    }
    
}
