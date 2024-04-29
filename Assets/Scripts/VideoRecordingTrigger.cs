using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindowsInput;
using WindowsInput.Native;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;

public class VideoRecordingTrigger : MonoBehaviour
{
    // To excute this script properly, you should need install NugetForUnity and InputSimulatorPlus.NetStandard.
    
    // https://github.com/HavenDV/H.InputSimulator
    // https://gent.tistory.com/97
    
    
    // These keys should be set to the function to trigger in the OBS player.
    public VirtualKeyCode StartRecordKey = VirtualKeyCode.F9;
    public VirtualKeyCode StopRecordKey = VirtualKeyCode.F10;
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassname, string lpWindowName);
    
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    static extern bool AllowSetForegroundWindow(int dwProcessId);
    
    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOWMINIMIZED = 2;
    private const int SW_SHOWMAXIMIZED = 3;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        Process[] p = Process.GetProcessesByName("obs64");
        if (p.Length > 0)
            try
            {
                ShowWindowAsync(p[0].MainWindowHandle, SW_SHOWNORMAL);
                AllowSetForegroundWindow(p[0].Id);
                SetForegroundWindow(p[0].MainWindowHandle);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        
        //https://github.com/TChatzigiannakis/InputSimulatorPlus
        var simulator = new InputSimulator();
        // simulator.Keyboard.KeyDown(StartRecordKey);
        simulator.Keyboard.KeyDown(StartRecordKey).KeyPress(StartRecordKey).Sleep(100).KeyUp(StartRecordKey);
        UnityEngine.Debug.Log("Key Pressed");

        string currentProcessname = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            
        p = Process.GetProcessesByName(currentProcessname);
        if (p.Length > 0)
            try
            {
                ShowWindowAsync(p[0].MainWindowHandle, SW_SHOWNORMAL);
                AllowSetForegroundWindow(p[0].Id);
                SetForegroundWindow(p[0].MainWindowHandle);
                UnityEngine.Debug.Log("comeback");
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }

    }
    
    private void OnDisable()
    {
        Process[] p = Process.GetProcessesByName("obs64");
        if (p.Length > 0)
            try
            {
                ShowWindowAsync(p[0].MainWindowHandle, SW_SHOWNORMAL);
                AllowSetForegroundWindow(p[0].Id);
                SetForegroundWindow(p[0].MainWindowHandle);
                UnityEngine.Debug.Log("yeah");
            }
            catch(Exception ex)
            {
                // Debug.Log(ex.Message);
            }
            
        var simulator = new InputSimulator();
        simulator.Keyboard.KeyDown(StopRecordKey).KeyPress(StopRecordKey).Sleep(1000).KeyUp(StopRecordKey);

        UnityEngine.Debug.Log("Key Pressed");
    }
}
