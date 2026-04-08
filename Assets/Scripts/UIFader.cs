using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFader : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDuration = 1.0f; 
    private Coroutine fadeLoopCoroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log("CanvasGroup was missing, so one was added automatically.");
        }
    }

    void OnEnable()
    {
        
        fadeLoopCoroutine = StartCoroutine(FadeLoop());
    }

    void OnDisable()
    {
        
        if (fadeLoopCoroutine != null)
        {
            StopCoroutine(fadeLoopCoroutine);
        }
    }

    private IEnumerator FadeLoop()
    {
        while (true)
        {
            
            yield return FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration);
          
            yield return FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, fadeDuration);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        cg.alpha = endAlpha;

        cg.interactable = endAlpha > 0f;
        cg.blocksRaycasts = endAlpha > 0f;
    }


    public void SetVisible(bool visible)
    {
        gameObject.SetActive(true); // ensure active to allow fading
       // StopAllCoroutines();
        fadeLoopCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, visible ? 1f : 0f, fadeDuration));
    }
}