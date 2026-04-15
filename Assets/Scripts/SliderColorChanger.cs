using UnityEngine;
using UnityEngine.UI;

public class SliderColorChanger : MonoBehaviour
{
    public Slider slider;
    public Image icon;

    public Color normalColor = Color.white;
    public Color zeroColor = Color.gray;

    private bool initialized = false;

    void Start()
    {
        // ❌ IMPORTANT: prevent triggering OnValueChanged during setup
        slider.onValueChanged.RemoveListener(CheckValue);

        CheckValue(slider.value);

        slider.onValueChanged.AddListener(CheckValue);

        initialized = true;
    }

    void CheckValue(float value)
    {
        if (!initialized) return;

        if (icon == null) return;

        icon.color = (value <= 0) ? zeroColor : normalColor;
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(CheckValue);
    }
}