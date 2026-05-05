using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UnlockManager : MonoBehaviour
{
    [Header("References")]
    public GameDatabase database;
    public GameObject notEnoughCoinsPanel;
    public TMP_Text notEnoughCoinsText;
    public TMP_Text coinDisplayText;

    public static event Action OnItemUnlocked;

    private const string CoinKey = "Playerscore";

    void Start()
    {
        LoadAllUnlockStates(); // ← Load saved unlock states on startup
        RefreshCoinDisplay();
        if (notEnoughCoinsPanel != null)
            notEnoughCoinsPanel.SetActive(false);
    }

    public int GetCoins() => PlayerPrefs.GetInt(CoinKey, 0);

    public void RefreshCoinDisplay()
    {
        if (coinDisplayText != null)
            coinDisplayText.text = LocalizedNumber.Format(GetCoins());
    }

    // Generates a unique key per item, e.g. "Unlocked_0_2"
    private string GetUnlockKey(int categoryIndex, int itemIndex)
        => $"Unlocked_{categoryIndex}_{itemIndex}";

    // Call once at startup to apply saved states to the ScriptableObject
    private void LoadAllUnlockStates()
    {
        for (int c = 0; c < database.categories.Count; c++)
        {
            var category = database.categories[c];
            for (int i = 0; i < category.items.Count; i++)
            {
                string key = GetUnlockKey(c, i);
                if (PlayerPrefs.GetInt(key, 0) == 1)
                {
                    category.items[i].isLocked = false; // Apply saved unlock
                }
            }
        }
    }

    public void TryUnlock(int categoryIndex, int itemIndex)
    {
        var item = database.categories[categoryIndex].items[itemIndex];

        if (!item.isLocked)
        {
            Debug.Log($"{item.itemName} is already unlocked.");
            return;
        }

        int currentCoins = GetCoins();

        if (currentCoins >= item.unlockCost)
        {
            // Deduct coins
            PlayerPrefs.SetInt(CoinKey, currentCoins - item.unlockCost);

            // Save unlock state permanently
            PlayerPrefs.SetInt(GetUnlockKey(categoryIndex, itemIndex), 1);
            PlayerPrefs.Save();

            // Update the ScriptableObject in memory
            item.isLocked = false;

            RefreshCoinDisplay();
            HideNotEnoughCoins();

            Debug.Log($"✅ Unlocked: {item.itemName}. Remaining coins: {GetCoins()}");

            OnItemUnlocked?.Invoke();
        }
        else
        {
            int missing = item.unlockCost - currentCoins;
            ShowNotEnoughCoins(missing, item.itemName);
        }
    }

    private void ShowNotEnoughCoins(int missing, string itemName)
    {
        if (notEnoughCoinsPanel == null) return;
        notEnoughCoinsPanel.SetActive(true);

        if (notEnoughCoinsText != null)
            notEnoughCoinsText.text =
                $"Not enough coins to unlock {itemName}!\n" +
                $"You need {missing} more coin(s).\n" +
                $"Play the game to earn more! 🎮";

        CancelInvoke(nameof(HideNotEnoughCoins));
        Invoke(nameof(HideNotEnoughCoins), 3f);
    }

    public void HideNotEnoughCoins()
    {
        if (notEnoughCoinsPanel != null)
            notEnoughCoinsPanel.SetActive(false);
    }
}