using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicControl : MonoBehaviour
{
    public KeyCode key;
    void Update()
    {
        GetComponent<AudioSource>().enabled = Input.GetKey(key);
    }
}
