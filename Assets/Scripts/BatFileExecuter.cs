using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public class BatFileExecutor : MonoBehaviour
{
    public string batFilePath =  @"Path\to\your\file.bat";
    public string workingDirectory = @"Path\to\bat\file\directory";
    public int portToCheck = 5001;
    
    void Start()
    {
        if (!IsPortOpen(portToCheck))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = batFilePath,
                WorkingDirectory = workingDirectory
            };

            Process.Start(startInfo);
        }
        else
        {
            UnityEngine.Debug.Log("Already your socket server is open.");
        }
    }
    
    bool IsPortOpen(int port)
    {
        try
        {
            // Attempt to connect to the specified port
            TcpClient client = new TcpClient("localhost", port);
            client.Close();
            return true;
        }
        catch (SocketException)
        {
            // Port is not open or server is not running
            return false;
        }
    }
}