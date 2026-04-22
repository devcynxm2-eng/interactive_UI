using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System;
using System.Security.Cryptography.X509Certificates;



public class NumEq : MonoBehaviour
{
    public float duration = 0.5f;

    [Header("UI References")]
    public Image blankImage;
    public Image blankImage2;
    //public TMP_Text completepanel_Txt;//================================>
    //public Image Completepanel;//===========================================>
    public Image greencorrectimage;
    public Image correctImage;
    public Image correctImage2;
    public Image wrongImage;
    public TMP_Text resultText;
    public TMP_Text resultText2;
    public RectTransform rectTransform;
    public RectTransform rectTransform2;
    public GameObject redonwrong;
    public GameObject Cross_Image;
    public GameObject nextbuttonin_completepanel;
    //public GameObject change_no_Text;


    //testing new approach
    private int hintValueForBlank2 = -1;


    [Header("Equation Values")]
    public int secondNumber;
    public int result;

    private MathOperator currentOperator;
    private int selectedNumber1 = -1;
    private int selectedNumber2 = -1;
    private int selectedNumber;
    private bool answered = false;

    public GameManager gamemanager;
    public ProgressBar progressBarController;
    public NumEq resettimer;

    public int CurrentBlank = 1;




    public int eqdata;
    public Action OnCompletePanelFinished;



    // simple check 
    public int hintcheck = 0;
    //help in check
    public int chknum;
    private Vector2 originalPosition;


    // taking data from other class
    public ScoreManager scoreManager;
    public Complete_Panel Complete_Panel;

    private void Start()
    {

        eqdata = EquationManager.currentEquationLength;
        Complete_Panel.completepanel_Txt.gameObject.SetActive(false);// =================================>
        correctImage.gameObject.SetActive(false);
        scoreManager.updateScoreui();
        originalPosition = rectTransform.anchoredPosition;
    }
    public int SelectedBlank1
    {
        get { return selectedNumber1; }
    }



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


    private bool IsBlank1Valid(int blank1, int target, string op1)
    {
        switch (op1)
        {
            case "+":
                bool addValid = blank1 >= 0 && blank1 < target;
                Debug.Log($"[IsBlank1Valid] + : blank1={blank1}, target={target}, valid={addValid}");
                return addValid;

            case "-":
                bool subValid = blank1 >= 0 && blank1 > target;
                Debug.Log($"[IsBlank1Valid] - : blank1={blank1}, target={target}, valid={subValid}");
                return subValid;

            case "*":
                // Guard against divide by zero
                bool mulValid = blank1 == 0 ? target == 0 : (blank1 > 0 && target % blank1 == 0);
                Debug.Log($"[IsBlank1Valid] * : blank1={blank1}, target={target}, valid={mulValid}");
                return mulValid;

            case "/":
                bool divValid = blank1 >= 0 && target != 0 && blank1 % target == 0;
                Debug.Log($"[IsBlank1Valid] / : blank1={blank1}, target={target}, valid={divValid}");
                return divValid;

            default:
                return blank1 >= 0 && blank1 < target;
        }
    }


