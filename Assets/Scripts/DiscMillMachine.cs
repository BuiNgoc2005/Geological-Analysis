using UnityEngine;

public class DiscMillMachine : MonoBehaviour
{
    [Header("Cấu hình Animator & Visuals")]
    public Animator machineAnimator;
    public GameObject trayInsideMachine; // Object khay nằm bên trong máy

    [Header("Cài đặt tương tác")]
    public float autoCloseDistance = 4f; // Khoảng cách đi xa để máy tự đóng nắp

    // Các biến trạng thái
    private bool isLidOpen = false;
    private bool hasTrayInside = true;
    private Transform currentPlayerTransform = null; // Theo dõi vị trí player để tự đóng nắp

    void Start()
    {
        UpdateVisuals();
    }

    void Update()
    {
        // Nếu nắp đang mở và có theo dõi Player, kiểm tra khoảng cách
        if (isLidOpen && currentPlayerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, currentPlayerTransform.position);
            
            // Nếu player đi ra xa quá khoảng cách cho phép thì tự động đóng nắp
            if (distanceToPlayer > autoCloseDistance)
            {
                CloseLid();
            }
        }
    }

    public void InteractWithMachine(PlayerInteract player)
    {
        // Ghi nhớ Transform của player để tính khoảng cách đi xa
        currentPlayerTransform = player.transform;

        // 1. Nếu nắp đang đóng -> Bấm E để Mở nắp
        if (!isLidOpen)
        {
            OpenLid();
            return;
        }

        // 2. Nếu nắp ĐÃ MỞ, thực hiện logic lấy/đặt khay
        if (isLidOpen)
        {
            // Lấy khay ra (Máy có khay và Tay không cầm gì)
            if (hasTrayInside && player.currentHandObject == null)
            {
                // Giả định bạn có ItemType.TrayDiscMill trong enum ItemType
                player.EquipHandItem(ItemType.TrayDiscMill); 
                hasTrayInside = false;
                UpdateVisuals();
                Debug.Log("Đã lấy khay từ máy Disc Mill.");
                return;
            }

            // Đặt khay vào (Máy không có khay và Tay đang cầm đúng khay DiscMill)
            if (!hasTrayInside && player.currentHandObject != null)
            {
                ItemInfo info = player.currentHandObject.GetComponent<ItemInfo>();
                if (info != null && info.itemType == ItemType.TrayDiscMill) // Thay bằng ItemType khay tương ứng của bạn
                {
                    player.ClearHand();
                    hasTrayInside = true;
                    UpdateVisuals();
                    Debug.Log("Đã đặt khay vào máy Disc Mill.");
                    return;
                }
            }
        }
    }

    private void OpenLid()
    {
        isLidOpen = true;
        if (machineAnimator != null) machineAnimator.SetTrigger("OpenLid"); // Cần có trigger "OpenLid" trong Animator
        Debug.Log("Đang mở nắp máy Disc Mill.");
    }

    private void CloseLid()
    {
        isLidOpen = false;
        currentPlayerTransform = null; // Xóa theo dõi player vì nắp đã đóng
        if (machineAnimator != null) machineAnimator.SetTrigger("CloseLid"); // Cần có trigger "CloseLid" trong Animator
        Debug.Log("Player đi xa, tự động đóng nắp máy Disc Mill.");
    }

    private void UpdateVisuals()
    {
        // Hiện/ẩn model cái khay bên trong máy (Không dùng animation)
        if (trayInsideMachine != null)
        {
            trayInsideMachine.SetActive(hasTrayInside);
        }
    }
}