using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Panels")]
    public GameObject gamePanel;
    public GameObject TimeUpPanel;

    [Header("References")]
    public NumEq numEq;
    public ProgressBar ProgressBarController;
    public Complete_Panel Complete_Panel;
    public restart_timer timer; // 👈 reference to timer script

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        TimeUpPanel.SetActive(false);
        numEq.ResetStatemgr();
    }

    // ================================
    // PANEL CONTROL
    // ================================
    public void OpenGamePanel()
    {
        gamePanel.SetActive(true);
    }

    public void CloseGamePanel()
    {
        gamePanel.SetActive(false);
    }

    // ================================
    // NUMBER INPUT
    // ================================
    public void SelectNumber(int number)
    {
        if (EquationManager.currentEquationLength == 5)
            numEq.SelectNumber(number);
        else if (EquationManager.currentEquationLength == 7)
            numEq.SelectNumberdouble(number);
    }

    // ================================
    // NEXT BUTTON
    // ================================
    public void OnNextButtonPressed()
    {
        TimeUpPanel.SetActive(false);
        timer.RestartTimerExternally(); // 👈 restart timer
        Complete_Panel.ResetCompleteUI();
        ProgressBarController.restartprogress();

        numEq.cleartextfield();
        numEq.ResetBothBlanks();



        EquationManager.Instance.OnCorrectAnswer();





    }


    public void OnNextWordButtonPressed()
    {
        TimeUpPanel.SetActive(false);

        timer.RestartTimerExternally();

        Complete_Panel.ResetCompleteUI();
        ProgressBarController.restartprogress();

        // reset word state FIRST
        WordPuzzleManager.Instance.ClearTiles();
        WordPuzzleManager.Instance.ResetWordState();
        WordPuzzleManager.Instance.SaveProgress();
        // THEN load next word
        WordPuzzleManager.Instance.LoadNextWord();
    }


    // ================================
    // CALLED BY TIMER
    // ================================
    public void OnTimeUp()
    {
        TimeUpPanel.SetActive(true);

        numEq.cleartextfield();
        numEq.ResetBothBlanks();
        // Word reset - same pattern
        WordPuzzleManager.Instance.ClearTiles();
        WordPuzzleManager.Instance.ResetWordState();
    }
}