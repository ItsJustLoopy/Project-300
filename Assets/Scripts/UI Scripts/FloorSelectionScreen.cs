using UnityEngine;

public class FloorSelectionScreen : MonoBehaviour
{
    public Transform ListContent;
    public LevelButton LevelButtonPrefab;

    private LevelManager LevelManager;

    private void OnEnable()
    {
        LevelManager = FindAnyObjectByType<LevelManager>();

        if(LevelManager == null)
        {
            Debug.LogError("LevelManager not found.");
            return;
        }

        for (int i = 0; i < LevelManager.levelDatas.Length; i++)
        {
            LevelButton ButtonInstance = Instantiate(LevelButtonPrefab, ListContent);
            ButtonInstance.txtLevelName.text = $"Level {i + 1}";

            ButtonInstance.OnFloorSelected = () => 
            {
               var LevelData =  LevelManager.levelDatas[i]; //load this level
            };
        }
    }

    private void OnDisable()
    {
        foreach (Transform t in ListContent)
        {
            Destroy(t.gameObject);
        }
    }
}