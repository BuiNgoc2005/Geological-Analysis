using UnityEngine;
using System.Collections;

public class XRFMachine : MonoBehaviour
{
    [Header("Nắp máy")]
    public Transform lid;

    public float openAngle = -90f;
    public float openSpeed = 2f;

    [Header("Tube trong máy")]
    public GameObject tubeModel;

    public GameObject tubeFlourModel;

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

    // 1. Phải để TRUE để máy báo là ĐÃ CÓ TUBE
    private bool hasTube = true; 
    private bool isScanning = false;

    // 2. Phải CÓ DÒNG NÀY để khai báo cái Tube có sẵn là loại gì
    private ItemType currentTubeType = ItemType.XRFContainer; 

    void Start()
    {
        closedRot = lid.localRotation;
        // Giữ rotation gốc
        openedRot = closedRot * Quaternion.Euler(openAngle, 0, 0);
        // 3. XÓA MẤY DÒNG TUBE.SETACTIVE(FALSE) ĐI, THAY BẰNG DÒNG NÀY:
        UpdateTubeVisual(); 
        // Màn hình mặc định
        if (screenRenderer != null && scene1 != null)
        {
            screenRenderer.material.mainTexture = scene1;
        }
    }

    void Update()
    {
        Quaternion targetRot =
            isOpen ? openedRot : closedRot;

        lid.localRotation = Quaternion.Lerp(
            lid.localRotation,
            targetRot,
            Time.deltaTime * openSpeed
        );
    }

    // =====================================================
    // TƯƠNG TÁC CHÍNH
    // =====================================================
    public void Interact(PlayerInteract player)
    {
        // Đang scan thì khóa thao tác
        if (isScanning)
        {
            Debug.Log("Máy đang scan...");
            return;
        }

        // 1. ƯU TIÊN MỞ NẮP: Giống DiscMill, nếu nắp đang đóng thì bấm E luôn là mở nắp
        if (!isOpen)
        {
            ToggleLid();
            return;
        }
        // 2. TAY TRỐNG
        if (player.currentHandObject == null)
        {
            // Nếu trong máy có tube -> Lấy tube ra
            if (hasTube)
            {
                TakeTube(player);
            }
            else
            {
                ToggleLid();
            }
            return;
        }

        // 3. ĐANG CẦM ĐỒ
        ItemInfo handInfo = player.currentHandObject.GetComponent<ItemInfo>();

        if (handInfo == null)
            return;

        // Nếu cầm Tube hoặc TubeFlour
        if (handInfo.itemType == ItemType.XRFContainer ||
            handInfo.itemType == ItemType.TubeFlour)
        {
            InsertTube(player);
            return;
        }

        // Đồ khác thì không cho thao tác
        Debug.Log("Không thể bỏ vật này vào XRF");
    }

    // =====================================================
    // MỞ / ĐÓNG NẮP
    // =====================================================
    public void ToggleLid()
    {
        isOpen = !isOpen;

        // Khi đóng nắp và có TubeFlour -> scan
        if (!isOpen &&
            hasTube &&
            currentTubeType == ItemType.TubeFlour &&
            !isScanning)
        {
            StartCoroutine(StartScan());
        }
    }

    // =====================================================
    // BỎ TUBE VÀO
    // =====================================================
    public void InsertTube(PlayerInteract player)
    {
        if (!isOpen) return;
        if (hasTube) return;
        
        ItemInfo handInfo = player.currentHandObject.GetComponent<ItemInfo>();
        if (handInfo == null) return;

        // Lưu loại tube và cập nhật trạng thái
        currentTubeType = handInfo.itemType;
        hasTube = true;
        UpdateTubeVisual();
        player.ClearHand();
        Debug.Log("Đã đặt tube vào XRF!");

        // --- ĐOẠN THÊM MỚI ---
        // Nếu là lọ bột, tự động đóng nắp (hàm ToggleLid sẽ tự chạy StartScan)
        if (currentTubeType == ItemType.TubeFlour)
        {
            ToggleLid(); 
            Debug.Log("Phát hiện TubeFlour, tự động đóng nắp và quét...");
        }
        // ---------------------
    }
    // =====================================================
    // LẤY TUBE RA
    // =====================================================
    public void TakeTube(PlayerInteract player)
    {
        if (!isOpen)
        {
            Debug.Log("Phải mở nắp trước!");
            return;
        }
        if (!hasTube)
        {
            Debug.Log("Không có tube trong máy");
            return;
        }
        if (player.currentHandObject != null)
        {
            Debug.Log("Tay đang cầm đồ!");
            return;
        }
        // Trả đúng loại tube
        player.EquipHandItem(currentTubeType);
        hasTube = false;
        // Ẩn model
        UpdateTubeVisual();
        Debug.Log("Đã lấy tube khỏi máy!");
        // THÊM DÒNG NÀY: Dakan aapoap bandh karva mate
        ToggleLid(); 
    }
    // =====================================================
    // CẬP NHẬT HIỂN THỊ TUBE
    // =====================================================
    void UpdateTubeVisual()
    {
        // Ẩn hết trước
        if (tubeModel != null)
            tubeModel.SetActive(false);

        if (tubeFlourModel != null)
            tubeFlourModel.SetActive(false);

        // Không có tube
        if (!hasTube)
            return;
        // Hiện đúng loại
        if (currentTubeType == ItemType.XRFContainer)
        {
            if (tubeModel != null)
                tubeModel.SetActive(true);
        }
        else if (currentTubeType == ItemType.TubeFlour)
        {
            if (tubeFlourModel != null)
                tubeFlourModel.SetActive(true);
        }
    }

    // =====================================================
    // SCAN
    // =====================================================
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