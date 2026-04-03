using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class NumEq : MonoBehaviour
{


    public float duration = 0.5f;


    [Header("UI References")]
    public Image blankImage;
    public TMP_Text completepanel_Txt;
    public Image Completepanel;
    public Image greencorrectimage;
    public Image correctImage;
    public Image wrongImage;
    public TMP_Text resultText;
    public RectTransform rectTransform;
    public GameObject redonwrong;

    [Header("Equation Values")]
    public int secondNumber = 3;
    public int result = 7;

    private MathOperator currentOperator;
    private int selectedNumber = -1;
    private bool answered = false; // prevent double-firing

    // calling from other class
    public GameManager gamemanager;
    public ProgressBarController progressBarController;
    public NumEq resettimer;


    [Header("Slide Settings")]
    public Vector2 slideInPosition = new Vector2(0, 0);    // center
    public Vector2 slideOutPosition = new Vector2(0, -500); // off-screen below
    public float slideDuration = 0.5f;


    private void Start()
    {
        completepanel_Txt.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(false);

    }



    // Called by EquationManager after setting operator from JSON
    public void SetOperatorType(MathOperator op)
    {
        currentOperator = op;
    }

   

    // ─────────────────────────────────────────
    //  UI States
    // ─────────────────────────────────────────
    public void SetBlank()
    {
        answered = false;
        blankImage.gameObject.SetActive(true);
        correctImage.gameObject.SetActive(false);
        wrongImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
    }

    public void SetCorrect(string text)
    {
        gamemanager.RestartTimer();
        progressBarController.restartprogress();
        blankImage.gameObject.SetActive(false);
        
        correctImage.gameObject.SetActive(true);
        wrongImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(true);
        resultText.text = text;

        
        ShowUIForSeconds(1.0f);


    }

    public void ShowUIForSeconds(float duration)
    {
        StartCoroutine(Displaygreen(duration));
        
        StartCoroutine(DisplayRoutine(duration));
    }
    private IEnumerator Displaygreen(float time)
    {
        
        greencorrectimage.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        greencorrectimage.gameObject.SetActive(false);
    }
    private IEnumerator DisplayRoutine(float time)
    {
        yield return new WaitForSeconds(1);
        Completepanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        // Move completepanel to start off-screen

        completepanel_Txt.gameObject.SetActive(true);
        completepanel_Txt.rectTransform.anchoredPosition = slideOutPosition;

        // Slide in
        completepanel_Txt.rectTransform.DOAnchorPos(slideInPosition, slideDuration).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(time);

        // Slide out
        completepanel_Txt.rectTransform.DOAnchorPos(slideOutPosition, slideDuration).SetEase(Ease.InBack);
        Completepanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(slideDuration);

        completepanel_Txt.gameObject.SetActive(false);

        gamemanager.RestartTimer();
        progressBarController.restartprogress();
    }


    public void SetWrong(string text)
    {

        gamemanager.RestartTimer();
        wrongImage.gameObject.SetActive(true);
        redonwrong.SetActive(true);
        blankImage.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(false);
       
        
        resultText.gameObject.SetActive(true);
        resultText.text = text;
        
        TriggerShake();
    }

    public void TriggerShake()
    {
        rectTransform.DOShakeAnchorPos(0.5f, new Vector2(20f, 0f), 10, 90f);
    }

    public void ResetState()
    {
        answered = false;
        selectedNumber = -1;
        SetBlank();
    }

    // ─────────────────────────────────────────
    //  Called externally when player picks a number
    // ─────────────────────────────────────────
    public void SelectNumber(int number)
    {
        if (answered) return; // ignore taps after correct answer

        selectedNumber = number;
        CheckAnswer(number.ToString());
    }

   




    // ─────────────────────────────────────────
    //  Answer checking
    // ─────────────────────────────────────────
    void CheckAnswer(string text)
    {
        bool isCorrect = Calculate(selectedNumber, secondNumber, currentOperator) == result;

        if (isCorrect)
        {
            answered = true; // lock further input
            SetCorrect(text);
            EquationManager.Instance.OnCorrectAnswer(); // → triggers next equation
        }
        else
        {
            SetWrong(text);
        }
    }

    int Calculate(int a, int b, MathOperator op)
    {
        switch (op)
        {
            case MathOperator.Add: return a + b;
            case MathOperator.Subtract: return a - b;
            case MathOperator.Multiply: return a * b;
            case MathOperator.Divide: return b != 0 ? a / b : 0;
            default: return a + b;
        }
    }
}