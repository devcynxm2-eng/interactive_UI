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
        if (rectTransform == null) return; // ✅ safety check

        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();

        currentTween = rectTransform
            .DOScale(originalScale * scaleFactor, duration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject); // ✅ auto-kill when object destroyed
    }

    public void Hasnat1()
    {
        if (rectTransform == null) return; // ✅ safety check

        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();

        currentTween = rectTransform
            .DOScale(originalScale, duration)
            .SetEase(Ease.InOutQuad)
            .SetLink(gameObject); // ✅ auto-kill when object destroyed
    }

    // ✅ Kill tween when this object is destroyed
    void OnDestroy()
    {
        currentTween?.Kill();
    }
}