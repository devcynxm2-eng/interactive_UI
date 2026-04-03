using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class ProgressBarController : MonoBehaviour
{
  
    public Image progressBarFillImage;
    private float timer = 0.0f;
    public float duration = 10.0f;

    

    void Update()
    {

        if(timer < duration)
        {
            timer += Time.deltaTime;
            //clamp so it never goes above 1
            float progress = Mathf.Clamp01(1f-(timer/duration));

            progressBarFillImage.fillAmount = progress;


        }


    }

    public void restartprogress()
    {
        timer = 0f;

        if (progressBarFillImage != null)
            progressBarFillImage.fillAmount = 1f;
    }



}
