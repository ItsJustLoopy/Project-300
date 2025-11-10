using UnityEngine;

public class ScreenBase : MonoBehaviour
{
    protected ScreenManager ScreenManager;

    private void Awake()
    {
        ScreenManager = transform.parent.GetComponent<ScreenManager>();
    }
}
