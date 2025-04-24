using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(OVRHand))]
[RequireComponent(typeof(LineRenderer))]
public class SelectAvatar : MonoBehaviour
{
    OVRHand hand;
    public bool isLeftHand;
    LineRenderer lr;

    int selectedAvatarIndex = -1;

    public LayerMask layerMask;
    public bool isPinched = false;
    public GameObject Presets;
    private Vector3 rayDirection;
    // Start is called before the first frame update
    void Start()
    {
        hand = GetComponent<OVRHand>();    
        lr = GetComponent<LineRenderer>();

        rayDirection = isLeftHand? Vector3.right:Vector3.left;
    }

    // Update is called once per frame
    void Update()
    {
        


        // Check Pinch and trigger pinch events
        if (hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > 0.9f && hand.IsPointerPoseValid) {
            if (!isPinched)
                OnPinchStart();

            isPinched = true;
        } else {
            if (isPinched)
                OnPinchEnd();
            isPinched = false;
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(rayDirection), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(rayDirection) * hit.distance, Color.yellow);
            
            Debug.Log(hit.collider.gameObject.name);
            Debug.Log(hit.collider.gameObject.name.Split(' '));
            selectedAvatarIndex = int.Parse(hit.collider.gameObject.name.Split(' ')[1]);

            // Update linerenderer
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hit.point);
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(rayDirection) * 1000, Color.white);

            selectedAvatarIndex = -1;

            // Update linerenderer
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position + transform.TransformDirection(rayDirection));
        }
    }

    void OnPinchStart() {
        if (selectedAvatarIndex >= 0) {
            Debug.Log($"Pinch Start {selectedAvatarIndex}");
            // AvatarSelf.GetComponent<InitializeAvatar>().SetId(selectedAvatarIndex);
            // AvatarSelf.GetComponent<InitializeAvatar>().enabled = true;
            // AvatarSelf.SetActive(true);
            // Presets.SetActive(false);
        }
    }

    void OnPinchEnd() {
        if (selectedAvatarIndex >= 0) {
            Debug.Log($"Pinch End {selectedAvatarIndex}");
            SceneManager.LoadScene("V5");
            GameManager.instance.avatarIndex = selectedAvatarIndex;
            SceneManager.UnloadSceneAsync("Introduction");
        }

    }
}
