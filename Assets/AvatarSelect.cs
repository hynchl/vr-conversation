using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 1
using UnityEngine.SceneManagement;

public class AvatarSelect : MonoBehaviour,IPointerClickHandler
{
    public GameObject cursor;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        /*
        ps.haveRiffle = true;
        ps.haveGun = false;
        ps.haveBat = false;
        ps.haveFlamethrower = false;
       // ps.haveKnife = false;
        */
        Debug.Log("Click DETECTED ON RIFFLE IMAGE");
        cursor.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;
        GameManager.instance.selectedAvatar = int.Parse(gameObject.name);
    }


}
