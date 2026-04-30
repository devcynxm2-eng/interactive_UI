

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Sych.ShareAssets.Runtime;

public class ScreenshotManager : MonoBehaviour
{
    [Header("UI")]
    public Image previewImage;
    public GameObject previewPanel;
    public GameObject uiPanel;

    [Header("References")]
    public Camera screenshotCamera;
    public GalleryManager galleryManager;

    [Header("Output")]
    public int outputHeight = 512;

    private Texture2D screenshotTexture;
    private string lastSavedPath;

    // ================= TAKE SCREENSHOT =================
    public void TakeScreenshot()
    {
        StartCoroutine(Capture());
    }

    IEnumerator Capture()
    {
        uiPanel.SetActive(false);
        yield return new WaitForEndOfFrame();

        Camera cam = screenshotCamera != null ? screenshotCamera : Camera.main;

        if (cam == null)
        {
            Debug.LogError("[Screenshot] No camera found!");
            uiPanel.SetActive(true);
            yield break;
        }

        // Save camera state
        RenderTexture prevTarget = cam.targetTexture;
        Rect prevRect = cam.rect;

        float aspect = (float)Screen.width / Screen.height;
        int width = Mathf.RoundToInt(outputHeight * aspect);
        int height = outputHeight;

        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

        cam.targetTexture = rt;
        cam.rect = new Rect(0, 0, 1, 1);
        cam.aspect = aspect;

        cam.Render();

        // Restore camera
        cam.targetTexture = prevTarget;
        cam.rect = prevRect;
        cam.ResetAspect();

        // Read texture
        RenderTexture.active = rt;

        screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshotTexture.Apply();

        RenderTexture.active = null;
        Destroy(rt);

        uiPanel.SetActive(true);

        // Preview
        Sprite sprite = Sprite.Create(
            screenshotTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f)
        );

        previewImage.sprite = sprite;
        previewImage.preserveAspect = true;
        previewPanel.SetActive(true);
    }

    // ================= SAVE SCREENSHOT =================
    public void SaveScreenshot()
    {
        if (screenshotTexture == null)
        {
            Debug.LogError("[Screenshot] No screenshot to save!");
            return;
        }

        string folder = Application.persistentDataPath + "/Screenshots/";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        lastSavedPath = folder + fileName;

        File.WriteAllBytes(lastSavedPath, screenshotTexture.EncodeToPNG());

        Debug.Log("[Screenshot] Saved: " + lastSavedPath);

        previewPanel.SetActive(false);

        if (galleryManager != null)
            galleryManager.RefreshGallery();

        Destroy(screenshotTexture);
        screenshotTexture = null;
    }

    // ================= SHARE SCREENSHOT =================
    public void ShareScreenshot()
    {
        if (screenshotTexture == null)
        {
            Debug.LogError("[Screenshot] No screenshot available!");
            return;
        }

        string folder = Application.persistentDataPath + "/ShareTemp/";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string tempPath = folder + "share_temp_" + System.DateTime.Now.Ticks + ".png";

        File.WriteAllBytes(tempPath, screenshotTexture.EncodeToPNG());

        Share.Items(new List<string> { tempPath }, success =>
        {
            Debug.Log(success ? "Shared successfully" : "Share failed");
        });
    }

    // ================= DISCARD =================
    public void DiscardScreenshot()
    {
        previewPanel.SetActive(false);

        if (screenshotTexture != null)
        {
            Destroy(screenshotTexture);
            screenshotTexture = null;
        }
    }
}








