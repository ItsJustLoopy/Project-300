using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Data")]
    public LevelData[] levelDatas;

    [Header("Level Settings")]
    public int currentLevelIndex = 0;
    public float verticalSpacing = 5f;

    [Header("Objects")]
    public GameObject groundTilePrefab;
    public GameObject playerPrefab;
    public GameObject blockPrefab;
    public GameObject arrowPrefab;
    public GameObject mainCamera;

    [Header("Visuals")]
    public float inactiveLevelOpacity = 0.2f;
    public float fadeTransitionSpeed = 0.5f;
    public float arrowYOffset = 0.02f;

    public LevelData _currentLevelData;
    public GameObject _playerInstance;
    public Player _playerScript;

    public LevelLoader loader { get; private set; }
    public LevelVisuals visuals { get; private set; }
    public UndoManager undo { get; private set; }
    public ElevatorManager elevators { get; private set; }
 	public DebugCollector DebugCollector;

    void Awake()
    {
        DebugCollector = new DebugCollector();
        Instance = this;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        loader = new LevelLoader(this);
        visuals = new LevelVisuals(this);
        undo = new UndoManager(this);
        elevators = new ElevatorManager(this);

        BackgroundGenerator.CreateAnimatedBackground(mainCamera.GetComponent<Camera>());

        if (levelDatas == null || levelDatas.Length == 0 || groundTilePrefab == null)
        {
            Debug.LogError("Missing LevelData or GroundTile prefab and cannot generate level");
            return;
        }

        if (SaveManager.Instance != null && SaveManager.Instance.SaveFileExists())
        {
            bool loadSuccess = SaveManager.Instance.LoadGame();
            if (loadSuccess)
            {
                Debug.Log("Game loaded from save file");
                return;
            }
        }

        GenerateLevel(currentLevelIndex);
        _currentLevelData = levelDatas[currentLevelIndex];

        SpawnPlayer();

        UpdateLevelOpacities();
    }

    void Update()
    {
        visuals.Tick();
    }

    public void GenerateLevel(int levelIndex, bool skipBlocks = false)
    {
        loader.GenerateLevel(levelIndex, skipBlocks);
    }

    public void ManageLoadedLevels(bool loadMissingLevels = true)
    {
        loader.ManageLoadedLevels(loadMissingLevels);
    }

    public void UpdateLevelOpacities()
    {
        visuals.UpdateLevelOpacities();
    }

    public void SetLevelOpacity(int levelIndex, float opacity)
    {
        visuals.SetLevelOpacity(levelIndex, opacity);
    }

    public void SpawnPlayer()
    {
        loader.SpawnPlayer();
    }

    public bool CheckOutOfBounds(Vector2Int position)
    {
        return loader.CheckOutOfBounds(position);
    }

    public GroundTile GetTileAt(Vector2Int position)
    {
        return loader.GetTileAt(position);
    }

    public LevelData GetCurrentLevelData()
    {
        return loader.GetCurrentLevelData();
    }

    public void RegisterElevator(Vector2Int position, Block block)
    {
        elevators.RegisterElevator(position, block);
    }

    public bool IsElevatorAt(Vector2Int position)
    {
        return elevators.IsElevatorAt(position);
    }

    public Block GetElevatorAt(Vector2Int position)
    {
        return elevators.GetElevatorAt(position);
    }

    public IEnumerator UseElevator(Vector2Int elevatorPosition)
    {
        return elevators.UseElevator(elevatorPosition);
    }

    public void UpdateGroundTilesForCurrentLevel()
    {
        loader.UpdateGroundTilesForCurrentLevel();
    }

    public void RecordSnapshot()
    {
        undo.RecordSnapshot();
    }

    public void UndoLastMove()
    {
        undo.UndoLastMove();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RequestSave();
        }
    }

    public void RemoveBlock(Block block)
    {
        loader.RemoveBlock(block);
    }

    public bool CanPlaceBlockAt(Vector2Int position)
    {
        return loader.CanPlaceBlockAt(position);
    }

    public void PlaceExistingBlock(Vector2Int position, Block block)
    {
        loader.PlaceExistingBlock(position, block);
    }

}
