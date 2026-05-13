using UnityEngine;
using System.Collections;

public class XRFMachine : MonoBehaviour
{
    [Header("Nắp máy")]
    public Transform lid;

    public float openAngle = -90f;
    public float openSpeed = 2f;

    [Header("Mẫu trong máy")]
    public GameObject sampleInMachine;

    [Header("Màn hình XRF")]
    public MeshRenderer screenRenderer;

    public Texture scene1;
    public Texture scene2;
    public Texture scene3;

    [Header("Thời gian scan")]
    public float scanTime = 5f;

    private bool isOpen = false;

    private Quaternion closedRot;
    private Quaternion openedRot;

    private bool hasSample = false;
    private bool isScanning = false;

    void Start()
    {
        closedRot = lid.localRotation;

        openedRot = Quaternion.Euler(openAngle, 0, 0);

        // Ẩn mẫu khi bắt đầu
        if (sampleInMachine != null)
        {
            sampleInMachine.SetActive(false);
        }

        // Màn hình mặc định
        if (screenRenderer != null && scene1 != null)
        {
            screenRenderer.material.mainTexture = scene1;
        }
    }

    void Update()
    {
        Quaternion targetRot = isOpen ? openedRot : closedRot;

        lid.localRotation = Quaternion.Lerp(
            lid.localRotation,
            targetRot,
            Time.deltaTime * openSpeed
        );
    }

    // Mở / đóng nắp
    public void ToggleLid()
    {
        isOpen = !isOpen;

        // Nếu vừa đóng nắp và có mẫu
        if (!isOpen && hasSample && !isScanning)
        {
            StartCoroutine(StartScan());
        }
    }

    // Đặt mẫu vào máy
    public void InsertSample(PlayerInteract player)
    {
        if (player.currentHandObject == null)
            return;

        ItemInfo handInfo =
            player.currentHandObject.GetComponent<ItemInfo>();

        if (handInfo != null &&
            handInfo.itemType == ItemType.TubeFlour)
        {
            // Hiện mẫu trong máy
            if (sampleInMachine != null)
            {
                sampleInMachine.SetActive(true);
            }

            hasSample = true;

            // Xóa đồ trên tay
            player.ClearHand();

            Debug.Log("Đã đặt mẫu vào máy XRF!");
        }
    }

    IEnumerator StartScan()
    {
        isScanning = true;

        Debug.Log("Bắt đầu scan XRF...");

        // Scene loading
        if (screenRenderer != null && scene2 != null)
        {
            screenRenderer.material.mainTexture = scene2;
        }

        yield return new WaitForSeconds(scanTime);

        // Scene kết quả
        if (screenRenderer != null && scene3 != null)
        {
            screenRenderer.material.mainTexture = scene3;
        }

        Debug.Log("Scan hoàn tất!");

        isScanning = false;
    }
}