    private bool IsBlank2Valid(int blank1, int blank2, int target, string op1)
    {
        switch (op1)
        {
            case "+":
                bool addValid = blank2 >= 0 && (blank1 + blank2) == target;
                Debug.Log($"[IsBlank2Valid] + : {blank1}+{blank2}={blank1 + blank2}, target={target}, valid={addValid}");
                return addValid;

            case "-":
                bool subValid = blank2 >= 0 && (blank1 - blank2) == target;
                Debug.Log($"[IsBlank2Valid] - : {blank1}-{blank2}={blank1 - blank2}, target={target}, valid={subValid}");
                return subValid;

            case "*":
                bool mulValid = blank2 >= 0 && (blank1 * blank2) == target;
                Debug.Log($"[IsBlank2Valid] * : {blank1}*{blank2}={blank1 * blank2}, target={target}, valid={mulValid}");
                return mulValid;

            case "/":
                // blank2 cannot be 0 for division
                bool divValid = blank2 != 0 && blank1 % blank2 == 0 && (blank1 / blank2) == target;
                Debug.Log($"[IsBlank2Valid] / : {blank1}/{blank2}={(blank2 != 0 ? blank1 / blank2 : 0)}, target={target}, valid={divValid}");
                return divValid;

            default:
                return blank2 >= 0 && (blank1 + blank2) == target;
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

        if (CurrentBlank == 1)
        {

            chknum = number;
            // STEP 2: Is blank1 a valid first pick?
            if (!IsBlank1Valid(number, target, op1))
            {
                Debug.Log($"[SelectNumberdouble] REJECTED blank1={number}");
                SoundManager.Instance.PlayWrong();
                resultText.text = number.ToString();
                wrongImage.gameObject.SetActive(true);
                TriggerShake();
                Cross_Image.gameObject.SetActive(true);

                return;
            }

            // blank1 accepted
            selectedNumber1 = number;


            // ✅ Calculate blank2 hint value immediately at runtime
            string op1Temp = EquationManager.Instance.currentData.operators[0];
            string op2Temp = EquationManager.Instance.currentData.operators[1];
            int targetTemp = GetTarget(result, secondNumber, op2Temp);
            hintValueForBlank2 = CalculateBlank2(selectedNumber1, targetTemp, op1Temp);
            Debug.Log($"[Blank2 Hint Pre-calculated] hintValueForBlank2 = {hintValueForBlank2}");
            EquationManager.Instance.EnsureValueOnButton(hintValueForBlank2);
            correctImage.gameObject.SetActive(true);
            wrongImage.gameObject.SetActive(false);
            resultText.text = number.ToString();
            CurrentBlank = 2;

            Debug.Log("=============-------------- > " + selectedNumber1);

            hintcheck++;


            CurrentBlank = 2;
            if (EquationManager.Instance != null && EquationManager.Instance.HasDoubleHint())
            {
                EquationManager.Instance.ShowHintForCurrentBlank();
            }
            GameManager.Instance.timer.RestartTimerExternally();

            Debug.Log("progress bar restarted --------> ");
            progressBarController.restartprogress();



            Debug.Log($"[SelectNumberdouble] ACCEPTED blank1={number}, waiting for blank2...");
        }
        else if (CurrentBlank == 2)
        {
            // STEP 3: Does blank2 complete the equation exactly?
            if (!IsBlank2Valid(selectedNumber1, number, target, op1))
            {
                SoundManager.Instance.PlayWrong();
                Debug.Log($"[SelectNumberdouble] REJECTED blank2={number}");
                resultText2.text = number.ToString();
                redonwrong.gameObject.SetActive(true);
                greencorrectimage.gameObject.SetActive(false);
                //change_no_Text.SetActive(true);

                //scoreManager.Subtractscore(5);
                TriggerShake();
                return;
            }

            // blank2 accepted
            selectedNumber2 = number;
            redonwrong.gameObject.SetActive(false);
            greencorrectimage.gameObject.SetActive(true);
            resultText2.text = number.ToString();
            CurrentBlank = 1;
            Debug.Log($"[SelectNumberdouble] ACCEPTED blank2={number}");








            // STEP 4: Final correct check
            if (IsCorrect(selectedNumber1, selectedNumber2, target, op1))
            {
                Debug.Log($"[SelectNumberdouble] ✓ CORRECT! {selectedNumber1} {op1} {selectedNumber2} {op2} {secondNumber} = {result}");
                answered = true;
                SetCorrect();

                scoreManager.addscore(10);

                SoundManager.Instance.PlayCorrect();

            }
            else
            {
                Debug.Log($"[SelectNumberdouble] ✗ WRONG! {selectedNumber1} {op1} {selectedNumber2} {op2} {secondNumber} ≠ {result}");
                Debug.Log("-----------------> set wrong debug log <---------------");
                SoundManager.Instance.PlayWrong();
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


            scoreManager.addscore(10);
        }
        else
        {

            SetWrongsingle(text);
            //scoreManager.Subtractscore(5);
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


        if (blankIndex == 1)
        {
            selectedNumber1 = -1;
            if (resultText != null) resultText.text = " ";
            CurrentBlank = 1;
            blankImage.gameObject.SetActive(true);
        }
        else if (blankIndex == 2)
        {
            selectedNumber2 = -1;
            if (resultText2 != null) resultText2.text = " ";
            CurrentBlank = 2;
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

        SoundManager.Instance.PlayCorrect();

        EquationManager.Instance.SaveProgress();
        //gamemanager.RestartTimer();
        Debug.Log("progress bar restarted --------> ");
        //progressBarController.restartprogress();
        GameSession.RegisterEquationSolved();

        blankImage.gameObject.SetActive(false);
        blankImage2.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(true);
        correctImage2.gameObject.SetActive(true);
        wrongImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(true);
        resultText2.gameObject.SetActive(true);

        ShowUIForSeconds(1.0f);

        //StartCoroutine(ShowUIThenNext());

    }





    public void SetWrong()
    {

        SoundManager.Instance.PlayWrong();

        //gamemanager.RestartTimer();

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
        SoundManager.Instance.PlayCorrect();
        EquationManager.Instance.SaveProgress();
        //gamemanager.RestartTimer();
        GameSession.RegisterEquationSolved();

        progressBarController.restartprogress();
        Debug.Log("progress bar restarted --------> ");
        blankImage.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(true);
        wrongImage.gameObject.SetActive(false);

        resultText.gameObject.SetActive(true);
        resultText.text = text;

        ShowUIForSeconds(1.0f);

    }

    public void SetWrongsingle(string text)
    {
        SoundManager.Instance.PlayWrong();
        //gamemanager.RestartTimer();
        wrongImage.gameObject.SetActive(true);
        redonwrong.SetActive(true);
        blankImage.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(false);
        resultText.gameObject.SetActive(true);
        resultText.text = text;
        TriggerShake();
    }


    public void ResetBothBlanks()
    {
        StartCoroutine(ResetAfterWrong());
    }

    private IEnumerator ResetAfterWrong()
    {
        yield return new WaitForSeconds(0.8f);

        cleartextfield();

        blankImage.gameObject.SetActive(true);
        blankImage2.gameObject.SetActive(true);
        correctImage.gameObject.SetActive(false);
        correctImage2.gameObject.SetActive(false);
        wrongImage.gameObject.SetActive(false);
        redonwrong.SetActive(false);
        resultText.text = " ";
        resultText2.text = " ";

        CurrentBlank = 1;
        selectedNumber1 = -1;
        selectedNumber2 = -1;
        answered = false;
    }




    public void ShowUIForSeconds(float duration)
    {
        Debug.Log("==================> Working show ui dueration part ");
        StartCoroutine(Displaygreen(duration));
        Debug.Log("==================> Working show ui dueration part start coroutine display grenn ");
        StartCoroutine(Complete_Panel.DisplayRoutine(duration));
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


    public void TriggerShake()
    {
        rectTransform.DOKill();

        // ✅ Reset position first then shake
        rectTransform.anchoredPosition = originalPosition;

        rectTransform.DOShakeAnchorPos(0.5f, new Vector2(20f, 0f), 10, 90f);
    }




    public void cleartextfield()
    {
        CurrentBlank = 1;
        resultText.text = " ";
        resultText2.text = " ";
        selectedNumber1 = -1;
        selectedNumber2 = -1;
        chknum = -1;
        hintcheck = 0;
        hintValueForBlank2 = -1; // ✅ reset
        redonwrong.gameObject.SetActive(false);
        correctImage.gameObject.SetActive(false);
        Cross_Image.gameObject.SetActive(false);
        wrongImage.gameObject.SetActive(false);
        //change_no_Text.gameObject.SetActive(false);

        // ✅ Reset hint state so hint works fresh again
        EquationManager.Instance.ResetHintState();

        EquationManager.Instance.ResetAllHints();
    }



    private int CalculateBlank2(int blank1, int target, string op1)
    {
        switch (op1)
        {
            case "+": return target - blank1;
            case "-": return blank1 - target;
            case "*": return blank1 != 0 ? target / blank1 : 0;
            case "/": return target != 0 ? blank1 / target : 0;
            default: return target - blank1;
        }
    }

    public int GetBlank2HintValue()
    {
        return hintValueForBlank2;
    }

    public void OnTimeUp()
    {
        if (!gameObject.activeInHierarchy) return;

        cleartextfield();
        ResetBothBlanks();
    }



}