using UnityEngine;
using UnityEngine.UI;

public class NumberButton : MonoBehaviour
{
    public int number;
    public NumEq numEq;

    private Button button;
    public int eqdata;

    // using other script

    public SoundManager soundManager;

    void Start()
    {
        eqdata = EquationManager.currentEquationLength;
    }
    void Awake()
    {
        
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        soundManager.PlayButtonClick();
        GameManager.Instance?.SelectNumber(number);
        

    }
}