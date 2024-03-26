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
    public bool[] SPsManipulated;
    public int[] binNumbers;
    
    public Recorder recorder;
    [SerializeField]
    string fileName;

    public float evaluationInterval = 30f; //seconds
    
    public void Start()
    {
        vp = GetComponent<VideoPlayer>();
        recorder = new Recorder("Data/" + fileName + ".tsv");
        vp.Prepare();
        vp.prepareCompleted += OnVideoPrepared;
    }

    public void Update()
    {
        if ((binNumbers[(int)vp.frame] != binNumbers[(int)vp.frame + 1]) && vp.isPlaying)
        {
            // 다음 프레임부터 뭔가 바뀔 때
        }
        // 만약 지금 초가 n-1에서 n으로 바뀌면 평가창이 뜬다.
        // 그러지 말고 프레임을 이미 나눠놓을까? 빈 인덱스로
        // 지금 bin number가 바뀌었으면, 일단 정지. 
        // UI창을 띄워서 '평가하세요. 끝나면 버튼 혹은 엔터를 눌러 다음 구간으 평가하세요'
        
    }

    public void OnVideoPrepared(VideoPlayer vp)
    {
        if (vp.frameCount < 1)
        {
            Debug.LogError("Video Clip is not loaded.");  
        }
        
        scoresSC = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        scoresSP = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        SCsManipulated = new bool[(int)vp.frameCount];
        SPsManipulated = new bool[(int)vp.frameCount];
        binNumbers = new int[(int)vp.frameCount];

        // binning
        float totalTime = (float)vp.frameCount / vp.frameRate;
        for (int i = 0; i < (int)vp.frameCount; i++)
        {
            float currentTime = i / vp.frameRate;
            binNumbers[i] = (int)Mathf.Floor(currentTime / totalTime);
        }
    }
    
    

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
    }
    
    public void Add()
    {
        int currentFrame = (int)vp.frame;
        Debug.Log($"length: {scoresSC.Length}, current: {currentFrame}");
        
        scoresSC[currentFrame] = scoreSocialConnection.value;
        SCsManipulated[currentFrame] = true;
        scoresSP[currentFrame] = scoreSocialPresence.value;
        SPsManipulated[currentFrame] = true;
        
        
        if (currentFrame + 1 >= scoresSC.Length) return;
        
        int currentFrame_ = currentFrame+1;
        while (!SCsManipulated[currentFrame_]) {
            scoresSC[currentFrame_] = scoreSocialConnection.value;
            currentFrame_ += 1;
            if (currentFrame_ >= SCsManipulated.Length) break;
        }
        
        currentFrame_ = currentFrame+1; // re-initialize
        while (!SPsManipulated[currentFrame_]) {
            scoresSP[currentFrame_] = scoreSocialPresence.value;
            currentFrame_ += 1;
            if (currentFrame_ >= SPsManipulated.Length) break;
        }
        
        vp.Pause();
    }

    public void Add()
    {
        int currentFrame = (int)vp.frame;
        Debug.Log($"length: {scoresSC.Length}, current: {currentFrame}");
        
        scoresSC[currentFrame] = scoreSocialConnection.value;
        SCsManipulated[currentFrame] = true;
        scoresSP[currentFrame] = scoreSocialPresence.value;
        SPsManipulated[currentFrame] = true;
        
        
        if (currentFrame + 1 >= scoresSC.Length) return;
        
        int currentFrame_ = currentFrame+1;
        while (!SCsManipulated[currentFrame_]) {
            scoresSC[currentFrame_] = scoreSocialConnection.value;
            currentFrame_ += 1;
            if (currentFrame_ >= SCsManipulated.Length) break;
        }
        
        currentFrame_ = currentFrame+1; // re-initialize
        while (!SPsManipulated[currentFrame_]) {
            scoresSP[currentFrame_] = scoreSocialPresence.value;
            currentFrame_ += 1;
            if (currentFrame_ >= SPsManipulated.Length) break;
        }
        
        vp.Pause();
    }

    
    public void SetInteractable(bool val)
    {
        scoreSocialPresence.interactable = val;
        scoreSocialConnection.interactable = val;
    }
    
}
