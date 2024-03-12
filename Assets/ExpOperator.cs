using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExpOperator : MonoBehaviour
{
    private float tElapsed;
    private float tStart;

    public TMP_Text text;
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        ExpRecorder[] expRecorders = FindObjectsOfType<ExpRecorder>();
        foreach(ExpRecorder rec in expRecorders)
        {
            rec.enabled = true;
        }

        tStart = Time.time;
    }

    private void OnDisable()
    {
        ExpRecorder[] expRecorders = FindObjectsOfType<ExpRecorder>();
        foreach(ExpRecorder rec in expRecorders)
        {
            rec.enabled = false;
        }
    }

    private void Update()
    {
        tElapsed = Time.time - tStart;
        text.text = tElapsed.ToString("F1");
    }
}
