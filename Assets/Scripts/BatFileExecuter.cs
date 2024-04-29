using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class BatFileExecutor : MonoBehaviour
{
    public string batFilePath =  @"Path\to\your\file.bat";
    public string workingDirectory = @"Path\to\bat\file\directory";

    void Start()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = batFilePath,
            WorkingDirectory = workingDirectory
        };

        Process.Start(startInfo);
    }
}