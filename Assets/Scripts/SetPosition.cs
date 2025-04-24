using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPosition : MonoBehaviour
{
    public Transform target;
    float yOffset = 0.2f;

    // Start is called before the first frame update
    void OnEnable()
    {
        Vector3 position = transform.position;
        position.y = target.position.y - yOffset;
        transform.position = position;
    }
}
