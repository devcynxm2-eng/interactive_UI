using UnityEngine;
using UnityEngine.UI;

public class SliderColorChanger : MonoBehaviour
{
    public Slider slider;
    public Image icon;

    public Color normalColor = Color.white;
    public Color zeroColor = Color.gray;

    void Start()
    {
        slider.value = 1f;
        slider.onValueChanged.AddListener(CheckValue);
    }

    void CheckValue(float value)
    {
        if (value <= 0)
            icon.color = zeroColor;
        else
            icon.color = normalColor;
    }
}