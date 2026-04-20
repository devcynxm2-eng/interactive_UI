using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TimeUpPanelState : MonoBehaviour
{
    [SerializeField] private Button TryAgainButton;
    [SerializeField] private GameManager GameManagerObject;
    [SerializeField] private ProgressBar ProgressBarControllerObject;

    private CanvasGroup canvasGroup;
    public restart_timer timer;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        Debug.Log("TimeUpPanel Shown ✅");

        if (TryAgainButton != null)
            TryAgainButton.onClick.AddListener(TryAgain);

        canvasGroup.alpha = 0f;

        canvasGroup.DOFade(1f, 0.5f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject); // 🔥 auto-kill safety
    }

    private void OnDisable()
    {
        if (TryAgainButton != null)
            TryAgainButton.onClick.RemoveListener(TryAgain);

        canvasGroup.DOKill();
    }

    private void TryAgain()
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
}


//using System.Collections;
//using DG.Tweening;
//using UnityEngine;
//using UnityEngine.UIElements;


//public class TimeUpPanelState : MonoBehaviour
//{
//    [SerializeField] private UnityEngine.UI.Button TryAgainButton;
//    [SerializeField] private GameManager GameManagerObject;
//    [SerializeField] private ProgressBarController ProgressBarControllerObject;


//    public GameObject TimeUpPanel;

//    private void OnEnable()
//    {
//            TryAgainButton.gameObject.SetActive(false);
//            PositionAdjuster(new Vector2(0,0));

//        TryAgainButton.onClick.AddListener(TryAgain);
//    }

//    private void OnDisable()
//    {

//        TryAgainButton.onClick.RemoveListener(TryAgain);


//    }
//    Coroutine Coroutine;
//    void TryAgain()
//    {
//        GameManagerObject.RestartTimer();
//        ProgressBarControllerObject.restartprogress();
//        PositionAdjuster(new Vector2(-3000, 0));

//        if(Coroutine != null)
//             StopCoroutine(Coroutine);
//        Coroutine = StartCoroutine(DelayedAction(2.0f));


//    }
//    IEnumerator DelayedAction(float delay)
//    {
//        yield return new WaitForSeconds(delay); // Wait for 2 seconds
//        TimeUpPanel.SetActive(false);
//        Debug.Log("Action executed after delay!");
//    }
//    void PositionAdjuster(Vector2 position)
//    {
//        RectTransform rect = GetComponent<RectTransform>();

//        rect.DOAnchorPos(position, 1f).OnComplete(() =>
//        {
//            TryAgainButton.gameObject.SetActive(true);
//        });
//    }



//}