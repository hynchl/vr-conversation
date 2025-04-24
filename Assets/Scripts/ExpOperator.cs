using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExpOperator : MonoBehaviour
{
    private float tElapsed;
    private float tStart;

    public TMP_Text text;

    public Image buttonBackground;
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        ExpRecorder[] expRecorders = FindObjectsOfType<ExpRecorder>();
        foreach(ExpRecorder rec in expRecorders)
        {
            rec.enabled = true;
        }

        buttonBackground.color = Color.red;
        tStart = Time.time;
    }

    private void OnDisable()
    {
        ExpRecorder[] expRecorders = FindObjectsOfType<ExpRecorder>();
        foreach(ExpRecorder rec in expRecorders)
        {
            rec.enabled = false;
        }
        text.text = "Status: Idle";
        buttonBackground.color = Color.gray;

    }


    private void Update()
    {
        tElapsed = Time.time - tStart;
        text.text = "Status: Recording\n" +
                    $"Elapsed Time: {tElapsed:F1}";
    }
}
