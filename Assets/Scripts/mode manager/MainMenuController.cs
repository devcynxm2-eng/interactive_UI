using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;

    [Header("Menu Buttons")]
    public Button equationButton;
    public Button wordButton;

    [Header("Lock UI")]
    public GameObject wordLockIcon;
    public TMP_Text wordUnlockProgressText;

    void Start()
    {
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

    public void OnEquationButtonPressed()
    {
        GameSession.SetActiveGame(ActiveGame.Equation);
        SceneManager.LoadScene("GamePlayscene"); // your gameplay scene name
    }

    public void OnWordButtonPressed()
    {
        if (!GameSession.IsWordUnlocked()) return;
        GameSession.SetActiveGame(ActiveGame.Word);
        SceneManager.LoadScene("GamePlayscene");
    }
}