using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Open Level Editor", GUILayout.Height(40)))
        {
            LevelEditorWindow.OpenWindow((LevelData)target);
            Debug.Log("Opened Level Editor");
        }
    }
}
