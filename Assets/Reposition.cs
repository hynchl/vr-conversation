using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    public Transform[] itemsToMove;
    public Transform current;
    public Transform target;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // ...
            // OVR Anchor 의 포지션이 타겟 포지션 사이의 차이를 측정해서
            // 그만큼 OVRCameraRig과 local avatar들을 옮겨라
            Vector3 diff = target.position - current.position;
            foreach (Transform tf in itemsToMove)
            {
                tf.position = tf.position + diff;
            }
        }
    }
}
