using UnityEngine;
using UnityEngine.UI;

public class NumberButton : MonoBehaviour
{
    public int number;
    public NumEq numEq;

    private Button button;
    public int eqdata;


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

        GameManager.Instance?.SelectNumber(number);
        //if (eqdata == 5)
        //{
        //    Debug.Log("number clickked ============> " + number);
        //    GameManager.Instance?.SelectNumber(number);
        //}
        //else if(eqdata == 7)
        //{
        //    Debug.Log("number clickked ============> " + number);
        //    numEq?.SelectNumberdouble(number);
        //}
    }
}