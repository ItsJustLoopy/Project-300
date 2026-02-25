using UnityEngine;

public class LevelVisuals
{
    private readonly LevelManager _levelManager;

    public LevelVisuals(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public void Tick()
    {
        foreach (var lvl in _levelManager.loader.LoadedLevels)
        {
            LevelLoader.LevelObjects levelObjs = lvl.Value;
            if (Mathf.Abs(levelObjs.currentOpacity - levelObjs.targetOpacity) > 0.01f)
            {
                levelObjs.currentOpacity = Mathf.Lerp(levelObjs.currentOpacity, levelObjs.targetOpacity,
                    Time.deltaTime * _levelManager.fadeTransitionSpeed);
                SetLevelOpacity(lvl.Key, levelObjs.currentOpacity);
            }
        }
    }

    public void UpdateLevelOpacities()
    {
        foreach (var kvp in _levelManager.loader.LoadedLevels)
        {
            int levelIndex = kvp.Key;
            LevelLoader.LevelObjects levelObjs = kvp.Value;

            if (levelIndex == _levelManager.currentLevelIndex)
            {
                levelObjs.targetOpacity = 1f;
            }
            else
            {
                levelObjs.targetOpacity = _levelManager.inactiveLevelOpacity;
            }
        }
    }

    public void SetLevelOpacity(int levelIndex, float opacity)
    {
        if (!_levelManager.loader.TryGetLevelObjects(levelIndex, out var levelObjs))
        {
            return;
        }

        foreach (GameObject tileObj in levelObjs.tiles)
        {
            if (tileObj != null)
            {
                SetObjectOpacity(tileObj, opacity);
            }
        }

        foreach (GameObject blockObj in levelObjs.blocks)
        {
            if (blockObj != null)
            {
                SetObjectOpacity(blockObj, opacity);
            }
        }
    }

    private void SetObjectOpacity(GameObject obj, float opacity)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = opacity;
                    mat.color = color;

                    if (opacity < 1f)
                    {
                        mat.SetFloat("_Surface", 1);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                    else
                    {
                        mat.SetFloat("_Surface", 0);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 2000;
                    }
                }
            }
        }
    }

    public void PositionCameraForLevel(int levelIndex)
    {
        float targetLevelY = levelIndex * _levelManager.verticalSpacing;
        float cameraY = targetLevelY + 19f;

        Vector3 currentCameraPos = _levelManager.mainCamera.transform.position;
        _levelManager.mainCamera.transform.position = new Vector3(currentCameraPos.x, cameraY, currentCameraPos.z);
    }
}
