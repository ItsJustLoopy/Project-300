using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class LevelEditorWindow : EditorWindow
{
    private LevelData _levelData;

    private int _selectedTool; 
    // 0 = Block, 1 = Hole  2 = Player 3 = Remove Block
    private BlockData.BlockColor _selectedBlockColor = BlockData.BlockColor.Red;
    private const float CellSize = 50f;
    
    public static void OpenWindow(LevelData data)
    {
        LevelEditorWindow window = GetWindow<LevelEditorWindow>("Level Editor");
        window._levelData = data;
        window.maxSize = new Vector2(700, 700);
        window.minSize = new Vector2(700, 700);
        window.Show();
    }
    
    private void OnGUI()
    {
        
        EditorGUILayout.LabelField("Level: " + _levelData.name, EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Width:", GUILayout.Width(50));
        _levelData.levelWidth = EditorGUILayout.IntField(_levelData.levelWidth, GUILayout.Width(50));
        EditorGUILayout.LabelField("Height:", GUILayout.Width(50));
        _levelData.levelHeight = EditorGUILayout.IntField(_levelData.levelHeight, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        
        EditorGUILayout.LabelField("Select Tool:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Place Block", _selectedTool == 0 ? GUI.skin.box : GUI.skin.button))
            _selectedTool = 0;
        if (GUILayout.Button("Set Hole", _selectedTool == 1 ? GUI.skin.box : GUI.skin.button))
            _selectedTool = 1;
        if (GUILayout.Button("Set Player", _selectedTool == 2 ? GUI.skin.box : GUI.skin.button))
            _selectedTool = 2;
        if (GUILayout.Button("Remove Block", _selectedTool == 3 ? GUI.skin.box : GUI.skin.button))
            _selectedTool = 3;
        
        EditorGUILayout.EndHorizontal();
        
        if (_selectedTool == 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Block Color:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Red", _selectedBlockColor == BlockData.BlockColor.Red ? GUI.skin.box : GUI.skin.button))
                _selectedBlockColor = BlockData.BlockColor.Red;
            if (GUILayout.Button("Yellow", _selectedBlockColor == BlockData.BlockColor.Yellow ? GUI.skin.box : GUI.skin.button))
                _selectedBlockColor = BlockData.BlockColor.Yellow;
            if (GUILayout.Button("Blue", _selectedBlockColor == BlockData.BlockColor.Blue ? GUI.skin.box : GUI.skin.button))
                _selectedBlockColor = BlockData.BlockColor.Blue;
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space();
        DrawGrid();
        
        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            EditorUtility.SetDirty(_levelData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    private void DrawGrid()
    {
        Event e = Event.current;
        
        for (int y = _levelData.levelHeight - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            
            for (int x = 0; x < _levelData.levelWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                DrawCell(pos, e);
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
    
    private void DrawCell(Vector2Int pos, Event e)
    {
        Rect rect = GUILayoutUtility.GetRect(CellSize, CellSize);
        
        Color color = Color.white;
        string label = "Ground";
        
        if (pos == _levelData.holePosition)
        {
            color = Color.black;
            label = "Hole";
        }
        else if (pos == _levelData.playerSpawn)
        {
            color = Color.blue;
            label = "Player";
        }
        else if (HasBlockAt(pos, out BlockData blockData))
        {
            color = GetBlockDisplayColor(blockData.blockColor);
            label = blockData.blockColor.ToString();
        }
        
        EditorGUI.DrawRect(rect, color);
        GUI.Box(rect, label);
        
        if (rect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
        {
            HandleCellClick(pos);
            e.Use();
            Repaint();
        }
    }
    
    private bool HasBlockAt(Vector2Int pos, out BlockData foundBlock)
    {
        foundBlock = null;
        if (_levelData.blocks == null) return false;
        
        foreach (var block in _levelData.blocks)
        {
            if ((int)block.BlockPosition.x == pos.x && (int)block.BlockPosition.z == pos.y)
            {
                foundBlock = block;
                return true;
            }
        }
        return false;
    }
    
    private bool HasBlockAt(Vector2Int pos)
    {
        return HasBlockAt(pos, out _);
    }
    
    private Color GetBlockDisplayColor(BlockData.BlockColor blockColor)
    {
        switch (blockColor)
        {
            case BlockData.BlockColor.Red: return Color.red;
            case BlockData.BlockColor.Yellow: return Color.yellow;
            case BlockData.BlockColor.Blue: return new Color(0.3f, 0.3f, 1f);
            case BlockData.BlockColor.Purple: return new Color(0.5f, 0f, 0.5f);
            case BlockData.BlockColor.Orange: return new Color(1f, 0.5f, 0f);
            case BlockData.BlockColor.Green: return Color.green;
            case BlockData.BlockColor.Black: return new Color(0.2f, 0.2f, 0.2f);
            default: return Color.red;
        }
    }
    
    private void HandleCellClick(Vector2Int pos)
    {
        Undo.RecordObject(_levelData, "Edit Level");

        switch (_selectedTool)
        {
            case 0:
                RemoveBlockAt(pos);
                AddBlock(pos);
                break;
            case 1:
                _levelData.holePosition = pos;
                break;
            case 2:
                _levelData.playerSpawn = pos;
                break;
            case 3:
                RemoveBlockAt(pos);
                break;
        }
    }
    
    private void AddBlock(Vector2Int pos)
    {
        string levelPath = AssetDatabase.GetAssetPath(_levelData);
        string levelDir = Path.GetDirectoryName(levelPath);
        
        BlockData newBlock = CreateInstance<BlockData>();
        newBlock.BlockPosition = new Vector3(pos.x, 1, pos.y);
        newBlock.blockName = _selectedBlockColor.ToString() + " Block";
        newBlock.blockColor = _selectedBlockColor;
        newBlock.containedColors = new List<BlockData.BlockColor> { _selectedBlockColor };
        
        
        newBlock.name = $"Block_{pos.x}_{pos.y}_{_selectedBlockColor}";
        AssetDatabase.AddObjectToAsset(newBlock, _levelData);
        
        List<BlockData> blocks = new List<BlockData>(_levelData.blocks ?? new List<BlockData>());
        blocks.Add(newBlock);
        _levelData.blocks = blocks;
        
        EditorUtility.SetDirty(_levelData);
        EditorUtility.SetDirty(newBlock);
    }
    
    private void RemoveBlockAt(Vector2Int pos)
    {
        if (_levelData.blocks == null) return;
        
        List<BlockData> blocks = new List<BlockData>();
        foreach (var block in _levelData.blocks)
        {
            if ((int)block.BlockPosition.x == pos.x && (int)block.BlockPosition.z == pos.y)
            {
                
                AssetDatabase.RemoveObjectFromAsset(block);
            }
            else
            {
                blocks.Add(block);
            }
        }

        _levelData.blocks = blocks;
        EditorUtility.SetDirty(_levelData);
    }
}
