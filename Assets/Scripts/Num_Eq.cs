using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System;



public class NumEq : MonoBehaviour
{
    public float duration = 0.5f;

    [Header("UI References")]
    public Image blankImage;
    public Image blankImage2;
    public TMP_Text completepanel_Txt;
    public Image Completepanel;
    public Image greencorrectimage;
    public Image correctImage;
    public Image correctImage2;
    public Image wrongImage;
    public TMP_Text resultText;
    public TMP_Text resultText2;
    public RectTransform rectTransform;
    public GameObject redonwrong;

    [Header("Equation Values")]
    public int secondNumber;
    public int result;

    private MathOperator currentOperator;
    private int selectedNumber1 = -1;
    private int selectedNumber2 = -1;
    private int selectedNumber;
    private bool answered = false;

    public GameManager gamemanager;
    public ProgressBarController progressBarController;
    public NumEq resettimer;

    private int currentBlank = 1;

    [Header("Slide Settings")]
    public Vector2 slideInPosition = new Vector2(0, 0);
    public Vector2 slideOutPosition = new Vector2(0, -500);
    public float slideDuration = 0.5f;

    public int eqdata;
    public Action OnCompletePanelFinished;

    private void Start()
    {
        eqdata = EquationManager.currentEquationLength;
        completepanel_Txt.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════
    //   CORE ALGORITHM
    //   Supports ANY two-operator combination:
    //   __ OP1 __ OP2 c = answer
    //   e.g. __ + __ - 3 = 4
    //        __ * __ / 2 = 6
    //        __ - __ + 4 = 3
    // ═══════════════════════════════════════════════════════════════

    // ─────────────────────────────────────────
    //  STEP 1: Strip the constant from the right
    //          to find what the two blanks must produce
    // ─────────────────────────────────────────
    /// <summary>
    /// Reverse the constant's operation to isolate the blanks' combined result.
    /// op2 = the operator BETWEEN blank2 and the constant.
    ///
    ///   __ OP1 __ OP2 c = answer
    ///   blank1 OP1 blank2 = GetTarget(answer, c, op2)
    ///
    /// Examples:
    ///   __ + __ + 3 = 10  →  target = 10 - 3 = 7   (blanks must sum to 7)
    ///   __ + __ - 3 = 4   →  target = 4  + 3 = 7   (blanks must sum to 7)
    ///   __ * __ * 2 = 12  →  target = 12 / 2 = 6   (blanks must multiply to 6)
    ///   __ * __ / 2 = 6   →  target = 6  * 2 = 12  (blanks must multiply to 12)
    /// </summary>
    private int GetTarget(int answer, int constant, string op2)
    {
        switch (op2)
        {
            case "+":
                // blank_result + c = answer  →  blank_result = answer - c
                return answer - constant;

            case "-":
                // blank_result - c = answer  →  blank_result = answer + c
                return answer + constant;

            case "*":
                // blank_result * c = answer  →  blank_result = answer / c
                return constant != 0 ? answer / constant : answer;

            case "/":
                // blank_result / c = answer  →  blank_result = answer * c
                return answer * constant;

            default:
                return answer - constant;
        }
    }

    // ─────────────────────────────────────────
    //  STEP 2: Validate blank1
    //          Based on op1 (operator between blank1 and blank2)
    //          and the target the two blanks must reach
    // ─────────────────────────────────────────
    /// <summary>
    /// Checks if blank1 is a valid FIRST pick.
    /// op1 = the operator BETWEEN blank1 and blank2.
    ///
    /// Rules:
    ///   +  →  blank1 must be < target  (leave room for blank2 >= 1)
    ///   -  →  blank1 must be > target  (so blank2 = blank1 - target >= 1)
    ///   *  →  blank1 must be a factor of target
    ///   /  →  target must divide blank1 cleanly  (blank1 / blank2 = target → blank1 = target * blank2)
    /// </summary>
    private bool IsBlank1Valid(int blank1, int target, string op1)
    {
        switch (op1)
        {
            case "+":
                // blank1 + blank2 = target
                // blank2 = target - blank1, need blank2 >= 1 so blank1 < target
                bool addValid = blank1 >= 1 && blank1 < target;
                Debug.Log($"[IsBlank1Valid] + : blank1={blank1}, target={target}, valid={addValid}");
                return addValid;

            case "-":
                // blank1 - blank2 = target
                // blank2 = blank1 - target, need blank2 >= 1 so blank1 > target
                bool subValid = blank1 >= 1 && blank1 > target;
                Debug.Log($"[IsBlank1Valid] - : blank1={blank1}, target={target}, valid={subValid}");
                return subValid;

            case "*":
                // blank1 * blank2 = target
                // blank1 must be a factor of target so blank2 is a whole number
                bool mulValid = blank1 >= 1 && target % blank1 == 0;
                Debug.Log($"[IsBlank1Valid] * : blank1={blank1}, target={target}, valid={mulValid}");
                return mulValid;

            case "/":
                // blank1 / blank2 = target
                // blank1 must be divisible by target  (e.g. 12 / ? = 4  →  blank1 must be multiple of 4)
                bool divValid = blank1 >= 1 && target != 0 && blank1 % target == 0;
                Debug.Log($"[IsBlank1Valid] / : blank1={blank1}, target={target}, valid={divValid}");
                return divValid;

            default:
                return blank1 >= 1 && blank1 < target;
        }
    }

    // ─────────────────────────────────────────
    //  STEP 3: Validate blank2
    //          Given blank1 is already chosen,
    //          check if blank1 OP1 blank2 == target EXACTLY
    // ─────────────────────────────────────────
    /// <summary>
    /// Checks if blank2 completes the equation exactly.
    /// op1 = the operator BETWEEN blank1 and blank2.
    ///
    /// This is an EXACT check — we don't just check range,
    /// we verify the full result equals target.
    /// </summary>
    private bool IsBlank2Valid(int blank1, int blank2, int target, string op1)
    {
        switch (op1)
        {
            case "+":
                // blank1 + blank2 must == target exactly
                bool addValid = blank2 >= 1 && (blank1 + blank2) == target;
                Debug.Log($"[IsBlank2Valid] + : {blank1}+{blank2}={blank1 + blank2}, target={target}, valid={addValid}");
                return addValid;

            case "-":
                // blank1 - blank2 must == target exactly
                bool subValid = blank2 >= 1 && (blank1 - blank2) == target;
                Debug.Log($"[IsBlank2Valid] - : {blank1}-{blank2}={blank1 - blank2}, target={target}, valid={subValid}");
                return subValid;

            case "*":
                // blank1 * blank2 must == target exactly
                bool mulValid = blank2 >= 1 && (blank1 * blank2) == target;
                Debug.Log($"[IsBlank2Valid] * : {blank1}*{blank2}={blank1 * blank2}, target={target}, valid={mulValid}");
                return mulValid;

            case "/":
                // blank1 / blank2 must == target exactly, and divide cleanly
                bool divValid = blank2 != 0 && blank1 % blank2 == 0 && (blank1 / blank2) == target;
                Debug.Log($"[IsBlank2Valid] / : {blank1}/{blank2}={(blank2 != 0 ? blank1 / blank2 : 0)}, target={target}, valid={divValid}");
                return divValid;

            default:
                return blank2 >= 1 && (blank1 + blank2) == target;
        }
    }

    // ─────────────────────────────────────────
    //  STEP 4: Final correct check
    //          Same logic as IsBlank2Valid but
    //          explicitly for the final answer verify
    // ─────────────────────────────────────────
    private bool IsCorrect(int blank1, int blank2, int target, string op1)
    {
        switch (op1)
        {
            case "+":
                bool addCorrect = blank1 + blank2 == target;
                Debug.Log($"[IsCorrect] + : {blank1}+{blank2}={blank1 + blank2}, target={target}, correct={addCorrect}");
                return addCorrect;

            case "-":
                bool subCorrect = blank1 - blank2 == target;
                Debug.Log($"[IsCorrect] - : {blank1}-{blank2}={blank1 - blank2}, target={target}, correct={subCorrect}");
                return subCorrect;

            case "*":
                bool mulCorrect = blank1 * blank2 == target;
                Debug.Log($"[IsCorrect] * : {blank1}*{blank2}={blank1 * blank2}, target={target}, correct={mulCorrect}");
                return mulCorrect;

            case "/":
                bool divCorrect = blank2 != 0 && blank1 / blank2 == target;
                Debug.Log($"[IsCorrect] / : {blank1}/{blank2}={(blank2 != 0 ? blank1 / blank2 : 0)}, target={target}, correct={divCorrect}");
                return divCorrect;

            default:
                return blank1 + blank2 == target;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //   MAIN ENTRY POINT FOR DOUBLE BLANK EQUATIONS
    //   Called by GameManager.SelectNumber() when equation_length == 7
    // ═══════════════════════════════════════════════════════════════
    public void SelectNumberdouble(int number)
    {
       //// if (answered) return;

        // Get operators from current equation data
        // operators[0] = OP1 = between blank1 and blank2   e.g. "+"
        // operators[1] = OP2 = between blank2 and constant e.g. "-"
        string op1 = EquationManager.Instance.currentData.operators[0];
        string op2 = EquationManager.Instance.currentData.operators[1];

        // STEP 1: Strip the constant to find what blanks must produce
        int target = GetTarget(result, secondNumber, op2);
        Debug.Log($"[SelectNumberdouble] number={number}, op1={op1}, op2={op2}, constant={secondNumber}, answer={result}, target={target}");

        if (currentBlank == 1)
        {
            // STEP 2: Is blank1 a valid first pick?
            if (!IsBlank1Valid(number, target, op1))
            {
                Debug.Log($"[SelectNumberdouble] REJECTED blank1={number}");

                resultText.text = number.ToString();
                redonwrong.gameObject.SetActive(true);
                TriggerShake();
                return;
            }

            // blank1 accepted
            selectedNumber1 = number;
            redonwrong.gameObject.SetActive(false);
            greencorrectimage.gameObject.SetActive(true);
            resultText.text = number.ToString();
            currentBlank = 2;
            
            
            
            Debug.Log($"[SelectNumberdouble] ACCEPTED blank1={number}, waiting for blank2...");
        }
        else if (currentBlank == 2)
        {
            // STEP 3: Does blank2 complete the equation exactly?
            if (!IsBlank2Valid(selectedNumber1, number, target, op1))
            {
                Debug.Log($"[SelectNumberdouble] REJECTED blank2={number}");
                resultText2.text = number.ToString();
                redonwrong.gameObject.SetActive(true);
                greencorrectimage.gameObject.SetActive(false);
                TriggerShake();
                return;
            }

            // blank2 accepted
            selectedNumber2 = number;
            redonwrong.gameObject.SetActive(false);
            greencorrectimage.gameObject.SetActive(true);
            resultText2.text = number.ToString();
            currentBlank = 1;
            Debug.Log($"[SelectNumberdouble] ACCEPTED blank2={number}");

            // STEP 4: Final correct check
            if (IsCorrect(selectedNumber1, selectedNumber2, target, op1))
            {
                Debug.Log($"[SelectNumberdouble] ✓ CORRECT! {selectedNumber1} {op1} {selectedNumber2} {op2} {secondNumber} = {result}");
                answered = true;
                SetCorrect();
            }
            else
            {
                Debug.Log($"[SelectNumberdouble] ✗ WRONG! {selectedNumber1} {op1} {selectedNumber2} {op2} {secondNumber} ≠ {result}");
                Debug.Log("-----------------> set wrong debug log <---------------");
                SetWrong();
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //   SINGLE BLANK EQUATIONS  (__  OP  c  =  answer)
    //   Called by GameManager.SelectNumber() when equation_length == 5
    // ═══════════════════════════════════════════════════════════════
    public void SelectNumber(int number)
    {
        if (answered) return;
        selectedNumber = number;
        CheckAnswer(number.ToString());
    }

    void CheckAnswer(string text)
    {
        bool isCorrect = Calculate(selectedNumber, secondNumber, currentOperator) == result;

        if (isCorrect)
        {
            answered = true;
            SetCorrectsingle(text);
            EquationManager.Instance.OnCorrectAnswer();
        }
        else
        {
            SetWrongsingle(text);
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

    // ═══════════════════════════════════════════════════════════════
    //   UI METHODS (unchanged from your original)
    // ═══════════════════════════════════════════════════════════════
    public void SetOperatorType(MathOperator op)
    {
        currentOperator = op;
        Debug.Log($"[SetOperatorType] Operator set to: {op}");
    }

    public void SetBlank(int blankIndex)
    {
        answered = false;
        Debug.LogError("= > " + blankIndex);

        if (blankIndex == 1)
        {
            selectedNumber1 = -1;
            if (resultText != null) resultText.text = " ";
            currentBlank = 1;
            blankImage.gameObject.SetActive(true);
        }
        else if (blankIndex == 2)
        {
            selectedNumber2 = -1;
            if (resultText2 != null) resultText2.text = " ";
            currentBlank = 2;
            blankImage.gameObject.SetActive(true);
        }

        correctImage.gameObject.SetActive(false);
        wrongImage.gameObject.SetActive(false);
    }
  public void ResetState()
    {
        answered = false;
        selectedNumber1 = -1;
        SetBlank(1);

    }

    public void ResetStatemgr()
    {
        answered = false;
        selectedNumber1 = -1;
       //SetBlank(1);

    }

    public void SetBlanksingle()
    {
        answered = false;
        blankImage.gameObject.SetActive(true);
        correctImage.gameObject.SetActive(false);
        wrongImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
    }

  

    public void ResetStatesingle()
    {
        answered = false;
        selectedNumber = -1;
        SetBlanksingle();
    }

    public void SetCorrect()
    {
        gamemanager.RestartTimer();
        progressBarController.restartprogress();
        blankImage.gameObject.SetActive(false);
        blankImage2.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(true);
        correctImage2.gameObject.SetActive(true);
        wrongImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(true);
        resultText2.gameObject.SetActive(true);
        ShowUIForSeconds(1.0f);
        StartCoroutine(ShowUIThenNext());
    }





    public void SetWrong()
    {
        gamemanager.RestartTimer();

        Debug.Log("----------------->  make it red on wrong");
        wrongImage.gameObject.SetActive(true);
        redonwrong.SetActive(true);

        Debug.Log("-----------------> blank image false");
        blankImage.gameObject.SetActive(false);
        Debug.Log("-----------------> correct image false");
        correctImage.gameObject.SetActive(false);
        Debug.Log("-----------------> input  image true");
        resultText.gameObject.SetActive(true);
      

        blankImage2.gameObject.SetActive(false);
        correctImage2.gameObject.SetActive(false);
        Debug.Log("-----------------> blank image2 false");
        resultText2.gameObject.SetActive(true);
        

        TriggerShake();


        
    }

    public void SetCorrectsingle(string text)
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

    public void SetWrongsingle(string text)
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





    IEnumerator ShowUIThenNext()
    {
        yield return new WaitForSeconds(2.5f); // match your animation timing
        EquationManager.Instance.OnCorrectAnswer();
    }

    public void ShowUIForSeconds(float duration)
    {
        Debug.Log("==================> Working show ui dueration part ");
        StartCoroutine(Displaygreen(duration));
        Debug.Log("==================> Working show ui dueration part start coroutine display grenn ");
        StartCoroutine(DisplayRoutine(duration));
        Debug.Log("==================> Working show ui dueration part start coroutine display show ui end ");
    }

    private IEnumerator Displaygreen(float time)
    {
        Debug.Log("==================> Working show ui dueration part display green part ");
        greencorrectimage.gameObject.SetActive(true);
        Debug.Log("==================> Working show ui dueration part display green part w8ng");
        yield return new WaitForSeconds(time);
        greencorrectimage.gameObject.SetActive(false);
        Debug.Log("==================> Working show ui dueration part display green part end ");
    }

    private IEnumerator DisplayRoutine(float time)
    {
        yield return new WaitForSeconds(1);
        Completepanel.gameObject.SetActive(true);

        // Make text visible and set starting position
        completepanel_Txt.gameObject.SetActive(true);
        completepanel_Txt.rectTransform.anchoredPosition = slideOutPosition;

        // Slide in
        yield return completepanel_Txt.rectTransform
            .DOAnchorPos(slideInPosition, slideDuration)
            .SetEase(Ease.OutBack)
            .WaitForCompletion(); // wait until slide-in finishes

        // Keep text visible for a moment
        yield return new WaitForSeconds(1f);

        // Slide out
        yield return completepanel_Txt.rectTransform
            .DOAnchorPos(slideOutPosition, slideDuration)
            .SetEase(Ease.InBack)
            .WaitForCompletion(); // wait until slide-out finishes

        // Hide panel and text
        completepanel_Txt.gameObject.SetActive(false);
        Completepanel.gameObject.SetActive(false);

        // Restart timer & progress bar
        gamemanager.RestartTimer();
        progressBarController.restartprogress();
    }


    public void TriggerShake()
    {
        rectTransform.DOShakeAnchorPos(0.5f, new Vector2(20f, 0f), 10, 90f);
    }

    public void ResetCompleteUI()
    {
        Completepanel?.gameObject.SetActive(false);
        completepanel_Txt?.gameObject.SetActive(false);
    }
}