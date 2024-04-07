using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubjectiveRating : MonoBehaviour
{
    public Recorder recorder;
    [SerializeField]
    string fileName;

    public Button submitButton;
    
    public Dictionary<string, string> ratings;
    // Start is called before the first frame update
    public void Start()
    {
        fileName = PlayerPrefs.GetString("Name", "Test" + Random.Range(1000, 2000).ToString()) + "_post_conversation";
        recorder = new Recorder("Data/" + fileName + ".tsv");
        ratings = new Dictionary<string, string>();
        ratings["session"] = PlayerPrefs.GetString("Name", "Test"+Random.Range(1000,2000).ToString())+"_post_conversation";
    }

    public void AddData(RatingSliderContent sliderContent)
    {
        string name = sliderContent.variableName;
        ratings[name] = sliderContent.slider.value.ToString("F4");

        if (ratings.Count >= 24) submitButton.interactable = true;

    }

    public void Save()
    {
        if (ratings.Count >= 24)
        {
            recorder.Add(ratings);
            recorder.Save();    
        }

    }
}
