using UnityEngine;
using UnityEngine.UI;

public class XRFUI : MonoBehaviour
{
    public GameObject uiPanel;

    public RawImage resultImage;

    public XRFMachine xrfMachine;

    private bool isOpen = false;

    void Update()
    {
        // Sync texture realtime
        if (uiPanel.activeSelf)
        {
            resultImage.texture =
                xrfMachine.screenRenderer.material.mainTexture;
        }

        // ESC để thoát UI
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
        }
    }

    public void OpenUI()
    {
        uiPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isOpen = true;

        // Khóa camera player
        Time.timeScale = 0f;
    }

    public void CloseUI()
    {
        uiPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isOpen = false;

        // Mở lại game
        Time.timeScale = 1f;
    }
}