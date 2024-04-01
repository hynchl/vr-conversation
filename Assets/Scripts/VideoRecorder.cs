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
    public bool isDone;
    
    public Recorder recorder;
    [SerializeField]
    string fileName;

    public float evaluationInterval = 30f; //seconds
    public GameObject evaluationPanel;
    public int currentFrame;
    
    public void Start()
    {
        vp = GetComponent<VideoPlayer>();
        recorder = new Recorder("Data/" + fileName + ".tsv");
        vp.Prepare();
        vp.prepareCompleted += OnVideoPrepared;
    }

    public void Update()
    {
        if (vp.frame == -1) return;
        if (isDone) return;
        
        if (((int)vp.frame >= (int)vp.frameCount - 1))
        {
            vp.Pause();
            evaluationPanel.SetActive(true);
            currentFrame = Mathf.Min(binNumbers.Length-1, (int)vp.frame);
            return;
        }
        
        if ((binNumbers[(int)vp.frame] != binNumbers[(int)vp.frame + 1]) && vp.isPlaying)
        {
            vp.Pause();
            evaluationPanel.SetActive(true);
            currentFrame = (int)vp.frame;
        }
        

    }

    public void Next()
    {
        if ((int)vp.frame + 1 >= (int)vp.frameCount)
        {
            isDone = true;
            return;
        }
        vp.frame += 1;
        // vp.Play();
    }
    public void OnVideoPrepared(VideoPlayer vp)
    {
        if (vp.frameCount < 1)
        {
            Debug.LogError("A video clip is not loaded.");  
        }
        
        scoresSC = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        scoresSP = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        SCsManipulated = new bool[(int)vp.frameCount];
        SPsManipulated = new bool[(int)vp.frameCount];
        binNumbers = new int[(int)vp.frameCount];

        // binning
        for (int i = 0; i < (int)vp.frameCount; i++)
        {
            float currentTime = i / vp.frameRate;
            binNumbers[i] = (int)Mathf.Floor(currentTime / evaluationInterval);
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
    
    public void WriteScoreWithBin()
    {
        Debug.Log($"length: {scoresSC.Length}, current: {currentFrame}");

        int binNumber = binNumbers[currentFrame];
        
        for (int i = 0; i < binNumbers.Length; i++)
        {
            if (binNumbers[i] != binNumber) continue;
            scoresSC[currentFrame] = scoreSocialConnection.value;
            scoresSP[currentFrame] = scoreSocialPresence.value;
        }
        
        vp.Pause();
    }

    public void Add()
    {
        return;
        
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
