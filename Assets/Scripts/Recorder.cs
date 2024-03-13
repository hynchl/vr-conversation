using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Vector3 = System.Numerics.Vector3;

public class Recorder
{
    public int chunkLength = 10000;
    public string filePath;
    List<Dictionary<string, string>> rows;
    List<string> header;
    
    public Recorder(string filePath) {
        this.filePath = Path.Combine(GetPath(), filePath);
        this.rows = new List<Dictionary<string, string>>();
        this.header = new List<string>();
    }

    ~Recorder() {
        // this.Save();
    }

    public void Add <T> (Dictionary<string, T> row) {
        Dictionary<string, string> newRow = new Dictionary<string, string>();
        foreach (var key in row.Keys)
        {
            newRow[key] = row[key].ToString();
        }
        rows.Add(newRow);

        if (row.Count > header.Count)
        {
           this.header = row.Keys.ToList();
        }

        
        if (rows.Count >= chunkLength) {
            Save();
            Clear();
        }
    }

    public void Save(string delimiter="\t")
    {
        if (rows == null || rows.Count == 0)
        {
            Debug.LogError("No rows to save.");
            return;
        }

        // Create a StringBuilder to store the rows.
        var content = new System.Text.StringBuilder();

        // Write the header row.
        // var header = this.rows[this.rows.Count-1].Keys;
        bool fileExists = File.Exists(filePath);
        // content.AppendLine(string.Join(delimiter, header));
        if (!fileExists)
        {
            content.AppendLine(string.Join(delimiter, header));
        }
        foreach (Dictionary<string, string> row in rows)
        {
            var values = new List<string>();
            foreach (var key in header)
            {
                string val = "";
                row.TryGetValue(key, out val);
                values.Add(val);
            }
            content.AppendLine(string.Join(delimiter, values));
        }

        // Write the content to a file.
        if (fileExists)
        {
            File.AppendAllText(filePath, content.ToString());
        }
        else
        {
            File.WriteAllText(filePath, content.ToString());
        }
        // File.WriteAllText(filePath, content.ToString());
        Debug.Log("file saved to: " + filePath);
    }

    public static string GetPath()
    {
        string path = null;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(Application.persistentDataPath, "Resources/");
            case RuntimePlatform.IPhonePlayer:
                path = Application.persistentDataPath;
                return Path.Combine(path, "Resources/");
            case RuntimePlatform.OSXEditor:
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Assets", "Resources/");
            case RuntimePlatform.OSXPlayer:
                path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Assets", "Resources/");       
            case RuntimePlatform.WindowsEditor:
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Assets", "Resources/");
            default:
                path = Application.streamingAssetsPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Resources/");
        }
    }

    void Clear() {
        rows.Clear();
    }
}
