using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Video; // Required when using Event data.



public class ProgressBarHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{

    public VideoPlayer vp;
    public VideoRecorder videoRecorder;
    public VideoHandler videoHandler;
    public void OnPointerEnter (PointerEventData eventData) 
    {
        Debug.Log ("The cursor entered the selectable UI element.");
    }
    
    //Do this when the cursor enters the rect area of this selectable UI object.
    public void OnPointerDown (PointerEventData eventData) 
    {
        Debug.Log (this.gameObject.name + " Was Clicked. (down)");
        vp.Pause();
        videoHandler.pressed = true;
    }
    
    //Do this when the cursor enters the rect area of this selectable UI object.
    public void OnPointerUp (PointerEventData eventData) 
    {
        Debug.Log (this.gameObject.name + " Was Clicked. (up)");
        videoRecorder.Add();
        videoHandler.pressed = false;
        // videoManager = 0;
    }
}