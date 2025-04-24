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
        DoReposition();
    }

    void OnEnable()
    {
        DoReposition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            DoReposition();
        }
    }

    public void DoReposition()
    {
        Vector3 diff = target.position - current.position;
        foreach (Transform tf in itemsToMove)
        {
            tf.position = tf.position + diff;
            Vector3 eulerAngle = tf.eulerAngles;
            eulerAngle.y = target.eulerAngles.y;
            tf.eulerAngles = eulerAngle;
        }
    }

}
