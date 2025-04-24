using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using System;

public class VideoConverter : MonoBehaviour
{
    public string folderPath = "C:/Users/vclexp1/Videos";
    public GameObject videoGroup;
    public VideoPlayer videoPlayer;
    
    private string movFileName;
    private string latestMkvFile;
    public bool isDone;
    
    void Start()
    {
        // string directoryPath = @"C:\Program Files\obs-studio\bin\64bit";
        // string programPath = @"obs64.exe";
        //
        // ProcessStartInfo startInfo = new ProcessStartInfo
        // {
        //     WorkingDirectory = directoryPath,
        //     FileName = "cmd.exe",
        //     Arguments = $"/c start \"\" \"{programPath}\""
        // };
        //
        // Process.Start(startInfo);


        // Find the latest MKV file.
        latestMkvFile = Directory.GetFiles(folderPath, "*.mkv")
            .OrderByDescending(f => new FileInfo(f).CreationTime)
            .FirstOrDefault();
        UnityEngine.Debug.Log(latestMkvFile);
        
        // Convert to mp4 using ffmpeg if mkv file exists.
        if (!string.IsNullOrEmpty(latestMkvFile))
        {
            movFileName = Path.ChangeExtension(latestMkvFile, ".mov");
            if (File.Exists(movFileName))
            {
                isDone = true;
                return;
            }
            
            Process process = new Process();
            process.StartInfo.FileName = "ffmpeg"; 
            process.StartInfo.Arguments = "-i \"" + latestMkvFile + "\" -map 0 -c copy -c:a aac \"" + movFileName + "\"";
            process.Exited += ProcessExited;
            process.EnableRaisingEvents = true;
            process.Start();
        }
        else
        {
            UnityEngine.Debug.LogError("No .mkv file found in the specified folder.");
        }
    }
    
    
    private void ProcessExited(object sender, EventArgs e)
    {
        isDone = true;
    }
    
    private void Update()
    {
        if (isDone)
        {
            videoGroup.SetActive(true);
            videoPlayer.url = Path.ChangeExtension(latestMkvFile, ".mov");
            gameObject.SetActive(false);
        }
    }


}