using UnityEditor;
using UnityEngine;

public class Menu : ScreenBase
{
    Animator MenuAnimator;

    private void Awake()
    {
        MenuAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        MenuAnimator.enabled = true;
        MenuAnimator.Play("Show", -1, 0);
    }

    private void OnDisable()
    {
        MenuAnimator.enabled = false;
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #else
        Application.Quit();
        #endif
    }
}
