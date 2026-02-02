using UnityEngine;
using System.IO;

public class DebugCollector : MonoBehaviour
{
    private static string logFilePath;

    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        //creating a game object to allow code to run in windows build
        var go = new GameObject("DebugCollector");
        DontDestroyOnLoad(go);
        go.AddComponent<DebugCollector>();
    }

    private void Awake()
    {
        //ensures logs are wrote to the correct file
        string folder = Path.Combine(Application.persistentDataPath, "Log");
        Directory.CreateDirectory(folder);

        logFilePath = Path.Combine(folder, "ConsoleLog.txt");

        File.WriteAllText(logFilePath, "-- New Session --\n");

        Debug.Log($"Log file saved to: {logFilePath}");

        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
        string entry = $"[{System.DateTime.Now:HH:mm:ss}] [{type}] {logString}\n";

        if (type == LogType.Error || type == LogType.Exception)
        {
            entry += stackTrace + "\n";
        }

        try
        {
            File.AppendAllText(logFilePath, entry);
        }
        catch
        {
            // Avoid recursive logging
        }
    }
}
