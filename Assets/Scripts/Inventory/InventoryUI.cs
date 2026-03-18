using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private static readonly int CubeColorId = Shader.PropertyToID("_CubeColor");
    private static readonly int CubeScaleId = Shader.PropertyToID("_CubeScale");

    [Header("References")]
    public Player player;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private GameObject blockIconObject;
    [SerializeField] private Image blockIcon;
    [Header("Cube Icon Shader")]
    [SerializeField] private Shader blockCubeShader;
    [SerializeField, Range(0.4f, 1.6f)] private float cubeScale = 0.95f;

    private Material blockIconMaterialInstance;
    public static InventoryUI Instance;

    private void Awake()
    {
        Instance = this;
        ResolveBlockIcon();
        EnsureCubeMaterial();

        ResolvePlayerAndInventory();

        if (inventory != null)
        {
            inventory.OnChanged -= Refresh;
            inventory.OnChanged += Refresh;
        }
    }

    private void Start()
    {
        Refresh();
    }


    private void OnEnable()
    {
        ResolveBlockIcon();
        EnsureCubeMaterial();
        ResolvePlayerAndInventory();

        if (inventory != null)
        {
            inventory.OnChanged -= Refresh;
            inventory.OnChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnChanged -= Refresh;
        }
    }

    private void OnDestroy()
    {
        if (blockIconMaterialInstance != null)
        {
            Destroy(blockIconMaterialInstance);
            blockIconMaterialInstance = null;
        }
    }

    private void Refresh()
    {
        ResolveBlockIcon();
        EnsureCubeMaterial();

        if (blockIcon == null)
        {
            Debug.LogWarning("InventoryUI: assign either blockIcon (Image) or blockIconObject (GameObject with Image).", this);
            return;
        }

        if (inventory == null)
        {
            blockIcon.enabled = false;
            return;
        }

        bool hasItem = inventory.HasItem;
        blockIcon.enabled = hasItem;
        if (!hasItem)
        {
            return;
        }

        var c = inventory.TryGetHeldColor();
        if (blockIconMaterialInstance != null && blockIconMaterialInstance.HasProperty(CubeColorId))
        {
            blockIconMaterialInstance.SetColor(CubeColorId, c);
        }

        if (blockIconMaterialInstance != null && blockIconMaterialInstance.HasProperty(CubeScaleId))
        {
            blockIconMaterialInstance.SetFloat(CubeScaleId, cubeScale);
        }

        blockIcon.color = Color.white;
    }

    private void ResolveBlockIcon()
    {
        if (blockIcon == null && blockIconObject != null)
        {
            blockIcon = blockIconObject.GetComponent<Image>();
        }
    }

    private void EnsureCubeMaterial()
    {
        if (blockIcon == null)
            return;

        if (blockCubeShader == null)
        {
            blockCubeShader = Shader.Find("UI/InventorySpinningCube");
        }

        if (blockCubeShader == null)
            return;

        if (blockIconMaterialInstance != null && blockIconMaterialInstance.shader != blockCubeShader)
        {
            Destroy(blockIconMaterialInstance);
            blockIconMaterialInstance = null;
        }

        if (blockIconMaterialInstance == null)
        {
            blockIconMaterialInstance = new Material(blockCubeShader)
            {
                name = "Inventory Cube Icon (Runtime)"
            };
        }

        if (blockIcon.material != blockIconMaterialInstance)
        {
            blockIcon.material = blockIconMaterialInstance;
        }

        if (blockIconMaterialInstance.HasProperty(CubeScaleId))
        {
            blockIconMaterialInstance.SetFloat(CubeScaleId, cubeScale);
        }
    }

    private void ResolvePlayerAndInventory()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        InventoryManager nextInventory = ResolveInventoryFromPlayer(player);
        if (!ReferenceEquals(inventory, nextInventory))
        {
            if (inventory != null)
            {
                inventory.OnChanged -= Refresh;
            }

            inventory = nextInventory;
        }
    }

    private InventoryManager ResolveInventoryFromPlayer(Player targetPlayer)
    {
        if (targetPlayer == null)
            return null;

        if (targetPlayer.inventory != null)
            return targetPlayer.inventory;

        return targetPlayer.GetComponent<InventoryManager>();
    }


    public void AssignPlayer(Player p)
    {
        player = p;

        InventoryManager nextInventory = ResolveInventoryFromPlayer(player);
        if (ReferenceEquals(inventory, nextInventory))
        {
            Refresh();
            return;
        }

        if (inventory != null)
        {
            inventory.OnChanged -= Refresh;
        }

        inventory = nextInventory;

        if (isActiveAndEnabled && inventory != null)
        {
            inventory.OnChanged -= Refresh;
            inventory.OnChanged += Refresh;
        }

        Refresh();
    }
}