using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RatingSliderContent : MonoBehaviour
{
    public enum Language
    {
        ENGLISH, KOREAN
    }

    public Language language = Language.KOREAN;
    public Slider slider;
    public string variableName;
    [FormerlySerializedAs("onValueChange")] public UnityEvent onValueChanged;
    
    [Header("Question")]
    [TextArea(2,5)]
    public string questionKor;
    [TextArea(2,5)]
    public string questionEng;
    public TMP_Text questionTextComp;
    
    [Header("Lower Extreme Label")]
    [TextArea(2,5)]
    public string lowerExtremeLabelKor;
    [TextArea(2,5)]
    public string lowerExtremeLabelEng;
    public TMP_Text lowerExtremeLabelComp;
    
    [Header("Moderate Label")]
    [TextArea(2,5)]
    public string moderateLabelKor;
    [TextArea(2,5)]
    public string moderateLabelEng;
    public TMP_Text moderateLabelComp;
    
    [Header("High Extreme Label")]
    [TextArea(2,5)]
    public string upperExtremeLabelKor;
    [TextArea(2,5)]
    public string upperExtremeLabelEng;
    public TMP_Text upperExtremeLabelComp;

    private void Awake()
    {
        
        if (language == Language.KOREAN)
        {
            SetTextWithKorean();
        }
        else
        {
            SetTextWithEnglish();
        }
    }

    public void OnValueChanged()
    {
        onValueChanged.Invoke();
    }

    private void SetTextWithKorean()
    {
        questionTextComp.text = questionKor;
        lowerExtremeLabelComp.text = lowerExtremeLabelKor;
        moderateLabelComp.text = moderateLabelKor;
        upperExtremeLabelComp.text = upperExtremeLabelKor;
    }

    private void SetTextWithEnglish()
    {
        questionTextComp.text = questionEng;
        lowerExtremeLabelComp.text = lowerExtremeLabelEng;
        moderateLabelComp.text = moderateLabelEng;
        upperExtremeLabelComp.text = upperExtremeLabelEng;
    }
}
