using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video; // Required when using Event data.



public class ScoringHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{

    public VideoPlayer vp;
    public VideoRecorder VideoRecorder;
    
    public void OnPointerEnter (PointerEventData eventData) 
    {
        Debug.Log ("The cursor entered the selectable UI element.");
    }
    
    //Do this when the cursor enters the rect area of this selectable UI object.
    public void OnPointerDown (PointerEventData eventData) 
    {
        Debug.Log (this.gameObject.name + " Was Clicked. (down)");
        vp.Pause();
    }
    
    //Do this when the cursor enters the rect area of this selectable UI object.
    public void OnPointerUp (PointerEventData eventData) 
    {
        Debug.Log (this.gameObject.name + " Was Clicked. (up)");
        VideoRecorder.Add();
        // videoManager = 0;
    }
}
