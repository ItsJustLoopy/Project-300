using UnityEngine;

public static class BackgroundGenerator
{
    private const string MenuBackgroundObjectName = "MenuAnimatedBackground";

    public static void CreateAnimatedBackground(
        Camera camera,
        float animationSpeed = 0.8f,
        float intensity = 0.5f,
        Color? tintA = null,
        Color? tintB = null)
    {
        if (camera == null)
        {
            Debug.LogError("Animated background setup failed: camera is null.");
            return;
        }

        Shader shader = Shader.Find("Skybox/AnimatedBackground");
        if (shader != null)
        {
            Material backgroundMaterial = new Material(shader);

            backgroundMaterial.SetFloat("_AnimationSpeed", animationSpeed);
            backgroundMaterial.SetFloat("_Intensity", intensity);
            backgroundMaterial.SetColor("_TintA", tintA ?? new Color(0.20f, 0.35f, 0.55f, 1f));
            backgroundMaterial.SetColor("_TintB", tintB ?? new Color(0.85f, 0.45f, 0.95f, 1f));

            RenderSettings.skybox = backgroundMaterial;
            camera.clearFlags = CameraClearFlags.Skybox;

            DynamicGI.UpdateEnvironment();
        }
        else
        {
            Debug.LogError("AnimatedBackground shader not found, make sure it is in the project.");
        }
    }

    public static void CreateMenuBackground(
        Camera camera,
        float animationSpeed = 0.9f,
        float intensity = 0.9f,
        float scale = 90f,
        float quadDistance = 5f,
        float quadFill = 1.05f)
    {
        if (camera == null)
        {
            Debug.LogError("Menu background setup failed: camera is null.");
            return;
        }

        Shader shader = Shader.Find("Unlit/MenuBackgroundFlat");
        if (shader != null)
        {
            Material backgroundMaterial = new Material(shader);
            backgroundMaterial.SetFloat("_AnimationSpeed", animationSpeed);
            backgroundMaterial.SetFloat("_Intensity", intensity);
            backgroundMaterial.SetFloat("_Scale", scale);

            Transform existing = camera.transform.Find(MenuBackgroundObjectName);
            GameObject backgroundQuad;

            if (existing != null)
            {
                backgroundQuad = existing.gameObject;
            }
            else
            {
                backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                backgroundQuad.name = MenuBackgroundObjectName;
                backgroundQuad.transform.SetParent(camera.transform, false);
                var collider = backgroundQuad.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.Destroy(collider);
                }
            }

            float distance = Mathf.Max(0.1f, quadDistance);
            float fill = Mathf.Max(1f, quadFill);
            float height = 2.0f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * camera.aspect;

            backgroundQuad.transform.localPosition = new Vector3(0f, 0f, distance);
            backgroundQuad.transform.localRotation = Quaternion.identity;
            backgroundQuad.transform.localScale = new Vector3(width * fill, height * fill, 1f);

            Renderer renderer = backgroundQuad.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = backgroundMaterial;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
        }
        else
        {
            Debug.LogError("MenuBackground shader not found, make sure it is in the project.");
        }
    }
}
