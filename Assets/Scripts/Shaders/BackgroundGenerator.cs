using UnityEngine;

public static class BackgroundGenerator
{
    public static void CreateAnimatedBackground(Camera camera)
    {
        // makes a canvas
        GameObject backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backgroundQuad.name = "AnimatedBackground";
        Object.Destroy(backgroundQuad.GetComponent<Collider>());
        
        // sets the quad position and rotation based on the camera
        backgroundQuad.transform.position = camera.transform.position + camera.transform.forward * 50f;
        backgroundQuad.transform.rotation = Quaternion.LookRotation(camera.transform.forward); 
        
        // sets the quad size based on the camera
        float distance = 50f;
        float height = 2.0f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * camera.aspect;
        backgroundQuad.transform.localScale = new Vector3(width * 1.5f, height * 1.5f, 1f);
        
        
        Shader shader = Shader.Find("AnimatedBackground.shader");
        if (shader != null)
        {
            Material backgroundMaterial = new Material(shader);
            
            backgroundMaterial.SetColor("_DarkColor", new Color(0.1f, 0.1f, 0.15f, 1f));
            backgroundMaterial.SetColor("_BrightColor", new Color(0.7f, 0.7f, 0.8f, 1f));
            backgroundMaterial.SetFloat("_AnimationSpeed", 0.5f);
            backgroundQuad.GetComponent<Renderer>().material = backgroundMaterial;
            Debug.Log("Animated Background shader found");
        }
        else
        {
            Debug.LogError("AnimatedBackground shader not found, make sure the shadeer is in the project.");
        }
    }
}
