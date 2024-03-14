using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRemotePosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position = GameObject.Find("RemotePosition").transform.position;
        transform.rotation = GameObject.Find("RemotePosition").transform.rotation;
    }
    
    
}
