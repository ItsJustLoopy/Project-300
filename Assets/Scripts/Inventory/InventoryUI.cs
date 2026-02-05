using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References (optional, will auto-resolve)")]
    [SerializeField] private Player player;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private Image blockIcon;

    private void Awake()
    {
        if (blockIcon == null)
        {
            blockIcon = GetComponentInChildren<Image>(true);
        }

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (inventory == null && player != null)
        {
            inventory = player.inventory;
        }
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
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
        if (blockIcon == null)
        {
            return;
        }

        if (inventory == null || inventory.IsEmpty())
        {
            blockIcon.enabled = false;
            return;
        }

        blockIcon.enabled = true;

        if (inventory.TryGetHeldColor(out var c))
        {
            blockIcon.color = c;
        }
    }
}