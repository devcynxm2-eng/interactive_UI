using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GalleryManager : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject imagePrefab;
    public Image fullPreviewImage;
    public GameObject fullPreviewPanel;

    private string folder;

    void Start()
    {
        folder = Application.persistentDataPath + "/Screenshots/";
        Debug.Log("[Gallery] Looking in: " + folder);
        LoadImages();
    }

    public void LoadImages()
    {
        if (!Directory.Exists(folder))
        {
            Debug.LogWarning("[Gallery] Folder does not exist yet: " + folder);
            return;
        }

        string[] files = Directory.GetFiles(folder, "*.png");
        Debug.Log("[Gallery] Found " + files.Length + " images in " + folder);

        if (files.Length == 0) return;

        System.Array.Reverse(files); // newest first

        foreach (string file in files)
        {
            Debug.Log("[Gallery] Loading: " + file);
            LoadImage(file);
        }
    }

    void LoadImage(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("[Gallery] File not found: " + path);
            return;
        }

        byte[] bytes = File.ReadAllBytes(path);
        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("[Gallery] Empty file: " + path);
            return;
        }

        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
        {
            Debug.LogError("[Gallery] Failed to load image: " + path);
            return;
        }

        Debug.Log("[Gallery] Loaded texture: " + tex.width + "x" + tex.height);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        GameObject imgObj = Instantiate(imagePrefab, contentPanel);

        // Force the prefab RectTransform to be a fixed size
        RectTransform rt = imgObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(200, 200);
        }

        Image img = imgObj.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("[Gallery] imagePrefab has no Image component!");
            return;
        }

        img.sprite = sprite;
        img.preserveAspect = true;

        // ── Root button → full preview ──
        Button[] allButtons = imgObj.GetComponentsInChildren<Button>(true);
        Button rootBtn = imgObj.GetComponent<Button>();

        if (rootBtn != null)
        {
            rootBtn.onClick.AddListener(() => ShowFull(sprite));
        }

        // ── Child delete button ──
        foreach (Button b in allButtons)
        {
            if (b != rootBtn)
            {
                string localPath = path;
                GameObject localObj = imgObj;
                b.onClick.AddListener(() => DeleteImage(localPath, localObj));
                break;
            }
        }
    }

    public void ShowFull(Sprite sprite)
    {
        if (fullPreviewImage == null || fullPreviewPanel == null)
        {
            Debug.LogError("[Gallery] fullPreviewImage or fullPreviewPanel not assigned!");
            return;
        }

        fullPreviewImage.sprite = sprite;
        fullPreviewImage.preserveAspect = true;
        fullPreviewPanel.SetActive(true);
    }

    public void DeleteImage(string path, GameObject obj)
    {
        Debug.Log("[Gallery] Deleting: " + path);
        if (File.Exists(path)) File.Delete(path);
        Destroy(obj);
    }

    public void RefreshGallery()
    {
        Debug.Log("[Gallery] Refreshing gallery...");

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        LoadImages();
    }
}