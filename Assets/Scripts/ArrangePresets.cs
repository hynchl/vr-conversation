using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrangePresets : MonoBehaviour
{

    public float circleRadius = 8f;


    void Start()
    {
        int length = transform.childCount;

        for (int i =0; i < length; i++) {

            Transform avatar = transform.GetChild(i);
            // SampleAvatarEntity sae = avatar.GetComponent<SampleAvatarEntity>();
            
            Vector3 pos = Vector3.zero;
            pos.z = Mathf.Sin(i*Mathf.PI*2f/(length)) * circleRadius;
            pos.x = Mathf.Cos(i*Mathf.PI*2f/(length)) * circleRadius;

            avatar.localPosition = pos;
            avatar.forward = avatar.position - transform.position;
            // avatar.LookAt(transform.position, Vector3.up);

            // sae.enabled = false;
        }
    }


}
