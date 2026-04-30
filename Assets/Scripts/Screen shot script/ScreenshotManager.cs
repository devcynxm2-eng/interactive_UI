

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









//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using System.IO;

//public class ScreenshotManager : MonoBehaviour
//{
//    [Header("UI")]
//    public Image previewImage;
//    public GameObject previewPanel;
//    public GameObject uiPanel;

//    [Header("References")]
//    public GalleryManager galleryManager;
//    public Camera screenshotCamera;

//    [Header("Output")]
//    public int outputHeight = 512;

//    private Texture2D _screenshot;

//    public void TakeScreenshot()
//    {
//        StartCoroutine(Capture());
//    }

//    IEnumerator Capture()
//    {
//        uiPanel.SetActive(false);
//        yield return new WaitForEndOfFrame();

//        Camera cam = screenshotCamera != null ? screenshotCamera : Camera.main;

//        if (cam == null)
//        {
//            Debug.LogError("[Screenshot] No camera found!");
//            uiPanel.SetActive(true);
//            yield break;
//        }

//        Debug.Log($"[Screenshot] Camera: {cam.name} " +
//                  $"| Position: {cam.transform.position} " +
//                  $"| Rotation: {cam.transform.eulerAngles} " +
//                  $"| OrthoSize: {cam.orthographicSize} " +
//                  $"| FOV: {cam.fieldOfView}");

//        // ── Save camera state ─────────────────────────────────────────────────
//        RenderTexture prevTarget = cam.targetTexture;
//        Rect prevRect = cam.rect;

//        // ── RT sized to screen aspect — no distortion ─────────────────────────
//        float screenAspect = (float)Screen.width / (float)Screen.height;
//        int rtHeight = outputHeight;
//        int rtWidth = Mathf.RoundToInt(outputHeight * screenAspect);

//        RenderTexture rt = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.ARGB32);
//        rt.antiAliasing = 1;
//        rt.Create();

//        // ── Render exactly what camera sees right now ─────────────────────────
//        // No rotation change — captures live view as-is
//        cam.targetTexture = rt;
//        cam.rect = new Rect(0, 0, 1, 1);
//        cam.aspect = screenAspect;
//        cam.Render();

//        // ── Restore camera ────────────────────────────────────────────────────
//        cam.targetTexture = prevTarget;
//        cam.rect = prevRect;
//        cam.ResetAspect();

//        // ── Read pixels ───────────────────────────────────────────────────────
//        RenderTexture prevActive = RenderTexture.active;
//        RenderTexture.active = rt;

//        _screenshot = new Texture2D(rtWidth, rtHeight, TextureFormat.RGB24, false);
//        _screenshot.ReadPixels(new Rect(0, 0, rtWidth, rtHeight), 0, 0);
//        _screenshot.Apply();

//        RenderTexture.active = prevActive;
//        rt.Release();
//        Destroy(rt);

//        uiPanel.SetActive(true);

//        // ── Preview ───────────────────────────────────────────────────────────
//        Sprite sprite = Sprite.Create(
//            _screenshot,
//            new Rect(0, 0, rtWidth, rtHeight),
//            new Vector2(0.5f, 0.5f),
//            100f
//        );

//        previewImage.sprite = sprite;
//        previewImage.preserveAspect = true;
//        previewPanel.SetActive(true);
//    }

//    //public void SaveScreenshot()
//    //{
//    //    if (_screenshot == null)
//    //    {
//    //        Debug.LogError("[Screenshot] Nothing to save!");
//    //        return;
//    //    }

//    //    string folder = Application.persistentDataPath + "/Screenshots/";
//    //    string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
//    //    string fullPath = folder + fileName;

//    //    if (!Directory.Exists(folder))
//    //        Directory.CreateDirectory(folder);

//    //    byte[] png = _screenshot.EncodeToPNG();
//    //    File.WriteAllBytes(fullPath, png);

//    //    if (File.Exists(fullPath))
//    //        Debug.Log("[Screenshot] Saved: " + fullPath + " (" + png.Length + " bytes)");
//    //    else
//    //        Debug.LogError("[Screenshot] Failed to save: " + fullPath);

//    //    previewPanel.SetActive(false);
//    //    Destroy(_screenshot);
//    //    _screenshot = null;

//    //    if (galleryManager != null)
//    //        galleryManager.RefreshGallery();
//    //    else
//    //        Debug.LogError("[Screenshot] galleryManager not assigned!");
//    //}


//    public void SaveScreenshot()
//    {
//        if (_screenshot == null)
//        {
//            Debug.LogError("[Screenshot] Nothing to save!");
//            return;
//        }

//        // Example crop (center square)
//        int size = Mathf.Min(_screenshot.width, _screenshot.height);
//        Rect cropRect = new Rect(
//            (_screenshot.width - size) / 2,
//            (_screenshot.height - size) / 2,
//            size,
//            size
//        );

//        Texture2D cropped = CropTexture(_screenshot, cropRect);

//        string folder = Application.persistentDataPath + "/Screenshots/";
//        string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
//        string fullPath = folder + fileName;

//        if (!Directory.Exists(folder))
//            Directory.CreateDirectory(folder);

//        byte[] png = cropped.EncodeToPNG();
//        File.WriteAllBytes(fullPath, png);

//        Debug.Log("[Screenshot] Saved cropped: " + fullPath);

//        Destroy(cropped);
//        Destroy(_screenshot);
//        _screenshot = null;

//        previewPanel.SetActive(false);

//        if (galleryManager != null)
//            galleryManager.RefreshGallery();
//    }


//    public void DiscardScreenshot()
//    {
//        previewPanel.SetActive(false);
//        if (_screenshot != null)
//        {
//            Destroy(_screenshot);
//            _screenshot = null;
//        }
//    }


//    Texture2D CropTexture(Texture2D source, Rect cropRect)
//    {
//        int x = Mathf.FloorToInt(cropRect.x);
//        int y = Mathf.FloorToInt(cropRect.y);
//        int w = Mathf.FloorToInt(cropRect.width);
//        int h = Mathf.FloorToInt(cropRect.height);

//        Color[] pixels = source.GetPixels(x, y, w, h);

//        Texture2D result = new Texture2D(w, h, source.format, false);
//        result.SetPixels(pixels);
//        result.Apply();

//        return result;
//    }

//}