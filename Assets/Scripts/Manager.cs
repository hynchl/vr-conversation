using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    private static Manager instance = null;
    public static Manager Instance
        {
            get
            {
                return instance;
            }
        }
    public GameObject AvatarSdkManager;

    void Awake()
    {
        if(instance)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
}
