using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TimeUpPanelState : MonoBehaviour
{
    [SerializeField] private Button TryAgainButton;
    [SerializeField] private GameManager GameManagerObject;
    [SerializeField] private ProgressBarController ProgressBarControllerObject;

    // ── CHANGED: added CanvasGroup for fading ──
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // ── CHANGED: get or auto-add CanvasGroup ──
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        Debug.Log("TimeUpPanel Shown ✅");
        TryAgainButton.onClick.AddListener(TryAgain);

        // ── CHANGED: fade in when panel activates ──
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
    }

    private void OnDisable()
    {
        TryAgainButton.onClick.RemoveListener(TryAgain);

        // ── CHANGED: kill any running tween when disabled ──
        canvasGroup.DOKill();
    }

    private void TryAgain()
    {
        // ── CHANGED: fade out first, then hide and restart ──
        canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            gameObject.SetActive(false);
            GameManagerObject.RestartTimer();
            ProgressBarControllerObject.restartprogress();
        });
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
