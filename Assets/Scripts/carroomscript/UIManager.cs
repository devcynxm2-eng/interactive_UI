using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameDatabase database;
    public Transform contentParent;
    public GameObject cardPrefab;
    public Sprite lockSprite;
    public Sprite unlockSprite;
    public UnlockManager unlockManager;
    public ScrollRect scrollRect;

    private int _currentCategoryIndex = 0;

    void Start()
    {
        ShowCategory(0); 
    }

    void OnEnable()
    {
        UnlockManager.OnItemUnlocked += RefreshCurrentCategory;

    }

    void OnDisable()
    {
        UnlockManager.OnItemUnlocked -= RefreshCurrentCategory;
    }

    public void ShowCategory(int index)
    {
        scrollRect.horizontalNormalizedPosition = 0f;
        _currentCategoryIndex = index;
        Debug.Log("Button clicked, index: " + index);

        if (database == null) { Debug.LogError("❌ Database is NULL"); return; }
        if (database.categories == null) { Debug.LogError("❌ Categories list is NULL"); return; }
        if (database.categories.Count <= index) { Debug.LogError("❌ Index OUT OF RANGE"); return; }

        RebuildCards(index);
    }

    private void RefreshCurrentCategory() => RebuildCards(_currentCategoryIndex);

    private void RebuildCards(int index)
    {
        // Safely destroy all existing children
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        if (database == null || database.categories == null) return;
        if (index >= database.categories.Count) return;

        var category = database.categories[index];
        if (category.items == null) return;

        for (int i = 0; i < category.items.Count; i++)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("❌ cardPrefab is NULL — assign it in Inspector!");
                return;
            }

            GameObject card = Instantiate(cardPrefab, contentParent);
            card.SetActive(true);
            CardUI ui = card.GetComponent<CardUI>();

            if (ui == null)
            {
                Debug.LogError("❌ CardUI component missing on prefab: " + cardPrefab.name);
                return;
            }

            ui.Setup(category.items[i], lockSprite, unlockSprite, index, i, unlockManager);
        }
    }
}