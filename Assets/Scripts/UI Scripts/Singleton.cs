using Unity.VisualScripting;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    [Header("Singleton")]
    [SerializeField]
    private bool IsPersistent = false;

    public static T Instance { get; private set; }

    public virtual bool IsInitialized
    {
        get { return Instance != null; }
    }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Trying to instantiate a second instance of singleton class{GetType().Name}");
            Destroy(this);
        }
        else
        {
            Instance = (T)this;

            if (IsPersistent)
            {
                DontDestroyOnLoad(this);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
