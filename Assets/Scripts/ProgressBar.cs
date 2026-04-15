using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProgressBarController : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private float duration = 30f;

    private Tweener fillTweener;
    private Tweener countdownTweener;

    void Start()
    {
        if (progressSlider == null)
        {
            Debug.LogError("ProgressSlider is NOT assigned!");
            return;
        }

        // Setup slider
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.value = 1f;
        progressSlider.interactable = false;

        StartCountdown();
    }

    void StartCountdown()
    {
        if (progressSlider == null) return;

        // Kill previous tweens safely
        countdownTweener?.Kill();
        fillTweener?.Kill();

        countdownTweener = DOTween.To(
            () => progressSlider.value,
            x => progressSlider.value = x,
            0f,
            duration
        )
        .SetEase(Ease.Linear)
        .SetLink(gameObject) // 🔥 Auto-kill when object destroyed
        .OnComplete(() =>
        {
            // Safety check before calling anything
            if (this == null || progressSlider == null) return;

            Debug.Log("Progress finished");
            //restartprogress();
        });
    }

    public void restartprogress()
    {
        if (progressSlider == null) return;

        // Kill existing tweens
        countdownTweener?.Kill();
        fillTweener?.Kill();

        fillTweener = DOTween.To(
            () => progressSlider.value,
            x => progressSlider.value = x,
            1f,
            0.3f
        )
        .SetEase(Ease.OutCubic)
        .SetLink(gameObject) // 🔥 Important safety
        .OnComplete(() =>
        {
            if (this == null || progressSlider == null) return;

            StartCountdown();
        });
    }

    // Pause timer
    public void PauseCountdown()
    {
        countdownTweener?.Pause();
    }

    // Resume timer
    public void ResumeCountdown()
    {
        countdownTweener?.Play();
    }

    // Cleanup (VERY IMPORTANT)
    void OnDestroy()
    {
        countdownTweener?.Kill();
        fillTweener?.Kill();
    }
}