using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class LevelDataAssetHandler
{
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);
        
        if (obj is LevelData levelData)
        {
            LevelEditorWindow.OpenWindow(levelData);
            return true;
        }
        
        return false;
    }
}
