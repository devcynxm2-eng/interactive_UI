using UnityEngine;

public class SampleSceneUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject settingsPanel;

    void Start()
    {
        // Hide both first
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Show the correct one based on what button was clicked
        if (PausePanel.targetPanel == "Main_Menu")
        {
            menuPanel.SetActive(true);
        }
        else if (PausePanel.targetPanel == "settings")
        {
            settingsPanel.SetActive(true);
        }

        // Reset so it doesn't persist unexpectedly
        PausePanel.targetPanel = "";
    }
}