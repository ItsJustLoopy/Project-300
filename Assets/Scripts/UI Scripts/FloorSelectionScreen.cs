using UnityEngine;

public class FloorSelectionScreen : MonoBehaviour
{
    public Transform ListContent;
    public LevelButton LevelButtonPrefab;

    private LevelManager levelManager;

    private void OnEnable()
    {
        levelManager = FindAnyObjectByType<LevelManager>();

        if(levelManager == null)
        {
            Debug.LogError("LevelManager not found.");
            return;
        }

        for (int i = 0; i < levelManager.levelDatas.Length; i++)
        {
            LevelButton buttonInstance = Instantiate(LevelButtonPrefab, ListContent);
            buttonInstance.txtLevelName.text = $"Level {i + 1}";

            buttonInstance.OnFloorSelected = () => 
            {
               var levelData =  levelManager.levelDatas[i];
                //load this level
               
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
