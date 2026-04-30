using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour
{
    public enum Language { English, Arabic, Chinese }
    public Language language;

    void Awake()
    {
        // 🔥 Hook up onclick in code — never loses reference
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (LanguageSwitcher.Instance == null) return;

        switch (language)
        {
            case Language.English: LanguageSwitcher.Instance.SetEnglish(); break;
            case Language.Arabic: LanguageSwitcher.Instance.SetArabic(); break;
            case Language.Chinese: LanguageSwitcher.Instance.SetChinese(); break;
        }
    }
}