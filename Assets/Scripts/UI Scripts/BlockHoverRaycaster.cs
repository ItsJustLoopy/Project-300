using UnityEngine;
using UnityEngine.EventSystems;

public class BlockHoverRaycaster : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask blockLayerMask = ~0; // default = everything
    [SerializeField] private float maxDistance = 200f;

    [Header("UI")]
    [SerializeField] private BlockHoverContextMenuUI contextMenu;

    private Block _lastHovered;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (contextMenu == null) contextMenu = FindAnyObjectByType<BlockHoverContextMenuUI>();
    }

    private void Update()
    {
        if (targetCamera == null || contextMenu == null)
            return;

        // If pointer is over UI, you can either hide menu or keep last state.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHover();
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, blockLayerMask, QueryTriggerInteraction.Ignore))
        {
            // In case collider is on child, search up
            Block block = hit.collider.GetComponentInParent<Block>();

            if (block != null)
            {
                if (block != _lastHovered)
                {
                    _lastHovered = block;
                    contextMenu.ShowFor(block);
                }
                else
                {
                    // same block; menu already up
                    contextMenu.ShowFor(block);
                }

                return;
            }
        }

        // No block hit
        ClearHover();
    }

    private void ClearHover()
    {
        if (_lastHovered != null)
        {
            _lastHovered = null;
            contextMenu.Hide();
        }
        else
        {
            // Ensure hidden anyway
            contextMenu.Hide();
        }
    }
}