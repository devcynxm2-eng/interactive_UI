using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Complete_Panel : MonoBehaviour
{
    public TMP_Text completepanel_Txt;//================================>
    public GameObject Completepanel;//===========================================>


    [Header("Slide Settings")]
    public Vector2 textSlideInPosition = new Vector2(0, 60);    // where the text lands =================================>
    public Vector2 textSlideStartPosition = new Vector2(0, -300);
    public float slideDuration = 0.5f;



    public restart_timer timer;
    public ProgressBar progressBarController;


    public IEnumerator DisplayRoutine(float time)
    {
        if (completepanel_Txt == null || completepanel_Txt.rectTransform == null)
            yield break;






        Debug.Log("Coroutine STARTED");

        yield return new WaitForSeconds(1);

        Debug.Log("Coroutine AFTER WAIT"); // 👈 CHECK THIS

        Completepanel.SetActive(true);
        Debug.Log("Panel activated");
        if (timer != null)
            timer.StopTimerAndProgress();
        // Set starting positions (off screen)
        completepanel_Txt.rectTransform.anchoredPosition = textSlideStartPosition;

        completepanel_Txt.gameObject.SetActive(true);

        // Slide text to its position
        completepanel_Txt.rectTransform
            .DOAnchorPos(textSlideInPosition, slideDuration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);

        // Slide button to its own separate position with a slight delay


        yield return new WaitForSeconds(slideDuration);

        //gamemanager.RestartTimer();
        //progressBarController.restartprogress();
    }



    public void ResetCompleteUI()
    {
        Completepanel?.gameObject.SetActive(false);
        completepanel_Txt?.gameObject.SetActive(false);


        if (timer != null)
            timer.ResumeTimer();

        if (progressBarController != null)
            progressBarController.ResumeProgress();

    }




}