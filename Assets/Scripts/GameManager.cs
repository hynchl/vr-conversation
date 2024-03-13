using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;
using Oculus.Avatar2;
using CAPI = Oculus.Avatar2.CAPI;
using Oculus.Platform;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public int selectedAvatar;
    public string sessionId;
    public SampleAvatarEntity[] avatarEntities;
    
    public static GameManager instance = null;

    public GameObject expOperator;

    public GameObject conversationGroup;
    public GameObject preExperimentGroup;
    
    // Start is called before the first frame update

        void Awake()
        {
            if (null == instance)
            {
                //이 클래스 인스턴스가 탄생했을 때 전역변수 instance에 게임매니저 인스턴스가 담겨있지 않다면, 자신을 넣어준다.
                instance = this;
                
                // DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                selectedAvatar = instance.selectedAvatar;
            }
        // SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                conversationGroup.SetActive(true);
                preExperimentGroup.SetActive(false);
            }

            
        }
        
        public void SetAvatar(int idx)
        {
            instance.selectedAvatar = idx;
            
            foreach (SampleAvatarEntity entity in avatarEntities)
            {
                entity._assets[0] = new SampleAvatarEntity.AssetData()
                {
                    source = OvrAvatarEntity.AssetSource.Zip,
                    path = instance.selectedAvatar.ToString()
                };
            }
        }

    public void SetParticipantID()
    {
        sessionId = GameObject.Find("Text(PID)").GetComponent<TMP_Text>().text;
    }
    
    // public void OpenConversationScene()
    // {
    //     SceneManager.LoadScene("3 Conversation");
    // }

    public void StartEvaluation()
    {
        SceneManager.LoadScene("4 Video Evaluation");
    }
}
