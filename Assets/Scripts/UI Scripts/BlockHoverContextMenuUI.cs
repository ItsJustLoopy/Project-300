using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlockHoverContextMenuUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject rowPrefab;

    // Track current so we don’t rebuild UI every frame for same block
    private Block _currentBlock;

    private readonly List<GameObject> _spawnedRows = new List<GameObject>();

    private void Awake()
    {
        // Auto-resolve for safety
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    public void ShowFor(Block block)
    {
        if (block == null)
        {
            Hide();
            return;
        }

        // If hovering the same block, do nothing
        if (_currentBlock == block && IsVisible())
            return;

        _currentBlock = block;

        RebuildRows(block);
        Show();
    }

    public void Hide()
    {
        _currentBlock = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;   // UI is display-only
            canvasGroup.blocksRaycasts = false; // don’t block mouse raycasts
        }
    }

    private bool IsVisible()
    {
        return canvasGroup != null && canvasGroup.alpha > 0.5f;
    }

    private void RebuildRows(Block block)
    {
        ClearRows();

        if (contentRoot == null || rowPrefab == null)
        {
            Debug.LogWarning("BlockHoverContextMenuUI not wired: contentRoot/rowPrefab missing.");
            return;
        }

        var primaries = block.containedPrimaryColors;

        // If empty (e.g., immovable or white), show one row saying “None”
        if (primaries == null || primaries.Count == 0)
        {
            SpawnRow("None", Color.gray);
            return;
        }

        for (int i = 0; i < primaries.Count; i++)
        {
            var c = primaries[i];
            SpawnRow(c.ToString(), ToUnityColor(c));
        }
    }

    private void SpawnRow(string label, Color colorColorSquare)
    {
        var row = Instantiate(rowPrefab, contentRoot);
        _spawnedRows.Add(row);

        // Find subcomponents by type/name (simple + robust)
        var images = row.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject.name.ToLower().Contains("colorsquare"))
            {
                img.color = colorColorSquare;
                break;
            }
        }

        var tmp = row.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) tmp.text = label;
    }

    private void ClearRows()
    {
        for (int i = 0; i < _spawnedRows.Count; i++)
        {
            if (_spawnedRows[i] != null)
                Destroy(_spawnedRows[i]);
        }
        _spawnedRows.Clear();
    }

    // Only needs primary colors for this feature (matches your Block logic).
    private Color ToUnityColor(BlockData.BlockColor color)
    {
        switch (color)
        {
            case BlockData.BlockColor.Red: return Color.red;
            case BlockData.BlockColor.Yellow: return Color.yellow;
            case BlockData.BlockColor.Blue: return Color.blue;
            default: return Color.white;
        }
    }
}