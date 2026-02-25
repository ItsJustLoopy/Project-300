using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Player player;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private GameObject blockIconObject;
    [SerializeField] private Image blockIcon;
    public static InventoryUI Instance;

    private void Awake()
    {
        Instance = this;
        ResolveBlockIcon();

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

    private void Refresh()
    {
        ResolveBlockIcon();

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
        blockIcon.color = c;
    }

    private void ResolveBlockIcon()
    {
        if (blockIcon == null && blockIconObject != null)
        {
            blockIcon = blockIconObject.GetComponent<Image>();
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