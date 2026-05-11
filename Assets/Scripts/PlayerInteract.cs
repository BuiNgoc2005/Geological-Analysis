using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Cài đặt tương tác")]
    public float interactDistance = 3f;
    public Camera playerCamera;

    [Header("Kho đồ (Holder)")]
    public Transform holder;

    // Đổi sang public để các Script khác (như JawCrusherMachine) có thể kiểm tra
    public GameObject currentHandObject = null;
    public GameObject currentWorldPrefab = null;

    void Start()
    {
        // Tắt tất cả đồ trong tay khi bắt đầu
        foreach (Transform child in holder) child.gameObject.SetActive(false);
        currentHandObject = null;
        currentWorldPrefab = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformInteract();
        }
    }

    void PerformInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // 1. Tương tác với Nút bấm (Start/Stop)
            if (hit.collider.CompareTag("MachineButton"))
            {
                JawCrusherMachine machine = hit.collider.GetComponentInParent<JawCrusherMachine>();
                if (machine != null) machine.ToggleMachine(hit.collider.gameObject.name);
                return; // Xử lý xong nút bấm thì dừng
            }

            // 2. Tương tác với Máy nghiền (Thân máy)
            if (hit.collider.CompareTag("JawCrusher"))
            {
                JawCrusherMachine machine = hit.collider.GetComponent<JawCrusherMachine>();
                if (machine != null)
                {
                    machine.InteractWithMachine(this);
                    return;
                }
            }

            if (hit.collider.CompareTag("DiscMill"))
            {
                DiscMillMachine discMachine = hit.collider.GetComponent<DiscMillMachine>();
                if (discMachine != null)
                {
                    discMachine.InteractWithMachine(this);
                    return;
                }
            }

            // 3. Tương tác với đồ vật để nhặt (PickupItem)
            if (hit.collider.CompareTag("PickupItem"))
            {
                ItemInfo itemInfo = hit.collider.GetComponent<ItemInfo>();
                if (itemInfo != null)
                {
                    if (currentHandObject == null) PickUpItem(hit.collider.gameObject, itemInfo);
                    else SwapItem(hit.collider.gameObject, itemInfo);
                }
                return;
            }

            // 4. Tương tác với bàn để đặt đồ
            if (hit.collider.CompareTag("Table") && currentHandObject != null)
            {
                PlaceItemOnTable(hit.point);
                return;
            }
        }

        // 5. Nếu bấm E vào không trung hoặc chỗ không có tag đặc biệt thì Vứt đồ
        if (currentHandObject != null)
        {
            DropItem();
        }
    }

    // --- CÁC HÀM HỖ TRỢ ---

    public void PickUpItem(GameObject worldObject, ItemInfo targetInfo)
    {
        Destroy(worldObject);
        EquipHandItem(targetInfo.itemType);
    }

    void SwapItem(GameObject worldObject, ItemInfo targetInfo)
    {
        Vector3 oldPos = worldObject.transform.position;
        Quaternion oldRot = worldObject.transform.rotation;
        
        // Vứt món đồ cũ ra
        Instantiate(currentWorldPrefab, oldPos, oldRot);
        
        // Hủy vật thể trên sàn và cầm món mới lên
        Destroy(worldObject);
        EquipHandItem(targetInfo.itemType);
    }

    public void DropItem()
    {
        if (currentHandObject == null) return;

        if (currentWorldPrefab == null) {
            Debug.LogWarning("Không thể vứt đồ: currentWorldPrefab là null!");
            ClearHand(); // Vẫn xóa đồ trên tay để tránh bị kẹt
            return;
        }

        Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
        Instantiate(currentWorldPrefab, dropPosition, playerCamera.transform.rotation);
        
        ClearHand();
    }

    public void ClearHand()
    {
        if (currentHandObject != null) currentHandObject.SetActive(false);
        currentHandObject = null;
        currentWorldPrefab = null;
    }

    void PlaceItemOnTable(Vector3 hitPoint)
    {
        if (currentWorldPrefab == null) {
            Debug.LogWarning("Không thể đặt đồ lên bàn: currentWorldPrefab là null!");
            return;
        }
        
        // Sửa 0.05f thành 0.01f hoặc thậm chí là 0 để sát mặt bàn nhất có thể
        Vector3 placePos = hitPoint + new Vector3(0, 0.01f, 0); 
        Instantiate(currentWorldPrefab, placePos, Quaternion.identity);
        ClearHand();
    }

    public void EquipHandItem(ItemType type)
    {
        // Tắt món đồ cũ (nếu có)
        if (currentHandObject != null) currentHandObject.SetActive(false);

        foreach (Transform handItem in holder)
        {
            ItemInfo handInfo = handItem.GetComponent<ItemInfo>();
            if (handInfo != null && handInfo.itemType == type)
            {
                handItem.gameObject.SetActive(true);
                currentHandObject = handItem.gameObject;
                currentWorldPrefab = handInfo.worldPrefab;
                break;
            }
        }
    }
}