using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSelection : MonoBehaviour
{
    
    public void UpdateAvatarSelection(int num)
    {
        GameManager.instance.SetAvatar(num);
    }
}
