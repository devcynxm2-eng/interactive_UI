using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Room_Pick_Manager : MonoBehaviour
{
    public RectTransform[] images;
    private Vector3 normalScale = Vector3.one;
    private Vector3 zoomedScale = new Vector3(1.05f, 1.05f, 1.05f);
    private int currentZoomedIndex = -1;
    public float tweenDuration = 0.3f;
    public Ease easeType = Ease.OutBack;

    public void ZoomImage(int index)
    {
        if (index < 0 || index >= images.Length) return;

        if (currentZoomedIndex == index)
        {
            ResetAllImages();
            currentZoomedIndex = -1;
            return;
        }

        ResetAllImages();

        if (images[index] == null) return; // ✅ safety check

        images[index].DOKill();
        images[index].localScale = normalScale;
        images[index]
            .DOScale(zoomedScale, tweenDuration)
            .SetEase(easeType)
            .SetLink(images[index].gameObject); // ✅ auto-kill when destroyed

        currentZoomedIndex = index;
    }

    void ResetAllImages()
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null) continue;
            images[i].DOKill();
            images[i].localScale = normalScale;
        }
    }

    // ✅ Kill all tweens when this object is destroyed
    void OnDestroy()
    {
        ResetAllImages();
    }
}