using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePlayPanel;



    public TMP_Text totalEquationText;
    public TMP_Text totalSolvedText;



    [Header("Game Containers (children of Main inside GamePlay Panel)")]
    public GameObject solveEquationPart;    // 'solve equation part' GameObject
    public GameObject solveWordPuzzlePart;  // 'solve word puzzle part' GameObject

    [Header("Manager GameObjects (standalone outside canvas)")]
    public GameObject equationManagerGO;   // 'Equation Manager' GameObject
    public GameObject wordManagerGO;       // 'Wordmanager' GameObject

    [Header("Menu Buttons")]
    public Button equationButton;
    public Button wordButton;

    [Header("Lock UI")]
    public GameObject wordLockIcon;
    public TMP_Text wordUnlockProgressText;

    [Header("Shared UI")]
    public ProgressBar progressBarController;
    public restart_timer timer;

    public EquationManager equationManager;
    void Awake()
    {
        
    }

    void Start()
    {// Start on main menu

        // Disable BEFORE any Start() runs
        solveWordPuzzlePart.SetActive(false);
        wordManagerGO.SetActive(false);
        solveEquationPart.SetActive(false);
        equationManagerGO.SetActive(true);


        gamePlayPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        RefreshUI();
    }

    void RefreshUI()
    {
        bool wordUnlocked = GameSession.IsWordUnlocked();
        wordButton.interactable = wordUnlocked;

        if (wordLockIcon != null)
            wordLockIcon.SetActive(!wordUnlocked);

        if (wordUnlockProgressText != null)
        {
            int solved = PlayerPrefs.GetInt("global_equations_solved", 0);
            wordUnlockProgressText.text = wordUnlocked
                ? "Unlocked!"
                : $"{solved}/{GameSession.WordUnlockThreshold} solved";
        }
    }

    // ── Assign to Equation button onClick ──
    public void OnEquationButtonPressed()
    {
        GameSession.SetActiveGame(ActiveGame.Equation);

        mainMenuPanel.SetActive(false);
        gamePlayPanel.SetActive(true);
        equationManager.wordleveltext();
        // Equation ON, Word OFF
        solveEquationPart.SetActive(true);
        equationManagerGO.SetActive(true);
        solveWordPuzzlePart.SetActive(false);
        wordManagerGO.SetActive(false);
        //EquationManager.Instance.Initialize();
        progressBarController.restartprogress();
        timer.RestartTimerExternally();
    }

    // ── Assign to Word button onClick ──
    public void OnWordButtonPressed()
    {
        if (!GameSession.IsWordUnlocked()) return;

        GameSession.SetActiveGame(ActiveGame.Word);

        mainMenuPanel.SetActive(false);
        gamePlayPanel.SetActive(true);
        equationManager.showleveltext();
        // Word ON, Equation OFF
        solveWordPuzzlePart.SetActive(true);
        wordManagerGO.SetActive(true);
        solveEquationPart.SetActive(false);
        equationManagerGO.SetActive(false);

        progressBarController.restartprogress();
        timer.RestartTimerExternally();
    }

    // ── Assign to Back button ──
    public void OnBackToMenuPressed()
    {
        GameSession.SetActiveGame(ActiveGame.None);

        gamePlayPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        timer.StopTimerAndProgress();
        RefreshUI();
    }
}