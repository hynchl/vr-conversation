using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using System;


public class ConversionManager : MonoBehaviour
{
    public string folderPath = "C:/Users/vclexp1/Videos";
    public GameObject afterConversion;
    public VideoPlayer videoPlayer;
    private string mp4FileName;
    private string latestMkvFile;
    public bool done;
    
    void Start()
    {
        // 폴더 경로
        

        // 최신 mkv 파일 찾기
        latestMkvFile = Directory.GetFiles(folderPath, "*.mkv")
            .OrderByDescending(f => new FileInfo(f).CreationTime)
            .FirstOrDefault();
        UnityEngine.Debug.Log(latestMkvFile);
        // mkv 파일이 존재할 경우 ffmpeg를 이용하여 mp4로 변환
        if (!string.IsNullOrEmpty(latestMkvFile))
        {
            mp4FileName = Path.ChangeExtension(latestMkvFile, ".mov");

            // 프로세스 시작
            Process process = new Process();

            // 프로세스 정보 설정
            process.StartInfo.FileName = "ffmpeg"; // ffmpeg 실행 파일 경로
            process.StartInfo.Arguments = "-i \"" + latestMkvFile + "\" -map 0 -c copy -c:a aac \"" + mp4FileName + "\""; // ffmpeg에 전달할 인자들
            // process.StartInfo.Arguments = "-i \"" + latestMkvFile + "\" -map 0:v:0 -map 0:a:0 -c:v libx264 -c:a aac \"" + mp4FileName + "\""; // ffmpeg에 전달할 인자들
            // process.StartInfo.UseShellExecute = false;
            // process.StartInfo.RedirectStandardOutput = true;
            // process.StartInfo.RedirectStandardError = true;
            UnityEngine.Debug.Log(process.StartInfo.Arguments);
            process.Exited += ProcessExited;
            process.EnableRaisingEvents = true;
            // 프로세스 실행
            process.Start();
        }
        else
        {
            UnityEngine.Debug.LogError("No .mkv file found in the specified folder.");
        }
    }

    private void Update()
    {
        if (done)
        {
            videoPlayer.url = Path.ChangeExtension(latestMkvFile, ".mov");;
            afterConversion.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void ProcessExited(object sender, EventArgs e)
    {
        done = true;
    }
}