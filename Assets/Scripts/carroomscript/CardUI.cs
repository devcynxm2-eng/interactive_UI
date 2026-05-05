using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image mainImage;
    public Image icon;
    public Image lockIcon;
    public TextMeshProUGUI label;
    public TextMeshProUGUI costLabel;   // Shows coin cost e.g. "🪙 50"
    public Button unlockButton;         // The unlock button on the card

    public Sprite lockSprite;
    public Sprite unlockSprite;

    private int _categoryIndex;
    private int _itemIndex;
    private UnlockManager _unlockManager;

    public void Setup(GameDatabase.Item item, Sprite lockSpr, Sprite unlockSpr,
                  int categoryIndex, int itemIndex, UnlockManager unlockManager)
    {
        mainImage.sprite = item.image;
        icon.sprite = item.icon;
        label.text = item.itemName;

        if (lockIcon != null && lockSpr != null && unlockSpr != null)
            lockIcon.sprite = item.isLocked ? lockSpr : unlockSpr;

        _categoryIndex = categoryIndex;
        _itemIndex = itemIndex;
        _unlockManager = unlockManager;

        if (costLabel != null)
            costLabel.text = item.isLocked ? $"🪙 {item.unlockCost}" : "✅ Unlocked";

        if (unlockButton != null)
        {
            // ✅ NEVER SetActive here — just control interactability
            unlockButton.interactable = item.isLocked;
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }

        gameObject.SetActive(true); // ✅ always keep card visible
    }
    private void OnUnlockClicked()
    {
        _unlockManager?.TryUnlock(_categoryIndex, _itemIndex);
    }
}