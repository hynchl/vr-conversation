
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public string sessionId = "untitled";
    public int selectedAvatar;
    public SampleAvatarEntity[] avatarEntities;
    
    public static GameManager instance = null;

    public GameObject expOperator;

    public GameObject conversationGroup;
    public GameObject preExperimentGroup;
    
    void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            selectedAvatar = instance.selectedAvatar;
        }
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
            entity.ChangePresetAvatar(instance.selectedAvatar.ToString());
        }
    }

    public void SetParticipantID()
    {
        sessionId = GameObject.Find("Text(PID)").GetComponent<TMP_Text>().text;
    }

    public void StartEvaluation()
    {
        SceneManager.LoadScene("4 Video Evaluation");
    }
}
