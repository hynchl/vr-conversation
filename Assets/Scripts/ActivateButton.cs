using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivateButton : MonoBehaviour
{
    public Button button;
    public Scrollbar scrollbar;


    // Start is called before the first frame update
    void Start()
    {
        scrollbar = GetComponent<Scrollbar>();
    }

    // Update is called once per frame
    public void OnValueChanged()
    {
        if (scrollbar.value < 0.01) button.interactable = true;
    }
}
