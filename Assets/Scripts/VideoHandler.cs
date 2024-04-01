using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoHandler : MonoBehaviour
{
    private VideoPlayer vp;
    public Slider slider;
    public VideoRecorder videoRecorder;
    public bool pressed;
    
    private void Awake()
    {
        vp = GetComponent<VideoPlayer>();
    }

    void Update()
    {
        if (!vp.isPrepared) return;
        
        int currentFrame = (int)vp.frame;
        if (currentFrame == -1) return;
        
        
        slider.value = (float)(vp.time / vp.length);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (vp.isPlaying)
            {
                StopVideo();
            }
            else
            {

                PlayVideo();
            }
        }

        // no review
        // if (vp.isPlaying && videoRecorder != null)
        // {
        //     videoRecorder.scoreSocialConnection.value = videoRecorder.scoresSC[currentFrame];
        //     videoRecorder.scoreSocialPresence.value = videoRecorder.scoresSP[currentFrame]; 
        // }

    }

    public void MoveTo()
    {
        if (!vp.isPrepared || !pressed) return;
        
        int currentFrame = (int)vp.frame;
        if (currentFrame == -1) return;
        if (currentFrame >= (int)vp.frameCount) return;
        
        vp.time = ((float)vp.length) * slider.value;
        
        if (videoRecorder != null)
        {
            // Debug.Log(currentFrame);
            videoRecorder.scoreSocialConnection.value = videoRecorder.scoresSC[currentFrame];
            videoRecorder.scoreSocialPresence.value = videoRecorder.scoresSP[currentFrame]; 
        }
    }

    public void PlayVideo()
    {
        vp.Play();
    }

    public void StopVideo()
    {
        vp.Pause();
    }
    
}
