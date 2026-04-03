using UnityEngine;
using DG.Tweening;

public class HoverScaler : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    public float scaleFactor = 1.1f;
    public float duration = 2f;

    private Tween currentTween;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void Hasnat()
    {
        
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();  //  => to kill the previous animation 

        currentTween = rectTransform
            .DOScale(originalScale * scaleFactor, duration)
            .SetEase(Ease.OutBack);
    }

    public void Hasnat1()
    {
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();

        currentTween = rectTransform
            .DOScale(originalScale, duration)
            .SetEase(Ease.InOutQuad);
    }
}