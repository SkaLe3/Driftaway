using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFade : MonoBehaviour
{
    [SerializeField] float FadeInDuration = 1f;
    [SerializeField] float FadeOutDuration = 1f;
    private CanvasGroup CanvasGroup;


    private Coroutine FadeInCoroutine;
    private Coroutine FadeOutCoroutine;

    private void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn()
    {
        if (FadeOutCoroutine != null)
            StopCoroutine(FadeOutCoroutine);
        FadeOutCoroutine = null;
        FadeInCoroutine = StartCoroutine(FadeToAlpha(1f, FadeInDuration));
        Enable();
    }

    public void FadeOut()
    {
        if (FadeInCoroutine != null)
            StopCoroutine(FadeInCoroutine);  
        FadeInCoroutine = null;
        FadeOutCoroutine = StartCoroutine(FadeToAlpha(0f, FadeOutDuration));
        Disable();
    }

    public void Enable()
    {
        CanvasGroup canvas = GetComponent<CanvasGroup>();
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
        CanvasGroup.alpha = 1f;
    }

    public void Disable()
    {
        CanvasGroup canvas = GetComponent<CanvasGroup>();
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
        CanvasGroup.alpha = 0f;
    }
    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float startAlpha = CanvasGroup.alpha;  
        float time = 0f;

        while (time < duration)
        {
            CanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        CanvasGroup.alpha = targetAlpha;
    }
}