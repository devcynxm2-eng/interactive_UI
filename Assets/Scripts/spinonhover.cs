using UnityEngine;
using System.Collections;

public class SpinImage : MonoBehaviour
{
    public float duration = 1.0f; 

    private bool isSpinning = false;

   
    public void StartSpin()
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinCoroutine());
        }
    }

    IEnumerator SpinCoroutine()
    {
        isSpinning = true;
        Quaternion startRotation = transform.rotation;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Rotates on Z-axis (2D)
            transform.Rotate(0, 0, 360 * Time.deltaTime / duration);
            yield return null;
        }

        // Ensure final rotation is exact
        transform.rotation = startRotation;
        isSpinning = false;
    }
}
