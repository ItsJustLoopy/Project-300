using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class DebugCollector
{
    private static string logFilePath;
    public static DebugCollector Instance;

    public DebugCollector()
    {
        Instance = this;

        //set file path and folder if missing
        string folder = Path.Combine(Application.dataPath, "Scripts/Log");
        Directory.CreateDirectory(folder);
        logFilePath = Path.Combine(folder, "ConsoleLog.txt");
        
        //clearing out previous logs
        File.WriteAllText(logFilePath, "--New Session-- \n");
        //debug for finding where log is located
        Debug.Log($"File saved to {logFilePath} ");
        Application.logMessageReceived += HandleEditorLog;
    }

    private static void HandleEditorLog(string LogString, string stackTrace, LogType type)
    {
        string entry = $"[{System.DateTime.Now:HH:mm:ss}] [{type}] {LogString}\n";
        //includes stack trace for errors and exceptions
        if (type == LogType.Error || type == LogType.Exception)
        {
            entry += stackTrace + "\n";
        }
        File.AppendAllText(logFilePath, entry);
    }
}
