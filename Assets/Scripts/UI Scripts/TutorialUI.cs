using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float visibleSeconds = 4.0f;

    private Coroutine _routine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideImmediate();
    }

    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return Fade(1f);
        yield return new WaitForSecondsRealtime(visibleSeconds);
        yield return Fade(0f);
        _routine = null;
    }

    private IEnumerator Fade(float target)
    {
        if (canvasGroup == null)
            yield break;

        float start = canvasGroup.alpha;
        float t = 0f;

        canvasGroup.blocksRaycasts = target > 0.01f;
        canvasGroup.interactable = target > 0.01f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = fadeDuration <= 0f ? 1f : (t / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}