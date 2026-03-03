using System;
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
    [Tooltip("Pickup and Place unlock level")]
    [Min(1)] public int inventoryUnlockLevel = 1;

    [Header("Objects")]
    public GameObject groundTilePrefab;
    public GameObject playerPrefab;
    public GameObject blockPrefab;
    public GameObject arrowPrefab;
    public GameObject holeIndicatorPrefab;
    public GameObject mainCamera;

    [Header("Visuals")]
    public float inactiveLevelOpacity = 0.2f;
    public float fadeTransitionSpeed = 0.5f;
    public float arrowYOffset = 0.02f;

    [Header("Game Background")]
    [SerializeField, Range(0.1f, 5f)] private float gameShaderSpeed = 0.8f;
    [SerializeField, Range(0.1f, 2f)] private float gameShaderIntensity = 0.5f;
    [SerializeField] private Color gameShaderTintA = new Color(0.20f, 0.35f, 0.55f, 1f);
    [SerializeField] private Color gameShaderTintB = new Color(0.85f, 0.45f, 0.95f, 1f);

    public LevelData _currentLevelData;
    public GameObject _playerInstance;
    public Player _playerScript;

    public LevelLoader loader { get; private set; }
    public LevelVisuals visuals { get; private set; }
    public UndoManager undo { get; private set; }
    public ElevatorManager elevators { get; private set; }
 	public DebugCollector DebugCollector;
        private bool _isResetInProgress;

        public bool IsResetInProgress => _isResetInProgress;

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

        BackgroundGenerator.CreateAnimatedBackground(
            mainCamera.GetComponent<Camera>(),
            gameShaderSpeed,
            gameShaderIntensity,
            gameShaderTintA,
            gameShaderTintB);

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
        Debug.Log("meow 2");
      

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

    public Block GetBlockingBlockAt(Vector2Int position)
    {
        return loader.GetBlockingBlockAt(position);
    }

    public bool HasMovingBlockOnCurrentLevel()
    {
        return loader.HasMovingBlockOnCurrentLevel();
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
        if (!CanUseUndoOrReset())
        {
            return;
        }

        undo.UndoLastMove();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RequestSave();
        }
    }

    public void ResetCurrentLevel()
    {
        if (!CanUseUndoOrReset())
        {
            return;
        }

        if (_isResetInProgress)
        {
            return;
        }

        int levelIndex = currentLevelIndex;

        if (levelIndex < 0 || levelIndex >= levelDatas.Length)
        {
            return;
        }

        _isResetInProgress = true;

        try
        {
            _currentLevelData = UnityEngine.Object.Instantiate(levelDatas[levelIndex]);

            bool preserveReturnElevator = levelIndex > 0;
            var returnElevatorPosition = Vector2Int.zero;
            bool hasReturnElevatorPosition = preserveReturnElevator && elevators.TryGetElevatorPositionOnLevel(levelIndex, out returnElevatorPosition);

            if (!preserveReturnElevator)
            {
                elevators.ClearElevatorsForLevel(levelIndex);
            }

            loader.ResetLevelToInitialState(levelIndex, preserveInHoleBlocks: preserveReturnElevator);
            loader.ManageLoadedLevels(loadMissingLevels: false);
            loader.UpdateGroundTilesForCurrentLevel();

            Vector2Int spawn = hasReturnElevatorPosition ? returnElevatorPosition : _currentLevelData.playerSpawn;
            float spawnY = levelIndex * verticalSpacing + 1f;

            if (_playerScript != null)
            {
                _playerScript.gridPosition = spawn;
                _playerScript.facingDirection = Vector2Int.up;

                if (_playerScript.inventory != null)
                {
                    _playerScript.inventory.Clear();
                }
            }

            if (_playerInstance != null)
            {
                _playerInstance.transform.position = new Vector3(spawn.x, spawnY, spawn.y);
                _playerInstance.transform.rotation = Quaternion.identity;
            }

            undo.ClearHistoryForLevel(levelIndex);

            visuals.UpdateLevelOpacities();
            visuals.SetLevelOpacity(levelIndex, 1f);
            if (loader.TryGetLevelObjects(levelIndex, out var levelObjects))
            {
                levelObjects.currentOpacity = 1f;
                levelObjects.targetOpacity = 1f;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.RequestSave();
            }
        }
        finally
        {
            _isResetInProgress = false;
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

    public bool IsInventoryUnlocked()
    {
        int currentLevelNumber = currentLevelIndex + 1;
        return currentLevelNumber >= inventoryUnlockLevel;
    }

    public bool CanUseUndoOrReset()
    {
        return currentLevelIndex >= elevators.HighestVisitedLevel;
    }

}
