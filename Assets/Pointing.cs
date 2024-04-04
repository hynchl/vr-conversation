using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointing : MonoBehaviour
{
    public Transform origin;

    private LineRenderer lr;

    public LayerMask layerMask;
    
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // origin forward 방향으로 
        Debug.DrawRay(origin.position, -origin.right, Color.magenta);
        
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(origin.position, -origin.right, out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(origin.position, -origin.right * hit.distance, Color.yellow);
            lr.SetPosition(0, origin.position);
            lr.SetPosition(1, origin.position -origin.right);
            Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, -origin.right * 1000, Color.white);
            Debug.Log("Did not Hit");
            lr.SetPosition(0, origin.position);
            lr.SetPosition(1,origin.position);
        }
    }
}
