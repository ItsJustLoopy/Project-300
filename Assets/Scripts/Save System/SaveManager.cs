using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SAVE_FILENAME = "gamesave.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILENAME);

    [Header("Save Settings")]
    [SerializeField] private float saveDebounceSeconds = 0.25f;

    private Coroutine _pendingSaveCoroutine;
    private float _lastSaveTime = -999f;
    private readonly SaveStateCapture _stateCapture = new SaveStateCapture();
    private readonly SaveStateRestore _stateRestore = new SaveStateRestore();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    public void RequestSave()
    {
        if (Instance == null) return;

        if (_pendingSaveCoroutine != null)
        {
            StopCoroutine(_pendingSaveCoroutine);
        }

        _pendingSaveCoroutine = StartCoroutine(SaveSoonCoroutine());
    }

    private System.Collections.IEnumerator SaveSoonCoroutine()
    {
        float remaining = saveDebounceSeconds - (Time.unscaledTime - _lastSaveTime);
        if (remaining > 0f)
        {
            yield return new WaitForSecondsRealtime(remaining);
        }

        _pendingSaveCoroutine = null;
        SaveGame();
    }

    public void SaveGame()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("Cannot save: LevelManager not found");
            return;
        }

        GameSaveData saveData = _stateCapture.CaptureGameState(LevelManager.Instance);
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);

        try
        {
            File.WriteAllText(SavePath, json);
            _lastSaveTime = Time.unscaledTime;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public bool SaveFileExists()
    {
        return File.Exists(SavePath);
    }

    public bool LoadGame()
    {
        if (!SaveFileExists())
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameSaveData saveData = JsonConvert.DeserializeObject<GameSaveData>(json);

            if (saveData == null)
            {
                Debug.LogError("Failed to load game: save file deserialized to null.");
                return false;
            }

            _stateRestore.RestoreGameState(LevelManager.Instance, saveData);
            Debug.Log($"Game loaded successfully from: {SavePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    public void DeleteSave()
    {
        if (SaveFileExists())
        {
            try
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }
    }

    public void PrintSavePath()
    {
        Debug.Log($"Save file location: {SavePath}");
    }
}
