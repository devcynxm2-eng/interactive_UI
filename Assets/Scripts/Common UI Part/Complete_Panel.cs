using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Complete_Panel : MonoBehaviour
{
    public TMP_Text completepanel_Txt;
    public GameObject Completepanel;

    [Header("Slide Settings")]
    public Vector2 textSlideInPosition = new Vector2(0, 60);
    public Vector2 textSlideStartPosition = new Vector2(0, -300);
    public float slideDuration = 0.5f;

    public restart_timer timer;
    public ProgressBar progressBarController;

    private Tweener slideTweener;

    public IEnumerator DisplayRoutine(float time)
    {
        // ✅ Safety checks
        if (completepanel_Txt == null || completepanel_Txt.rectTransform == null)
            yield break;

        if (Completepanel == null)
            yield break;

        Debug.Log("Coroutine STARTED");
        yield return new WaitForSeconds(1);
        Debug.Log("Coroutine AFTER WAIT");

        // ✅ Check again after wait — scene may have changed
        if (this == null || completepanel_Txt == null || Completepanel == null)
            yield break;

        Completepanel.SetActive(true);
        Debug.Log("Panel activated");

        if (timer != null)
            timer.StopTimerAndProgress();

        // Set starting position
        completepanel_Txt.rectTransform.anchoredPosition = textSlideStartPosition;
        completepanel_Txt.gameObject.SetActive(true);

        // ✅ Kill previous tween before starting new one
        slideTweener?.Kill();

        // ✅ Store tween reference and use SetLink
        slideTweener = completepanel_Txt.rectTransform
            .DOAnchorPos(textSlideInPosition, slideDuration)
            .SetEase(Ease.OutBack)
            .SetLink(completepanel_Txt.gameObject); // ✅ link to the text object

        yield return new WaitForSeconds(slideDuration);
    }

    public void ResetCompleteUI()
    {
        // ✅ Kill tween before resetting
        slideTweener?.Kill();

        Completepanel?.gameObject.SetActive(false);
        completepanel_Txt?.gameObject.SetActive(false);

        if (timer != null)
            timer.ResumeTimer();

        if (progressBarController != null)
            progressBarController.ResumeProgress();
    }

    // ✅ Kill all tweens when this object is destroyed
    void OnDestroy()
    {
        slideTweener?.Kill();
        DOTween.Kill(gameObject);

        if (completepanel_Txt != null)
            DOTween.Kill(completepanel_Txt.rectTransform);
    }
}