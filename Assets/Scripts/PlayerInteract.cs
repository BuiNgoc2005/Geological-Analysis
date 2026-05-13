using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Cài đặt tương tác")]
    public float interactDistance = 3f;
    public Camera playerCamera;

    [Header("Kho đồ (Holder)")]
    public Transform holder;

    [Header("Trạng thái hiện tại")]
    public GameObject currentHandObject = null;
    public GameObject currentWorldPrefab = null;

    void Start()
    {
        foreach (Transform child in holder) child.gameObject.SetActive(false);
        currentHandObject = null;
        currentWorldPrefab = null;
    }

    void Update()
    {
        // E để tương tác vật lý
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformInteract();
        }

        // Click trái để mở UI XRF
        if (Input.GetMouseButtonDown(0))
        {
            TryOpenXRFUI();
        }
    }


    void TryOpenXRFUI()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("XRFMachine"))
            {
                XRFUI ui =
                    hit.collider.GetComponentInParent<XRFUI>();

                if (ui != null)
                {
                    ui.OpenUI();

                    Debug.Log("OPEN XRF UI");
                }
            }
        }
    }

    void PerformInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // XRF MACHINE
            // XRF MACHINE
            if (hit.collider.CompareTag("XRFMachine"))
            {
                XRFMachine xrf = hit.collider.GetComponent<XRFMachine>();

                if (xrf != null)
                {
                    // Nếu đang cầm đồ
                    if (currentHandObject != null)
                    {
                        ItemInfo handInfo =
                            currentHandObject.GetComponent<ItemInfo>();

                        // Nếu đang cầm TubeFlour
                        if (handInfo != null &&
                            handInfo.itemType == ItemType.TubeFlour)
                        {
                            xrf.InsertSample(this);
                        }
                        else
                        {
                            // Không phải TubeFlour thì mở nắp
                            xrf.ToggleLid();
                        }
                    }
                    else
                    {
                        // Tay trống thì mở nắp
                        xrf.ToggleLid();
                    }

                    return;
                }
            }

            if (hit.collider.CompareTag("MachineButton"))
            {
                JawCrusherMachine machine = hit.collider.GetComponentInParent<JawCrusherMachine>();
                if (machine != null) machine.ToggleMachine(hit.collider.gameObject.name);
                return;
            }

            if (hit.collider.CompareTag("JawCrusher"))
            {
                JawCrusherMachine machine = hit.collider.GetComponent<JawCrusherMachine>();
                if (machine != null) { machine.InteractWithMachine(this); return; }
            }

            if (hit.collider.CompareTag("DiscMill"))
            {
                DiscMillMachine discMachine = hit.collider.GetComponent<DiscMillMachine>();
                if (discMachine != null) { discMachine.InteractWithMachine(this); return; }
            }

            if (hit.collider.CompareTag("PickupItem"))
            {
                ItemInfo targetInfo = hit.collider.GetComponent<ItemInfo>();
                if (targetInfo != null)
                {
                    // GỌI LOGIC CHUYỂN ĐỔI ĐÁ
                    if (HandleTrayTransfer(targetInfo, hit.collider.gameObject)) return;

                    if (currentHandObject == null) PickUpItem(hit.collider.gameObject, targetInfo);
                    else SwapItem(hit.collider.gameObject, targetInfo);
                }
                return;
            }

            if (hit.collider.CompareTag("Table") && currentHandObject != null)
            {
                PlaceItemOnTable(hit.point);
                return;
            }
        }

        if (currentHandObject != null) DropItem();
    }

    // --- LOGIC ĐỔ ĐÁ (ĐÃ SỬA ĐỂ GỌI REPLACEWORLDITEM) ---
    bool HandleTrayTransfer(ItemInfo targetInfo, GameObject targetWorldObject)
    {
        if (currentHandObject == null) return false;
        ItemInfo handInfo = currentHandObject.GetComponent<ItemInfo>();

        // Trường hợp: Tay cầm khay Jaw có đá + Nhìn vào khay Disc rỗng trên bàn
        if (handInfo.itemType == ItemType.TrayRockJawCrusher && targetInfo.itemType == ItemType.TrayDiscMill)
        {
            // 1. Đổi món đồ trên tay thành Khay Jaw Rỗng
            EquipHandItem(ItemType.TrayJawCrusher);

            ReplaceWorldItem(targetWorldObject, ItemType.TrayRockDiscMill);

            Debug.Log("Đã đổ đá vào khay Disc Mill thành công!");
            return true;
        }

        if (handInfo.itemType == ItemType.TrayDiscMill && targetInfo.itemType == ItemType.TrayRockJawCrusher)
        {
            // 1. Đổi món đồ trên tay thành Khay Jaw Rỗng
            EquipHandItem(ItemType.TrayRockDiscMill);

            ReplaceWorldItem(targetWorldObject, ItemType.TrayJawCrusher);

            Debug.Log("Đã lấy đá vào khay Disc Mill thành công!");
            return true;
        }

        // Đổ bột từ khay Disc Mill vào lọ XRF
        if (handInfo.itemType == ItemType.TrayFlourDiscMill &&
            targetInfo.itemType == ItemType.XRFContainer)
        {
            // KHÔNG đổi khay trên tay
            // vì khay vẫn còn bột

            // Đổi lọ rỗng thành lọ có bột
            ReplaceWorldItem(targetWorldObject, ItemType.TubeFlour);

            Debug.Log("Đã đổ bột vào lọ XRF!");

            return true;
        }

        // Đổ bột từ khay Disc Mill vào lọ XRF
        if (handInfo.itemType == ItemType.TrayFlourDiscMill &&
            targetInfo.itemType == ItemType.XRFContainer)
        {
            Debug.Log("MATCH XRF");

            ReplaceWorldItem(targetWorldObject, ItemType.TubeFlour);

            return true;
        }


        return false;
    }

    // --- HÀM THAY THẾ VẬT THỂ TRÊN THẾ GIỚI ---
    void ReplaceWorldItem(GameObject oldObject, ItemType newType)
    {
        Vector3 pos = oldObject.transform.position;
        Quaternion rot = oldObject.transform.rotation;
        
        // Hủy vật thể cũ (cái khay rỗng)
        Destroy(oldObject);
        
        // Tìm prefab tương ứng trong holder để tạo vật thể mới
        foreach (Transform t in holder)
        {
            ItemInfo info = t.GetComponent<ItemInfo>();
            if (info != null && info.itemType == newType)
            {
                if (info.worldPrefab != null)
                {
                    // TẠO RA VẬT THỂ MỚI
                    GameObject newObj = Instantiate(info.worldPrefab, pos, rot);
                    
                    // QUAN TRỌNG: Đảm bảo nó được hiện lên (vì prefab lấy từ holder có thể đang bị ẩn)
                    newObj.SetActive(true); 
                    
                    // Xóa chữ (Clone) nếu muốn gọn hierarchy
                    newObj.name = info.worldPrefab.name;
                }
                break;
            }
        }
    }

    // --- CÁC HÀM CƠ BẢN KHÁC ---
    public void PickUpItem(GameObject worldObject, ItemInfo targetInfo)
    {
        Destroy(worldObject);
        EquipHandItem(targetInfo.itemType);
    }

    void SwapItem(GameObject worldObject, ItemInfo targetInfo)
    {
        Vector3 oldPos = worldObject.transform.position;
        Quaternion oldRot = worldObject.transform.rotation;
        Instantiate(currentWorldPrefab, oldPos, oldRot);
        Destroy(worldObject);
        EquipHandItem(targetInfo.itemType);
    }

    public void DropItem()
    {
        if (currentHandObject == null || currentWorldPrefab == null) return;
        Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
        GameObject dropped = Instantiate(currentWorldPrefab, dropPosition, playerCamera.transform.rotation);
        dropped.SetActive(true);
        ClearHand();
    }

    public void PlaceItemOnTable(Vector3 hitPoint)
    {
        if (currentWorldPrefab == null) return;
        Vector3 placePos = hitPoint + new Vector3(0, 0.01f, 0); 
        GameObject placed = Instantiate(currentWorldPrefab, placePos, Quaternion.identity);
        placed.SetActive(true);
        ClearHand();
    }

    public void EquipHandItem(ItemType type)
    {
        if (currentHandObject != null) currentHandObject.SetActive(false);

        foreach (Transform handItem in holder)
        {
            ItemInfo handInfo = handItem.GetComponent<ItemInfo>();
            if (handInfo != null && handInfo.itemType == type)
            {
                handItem.gameObject.SetActive(true);
                currentHandObject = handItem.gameObject;
                currentWorldPrefab = handInfo.worldPrefab;
                return;
            }
        }
    }

    public void ClearHand()
    {
        if (currentHandObject != null) currentHandObject.SetActive(false);
        currentHandObject = null;
        currentWorldPrefab = null;
    }
}