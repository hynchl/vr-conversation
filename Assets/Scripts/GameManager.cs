
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public string sessionId = "untitled";
    [FormerlySerializedAs("selectedAvatar")] public int avatarIndex;
    public SampleAvatarEntity[] avatarEntities;
    
    public static GameManager instance = null;

    public GameObject expOperator;
    public GameObject[] preConversationObjects;
    public GameObject[] inConversationObjects;
    public RTC.Client3 client;
    
    void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            avatarIndex = instance.avatarIndex;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F2))
        {
            ToConversation();
        }
        
    }

    public void ToConversation()
    {
        if (avatarIndex == -1) return;
        
        // conversationGroup.SetActive(true);
        // preExperimentGroup.SetActive(false);
       
        foreach (GameObject go in inConversationObjects)
        {
            go.SetActive(true);
        }
        
        foreach (GameObject go in preConversationObjects)
        {
            go.SetActive(false);
        }

        Camera.main.clearFlags = CameraClearFlags.Skybox;
    }
        
    public void SetAvatar(int idx)
    {
        instance.avatarIndex = idx;
        this.avatarIndex = idx;
        
        foreach (SampleAvatarEntity entity in avatarEntities)
        {
            entity.ChangePresetAvatar(instance.avatarIndex.ToString());
        }
    }

    public void SetSignalingServerAddress()
    {
        string address = GameObject.Find("InputField (TMP) : Signal Server Address").GetComponent<TMP_InputField>().text;
        client.socketUri = address;
    }

    public void SetSessionID()
    {
        sessionId = GameObject.Find("InputField (TMP) : Session ID").GetComponent<TMP_InputField>().text;
        PlayerPrefs.SetString("Name", sessionId);
    }

    public void StartEvaluation()
    {
        SceneManager.LoadScene("2 PostConversation");
    }
}
