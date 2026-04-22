using UnityEngine;

using TMPro;

public class restart_timer : MonoBehaviour

{

    [Header("Timer Settings")]

    public float totalTime = 30f;

    private float remainingTime;

    private bool timerRunning = false;

    [Header("References")]

    public GameObject gamePanel;

    public TextMeshProUGUI timerText;

    private bool wasPanelActive = false;

    private GameManager gameManager;

    public ProgressBar progressBar;

    void Start()

    {

        gameManager = GameManager.Instance;

        timerRunning = false;

        remainingTime = totalTime;

        UpdateTimerUI();

    }

    void Update()

    {



        bool isPanelActive = gamePanel.activeSelf;

        // Panel opened → resume

        if (isPanelActive && !wasPanelActive)

        {


            FullResetTimer();

            //timerRunning = true;

        }

        // Panel closed → pause

        else if (!isPanelActive && wasPanelActive)

        {

            timerRunning = false;

        }

        wasPanelActive = isPanelActive;

        if (timerRunning)

            UpdateTimer();

    }

    public void FullResetTimer()

    {

        remainingTime = totalTime;

        timerRunning = true;

        UpdateTimerUI();

        if (progressBar != null)

            progressBar.restartprogress();

    }

    // ================================

    // TIMER CORE

    // ================================

    void ResetAndStartTimer()

    {

        remainingTime = totalTime;

        timerRunning = true;

        UpdateTimerUI();

        if (progressBar != null)

            progressBar.restartprogress();

    }

    void UpdateTimer()

    {

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)

        {

            remainingTime = 0f;

            timerRunning = false;

            Debug.Log("Time Up Triggered");

            if (gameManager != null)

                gameManager.OnTimeUp();

        }

        UpdateTimerUI();

    }

    void UpdateTimerUI()

    {

        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(remainingTime);

        timerText.text = seconds + "s Remaining";

    }

    // ================================

    // EXTERNAL CONTROL (FROM GAME MANAGER)

    // ================================

    public void RestartTimerExternally()

    {

        ResetAndStartTimer();

    }

    public void PauseTimer()

    {

        timerRunning = false;

    }

    public void ResumeTimer()

    {

        timerRunning = true;

    }



    public void StopTimerAndProgress()
    {
        // Stop timer logic
        timerRunning = false;

        // Stop progress bar
        if (progressBar != null)
            progressBar.PauseProgress();
    }

    public void ResumeTimerAndProgress()
    {
        timerRunning = true;

        if (progressBar != null)
            progressBar.ResumeProgress();
    }
    public void ResetTimerAndProgress()
    {
        remainingTime = totalTime;
        timerRunning = false;

        UpdateTimerUI();

        if (progressBar != null)
            progressBar.restartprogress();
    }

}