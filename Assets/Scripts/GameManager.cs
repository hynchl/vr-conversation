
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindowsInput;
using WindowsInput.Native;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;

public class GameManager : MonoBehaviour
{
        
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    static extern bool AllowSetForegroundWindow(int dwProcessId);
    

    public string sessionId = "untitled";
    [FormerlySerializedAs("selectedAvatar")] public int avatarIndex;
    public SampleAvatarEntity[] avatarEntities;
    
    public static GameManager instance = null;
    public GameObject expOperator;
    public GameObject[] preConversationObjects;
    public GameObject[] inConversationObjects;
    public RTC.Client3 client;
    
    public bool useOBS = true;
    void Awake()
    {

        // Start OBS if it is not open
        string currentProcessname = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        if (useOBS)
        {
            if (!IsProcessRunning("obs64"))
            {
                StartOBS();
            }
            else
            {
                UnityEngine.Debug.Log("Program is already running.");
            }
        }


            
        Process[] p = Process.GetProcessesByName(currentProcessname);
        if (p.Length > 0)
            try
            {
                ShowWindowAsync(p[0].MainWindowHandle, 1);
                AllowSetForegroundWindow(p[0].Id);
                SetForegroundWindow(p[0].MainWindowHandle);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        

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

    public void StartOBS()
    {
        string directoryPath = @"C:\Program Files\obs-studio\bin\64bit";
        string programPath = @"obs64.exe";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            WorkingDirectory = directoryPath,
            FileName = "cmd.exe",
            Arguments = $"/c start \"\" \"{programPath}\""
        };

        Process.Start(startInfo);

    }

    private bool IsProcessRunning(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0;
    }
}
