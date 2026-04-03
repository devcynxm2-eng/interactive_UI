using UnityEngine;
using UnityEngine.UI;

public class NumberButton : MonoBehaviour
{
    public int number;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
       
            GameManager.Instance?.SelectNumber(number);
    }
}