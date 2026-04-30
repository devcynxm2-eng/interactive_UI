using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameResetManager : MonoBehaviour
{
    [Header("Reset button shown on AllComplete panel")]
    public Button resetButton;

    [Header("Confirmation dialog")]
    public GameObject confirmPanel;
    public Button confirmYesButton;
    public Button confirmNoButton;

    void Start()
    {
        confirmPanel.SetActive(false);

        resetButton.onClick.AddListener(OnResetClicked);
        confirmYesButton.onClick.AddListener(DoReset);
        confirmNoButton.onClick.AddListener(() => confirmPanel.SetActive(false));
    }

    void OnResetClicked()
    {
        confirmPanel.SetActive(true);
    }

    void DoReset()
    {
        // ── Equation progress ──
        PlayerPrefs.DeleteKey("progress");
        PlayerPrefs.DeleteKey("SolvedEquations");

        // ── Word puzzle progress ──
        PlayerPrefs.DeleteKey("word_progress");

        // ── Score ──
        PlayerPrefs.DeleteKey("Playerscore");

        // ── Word unlock progress ──
        PlayerPrefs.DeleteKey("global_equations_solved"); // ✅ was missing

        // ── Music and SFX settings: optional, uncomment to reset ──
        // PlayerPrefs.DeleteKey("music");
        // PlayerPrefs.DeleteKey("sfx");
        // PlayerPrefs.DeleteKey("musicVol");
        // PlayerPrefs.DeleteKey("sfxVol");

        // ── Language: keep it ──
        // PlayerPrefs.DeleteKey("selected_language");

        PlayerPrefs.Save();

        // ✅ Reset GameSession state
        GameSession.SetActiveGame(ActiveGame.None);

        // ✅ Kill all DOTween animations before loading
        DOTween.KillAll();

        // ✅ Reload from first scene
        SceneManager.LoadScene(0);
    }
}