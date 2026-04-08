using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject TimeUpPanel;

    [Header("Panels")]
    public GameObject gamePanel;

    [Header("References")]
    public NumEq numEq;
    public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    public float totalTime = 30f;

    private float remainingTime;
    private bool timerRunning = false;

    // ── CHANGED: track previous panel state ──
    private bool wasPanelActive = false;
    public int eqdata;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        eqdata = EquationManager.currentEquationLength;
        TimeUpPanel.SetActive(false);
        timerRunning = false;
        UpdateTimerUI();
        numEq.ResetStatemgr();
    }

    void Update()
    {
        // ── CHANGED: automatically detect when panel becomes active ──
        bool isPanelActive = gamePanel.activeSelf;

        if (isPanelActive && !wasPanelActive)
        {
            // Panel just became active — start timer
            ResetAndStartTimer();
        }
        else if (!isPanelActive && wasPanelActive)
        {
            // Panel just became inactive — stop and reset timer
            timerRunning = false;
            remainingTime = totalTime;
            UpdateTimerUI();
        }

        wasPanelActive = isPanelActive;
        // ────────────────────────────────────────────────────────

        if (timerRunning)
            UpdateTimer();
    }

    public void OpenGamePanel()
    {
        gamePanel.SetActive(true);
    }

    public void CloseGamePanel()
    {
        gamePanel.SetActive(false);
    }

    void ResetAndStartTimer()
    {
        remainingTime = totalTime;
        timerRunning = true;
        UpdateTimerUI();
    }

    void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerRunning = false;
            Debug.Log("Show time up");
            TimeUpPanel.SetActive(true);
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(remainingTime);
        timerText.text = seconds + "s" + " Remaining";
    }

    public void RestartTimer()
    {
        ResetAndStartTimer();
        numEq.ResetStatemgr();
    }

    public void SelectNumber(int number)
    {
        if (EquationManager.currentEquationLength == 5)
        {
            numEq.SelectNumber(number);
        }
        else if (EquationManager.currentEquationLength == 7)
        {
            numEq.SelectNumberdouble(number);
        }

    }
}


//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using System.Collections;

//public class GameManager : MonoBehaviour
//{
//    // Singleton instance
//    public static GameManager Instance;

//    [Header("UI")]
//    public TextMeshProUGUI equationText;

//    public TextMeshProUGUI timerText; // <-- Timer UI

//    [Header("Equation Values")]
//    public int correctAnswer = 4;  // because 4 + 3 = 7
//    public int secondNumber = 3;
//    public int result = 7;

//    private int selectedNumber = -1;

//    [Header("UI Colors")]
//    public Image resultimage;
//    public Color CG = Color.green;
//    public Color CR = Color.red;

//    [Header("Timer Settings")]
//    public float totalTime = 30f; // 30 seconds countdown
//    private float remainingTime;
//    private bool timerRunning = false;

//    void Awake()
//    {
//        // Singleton logic
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void Start()
//    {


//        StartTimer();
//        selectedNumber = -1;
//        UpdateEquation();


//    }

//    void Update()
//    {
//        if (timerRunning)
//        {
//            UpdateTimer();
//        }
//    }

//    // ------------------- Timer Logic -------------------
//    void StartTimer()
//    {
//        remainingTime = totalTime;
//        timerRunning = true;
//        UpdateTimerUI();
//    }

//    void UpdateTimer()
//    {
//        remainingTime -= Time.deltaTime;

//        if (remainingTime <= 0f)
//        {
//            remainingTime = 0f;
//            timerRunning = false;
//            TimerFinished();
//        }

//        UpdateTimerUI();
//    }

//    void UpdateTimerUI()
//    {
//        if (timerText == null)
//        {
//            Debug.LogError("TimerText is NOT assigned!");
//            return;
//        }

//        int seconds = Mathf.CeilToInt(remainingTime);
//        timerText.text = seconds + "s remanining";
//    }

//    void TimerFinished()
//    {
//        Debug.Log("Time's up!");
//        if (resultimage != null) resultimage.color = CR;
//    }

//    // ------------------- Equation Logic -------------------
//    public void SelectNumber(int number)
//    {
//        Debug.Log("Number selected: " + number);
//        selectedNumber = number;
//        UpdateEquation();
//        CheckAnswer();
//    }

//    void UpdateEquation()
//    {
//        string firstPart = selectedNumber == -1 ? "" : selectedNumber.ToString();
//        equationText.text = firstPart;
//    }

//    void CheckAnswer()
//    {
//        if (selectedNumber + secondNumber == result)
//        {
//            if (resultimage != null) resultimage.color = CG;
//        }
//        else
//        {
//            if (resultimage != null) resultimage.color = CR;
//        }
//    }
//}