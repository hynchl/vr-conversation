using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextHandler : MonoBehaviour
{
    private TMP_Text text;
    public Slider slider;
    
    public void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    public void UpdateSliderValue()
    {
        text.text = slider.value.ToString("F1");
    }
}
