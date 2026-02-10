using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References (optional, will auto-resolve)")]
     public Player player;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] public Image blockIcon;
    public static InventoryUI Instance;
    private void Awake()
    {
        Instance = this;

        //if (blockIcon == null)
        //{
        //    blockIcon = GetComponentInChildren<Image>(true);
        //}

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (inventory == null && player != null)
        {
            inventory = player.inventory;
        }


    }

    private void Start()
    {
        Refresh();
    }


    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnChanged += Refresh;
            Debug.Log("subscribed to onchanged");
        }
        
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
        Debug.Log("refreshing");
        if (blockIcon == null)
        {
            Debug.Log("block icon not found");
            return;
        }

        if (inventory == null)
        {
            Debug.Log("inventory is null");

            blockIcon.enabled = false;
            return;
        }

        blockIcon.enabled = true;

        var c = inventory.TryGetHeldColor();

        Debug.Log($"trying to get the color {c}");

        blockIcon.color = c;
    }


    public void AssignPlayer(Player p)
    {
        player = p;
        
        //if (blockIcon == null)
        //{
        //    blockIcon = GetComponentInChildren<Image>(true);
        //}

        //if (player == null)
        //{
        //    player = FindAnyObjectByType<Player>();
        //}

        if (inventory == null && player != null)
        {
            inventory = player.inventory;
        }
    }
}