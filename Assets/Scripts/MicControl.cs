using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MicControl : MonoBehaviour
{
    public KeyCode key;
    public Button button;
    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            ActivateMic();
            
        }
        
        if (Input.GetKeyUp(key))
        {
            DeactivateMic();
        }
        
    }

    public void ActivateMic()
    {
        audioSource.volume = 1;
        button.interactable = true;
    }

    public void DeactivateMic()
    {
        audioSource.volume = 0;
        button.interactable = false;
    }
}
