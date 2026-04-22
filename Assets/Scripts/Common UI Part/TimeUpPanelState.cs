using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TimeUpPanelState : MonoBehaviour
{

    public static TimeUpPanelState Instance;

    [SerializeField] private Button TryAgainButton;
    [SerializeField] private GameManager GameManagerObject;
    [SerializeField] private ProgressBar ProgressBarControllerObject;

    private CanvasGroup canvasGroup;
    public restart_timer timer;
    private void Awake()
    {
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 1f; // 🔥 IMPORTANT RESET

        canvasGroup.DOKill(); // stop old tweens

        Debug.Log("TimeUpPanel Shown");
    }

    public void TryAgain()
    {
        // 🔥 safety checks first
        if (canvasGroup == null) return;

        canvasGroup.DOFade(0f, 0.3f)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                // 🔥 extra safety (this fixes your crash)
                if (timer != null)
                    timer.RestartTimerExternally();

                gameObject.SetActive(false);
            });
        ProgressBarControllerObject.restartprogress();
    }


    public void TryAgainword()
    {
        // 🔥 safety checks first
        if (canvasGroup == null) return;

        canvasGroup.DOFade(0f, 0.3f)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                // 🔥 extra safety (this fixes your crash)
                if (timer != null)
                    timer.ResetTimerAndProgress();



                gameObject.SetActive(false);
            });
        ProgressBarControllerObject.restartprogress();
    }
}

