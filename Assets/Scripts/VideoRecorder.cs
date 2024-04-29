using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class VideoRecorder : MonoBehaviour
{
    public VideoPlayer vp;
    public Slider scoreSocialConnection;
    public Slider scoreSocialPresence;
    public GameObject finishInfo;
    public bool useCheckBinTransiion = true;
    
    public float[] scoresSC;
    public bool[] SCsManipulated;
    // public float[] scoresSP;
    // public bool[] SPsManipulated;
    public int[] binNumbers;
    public bool isDone;
    public long lastStartFrame;
    
    
    [FormerlySerializedAs("isLastSection")] public bool isLastBin;
    public Recorder recorder;
    [SerializeField]
    string fileName;

    public float evaluationInterval = 30f; //seconds
    public GameObject evaluationPanel;
    public int currentFrame;
    
    public void Start()
    {
        vp = GetComponent<VideoPlayer>();
        fileName = PlayerPrefs.GetString("Name", "test") + "_video_measure";
        recorder = new Recorder("Data/" + fileName + ".tsv");
        vp.Prepare();
        vp.prepareCompleted += OnVideoPrepared;
    }

    public void Update()
    {
        if (vp.frame == -1) return;
        if (isDone) return;
        
        if (((int)vp.frame >= (int)binNumbers.Length - 1))
        {
            vp.Pause();
            evaluationPanel.SetActive(true);
            currentFrame = Mathf.Min(binNumbers.Length-1, (int)vp.frame);
            isLastBin = true;
            return;
        }

        if (!useCheckBinTransiion) return;
        
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
        lastStartFrame = vp.frame;
        
        StartCoroutine(PlayVideoPlayer());
        StartCoroutine(SkipBinCheck());
    }
    
    public void Replay()
    {
        vp.frame = lastStartFrame;
        StartCoroutine(PlayVideoPlayer());
        StartCoroutine(SkipBinCheck());
    }


    
    public void OnVideoPrepared(VideoPlayer vp)
    {
        if (vp.frameCount < 1)
        {
            Debug.LogError("A video clip is not loaded.");  
        }
        
        scoresSC = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        // scoresSP = Enumerable.Repeat(-1f, (int)vp.frameCount).ToArray();
        SCsManipulated = new bool[(int)vp.frameCount];
        // SPsManipulated = new bool[(int)vp.frameCount];
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
            // row["SocialPresence"] = scoresSP[i].ToString("F6");
            row["SocialConnection"] = scoresSC[i].ToString("F6");
            recorder.Add(row);
            
            finishInfo.gameObject.SetActive(true);
            transform.parent.gameObject.SetActive(true);
        }
        
        recorder.Save();
        
        Debug.Log("Successfully saved.");
    }

    public IEnumerator PlayVideoPlayer()
    {
        yield return new WaitForSeconds(0.5f);
        vp.Play();
    }
    
    public IEnumerator SkipBinCheck()
    {
        useCheckBinTransiion = false;
        yield return new WaitForSeconds(1f);
        useCheckBinTransiion = true;
    }
    
    public void WriteScoreWithBin()
    {

        int binNumber = binNumbers[currentFrame];
        
        Debug.Log($"length: {scoresSC.Length}, current: {currentFrame}, bin number: {binNumber}");
        for (int i = 0; i < binNumbers.Length; i++)
        {
            if (binNumbers[i] != binNumber) continue;
            scoresSC[i] = scoreSocialConnection.value;
            // scoresSP[i] = scoreSocialPresence.value;
        }
        
    }

    public void CheckCompletion()
    {
        if (isLastBin)
        {
            Complete();
            transform.parent.gameObject.SetActive(false);
        }
    }
    public void Add()
    {
        return;
        
        int currentFrame = (int)vp.frame;
        Debug.Log($"length: {scoresSC.Length}, current: {currentFrame}");
        
        scoresSC[currentFrame] = scoreSocialConnection.value;
        SCsManipulated[currentFrame] = true;
        // scoresSP[currentFrame] = scoreSocialPresence.value;
        // SPsManipulated[currentFrame] = true;
        
        
        if (currentFrame + 1 >= scoresSC.Length) return;
        
        int currentFrame_ = currentFrame+1;
        while (!SCsManipulated[currentFrame_]) {
            scoresSC[currentFrame_] = scoreSocialConnection.value;
            currentFrame_ += 1;
            if (currentFrame_ >= SCsManipulated.Length) break;
        }
        
        // currentFrame_ = currentFrame+1; // re-initialize
        // while (!SPsManipulated[currentFrame_]) {
        //     scoresSP[currentFrame_] = scoreSocialPresence.value;
        //     currentFrame_ += 1;
        //     if (currentFrame_ >= SPsManipulated.Length) break;
        // }
        
        vp.Pause();
    }

    public void SetScores()
    {
        // return;
        
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
