using UnityEngine;

public enum ActiveGame { None, Equation, Word }

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    public static ActiveGame Current { get; private set; } = ActiveGame.None;
    public const int WordUnlockThreshold = 2;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static bool IsWordUnlocked()
    {
        return PlayerPrefs.GetInt("global_equations_solved", 0) >= WordUnlockThreshold;
    }

    public static void RegisterEquationSolved()
    {
        int current = PlayerPrefs.GetInt("global_equations_solved", 0);
        current++;
        PlayerPrefs.SetInt("global_equations_solved", current);
        PlayerPrefs.Save();
        Debug.Log($"[GameSession] Equations solved: {current}/{WordUnlockThreshold}");
    }

    public static void SetActiveGame(ActiveGame game)
    {
        Current = game;
        Debug.Log($"[GameSession] Active game: {game}");
    }

    [ContextMenu("Unlock Word Now")]
    void DEBUG_UnlockWord()
    {
        PlayerPrefs.SetInt("global_equations_solved", WordUnlockThreshold);
        PlayerPrefs.Save();
        Debug.Log("[GameSession] DEBUG: Word unlocked!");
    }

    [ContextMenu("Reset All Progress")]
    void DEBUG_ResetProgress()
    {
        PlayerPrefs.DeleteKey("global_equations_solved");
        PlayerPrefs.DeleteKey("progress");
        PlayerPrefs.DeleteKey("word_progress");
        PlayerPrefs.Save();
        Debug.Log("[GameSession] DEBUG: All progress reset!");
    }

    [ContextMenu("Print Current Progress")]
    void DEBUG_PrintProgress()
    {
        int solved = PlayerPrefs.GetInt("global_equations_solved", 0);
        Debug.Log($"[GameSession] Equations solved = {solved}/{WordUnlockThreshold}, Word unlocked = {IsWordUnlocked()}");
    }
}