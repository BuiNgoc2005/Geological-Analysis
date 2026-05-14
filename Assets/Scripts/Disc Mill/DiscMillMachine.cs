using UnityEngine;
using System.Collections;

public class DiscMillMachine : MonoBehaviour
{
    public enum MachineState
    {
        NoTray, Empty, Rock, Finished
    }

    [Header("Trạng thái máy")]
    // Khởi đầu máy đã có khay rỗng
    public MachineState currentState = MachineState.Empty;

    [Header("Animator (Đã tách 2 cái)")]
    public Animator lidAnimator;   // Kéo thả object NẮP vào đây
    public Animator leverAnimator; // Kéo thả object CẦN GẠT vào đây

    [Header("Visual")]
    public GameObject TrayEmpty;
    public GameObject TrayRock;
    public GameObject TrayFlour;

    [Header("Cài đặt")]
    public float autoCloseDistance = 4f;
    private bool isLidOpen = false;
    private bool isRunning = false;
    private Transform currentPlayerTransform;

    void Start()
    {
        UpdateVisuals();
    }

    void Update()
    {
        // Tự đóng nắp khi player đi xa (chỉ khi máy đang không nghiền)
        if (isLidOpen && currentPlayerTransform != null && !isRunning)
        {
            float distance = Vector3.Distance(transform.position, currentPlayerTransform.position);
            if (distance > autoCloseDistance)
                CloseLid();
        }
    }

    public void InteractWithMachine(PlayerInteract player)
    {
        currentPlayerTransform = player.transform;
        
        // Đang chạy -> Khoá tương tác
        if (isRunning)
        {
            Debug.Log("Máy đang hoạt động!");
            return;
        }

        // Ưu tiên: Nếu nắp đóng -> mở nắp
        if (!isLidOpen)
        {
            OpenLid();
            return;
        }

        // Nếu người chơi đang rảnh tay
        if (player.currentHandObject == null)
        {
            // Lấy khay rỗng
            if (currentState == MachineState.Empty)
            {
                player.EquipHandItem(ItemType.TrayDiscMill);
                currentState = MachineState.NoTray;
                UpdateVisuals();
                Debug.Log("Đã lấy khay rỗng.");
                return;
            }

            // Lấy khay bột ra
            if (currentState == MachineState.Finished)
            {
                // Gọi Coroutine để xử lý việc chờ đợi
                StartCoroutine(TakeFlourRoutine(player));
                return;
            }
        }

        // Nếu người chơi cầm đồ & máy đang không có khay
        if (currentState == MachineState.NoTray && player.currentHandObject != null)
        {
            ItemInfo info = player.currentHandObject.GetComponent<ItemInfo>();
            if (info == null) return;

            // Bỏ khay rỗng
            if (info.itemType == ItemType.TrayDiscMill)
            {
                currentState = MachineState.Empty;
                player.ClearHand();
                UpdateVisuals();
                Debug.Log("Đã đặt khay rỗng vào máy.");
                return;
            }

            // Bỏ khay đá -> Bắt đầu nghiền
            if (info.itemType == ItemType.TrayRockDiscMill)
            {
                currentState = MachineState.Rock;
                player.ClearHand();
                UpdateVisuals();
                StartGrinding();
                return;
            }
        }
    }

    // Tiến trình nghiền
    private void StartGrinding()
    {
        StartCoroutine(GrindingProcess());
    }
    
    private IEnumerator GrindingProcess()
    {
        isRunning = true;
        Debug.Log("Bắt đầu quá trình nghiền...");

        // GỌI LEVEL CHO CẦN GẠT: gạt cần xuống
        if (leverAnimator != null)
        {
            leverAnimator.SetTrigger("Level");
        }
        yield return new WaitForSeconds(1f);

        // GỌI ĐÓNG NẮP: Nắp đóng lại, không ảnh hưởng cần gạt
        if (lidAnimator != null)
        {
            lidAnimator.SetTrigger("CloseLid");
        }
        isLidOpen = false;
        yield return new WaitForSeconds(1f);

        Debug.Log("Đang nghiền trong 5s...");
        yield return new WaitForSeconds(5f);

        currentState = MachineState.Finished;
        UpdateVisuals();
        Debug.Log("Nghiền xong!");
        isRunning = false;
    }

    private IEnumerator TakeFlourRoutine(PlayerInteract player)
    {
        // Tạm thời khoá máy lại để người chơi không bấm E liên tục được
        isRunning = true; 

        // 1. GỌI UNLEVEL CHO CẦN GẠT: Bật cần gạt lên trước
        if (leverAnimator != null)
        {
            leverAnimator.SetTrigger("UnLevel"); 
        }

        // 2. ĐỢI 1 GIÂY NHƯ Ý BẠN
        yield return new WaitForSeconds(1f);

        // 3. Sau khi đợi xong mới giao khay bột cho người chơi
        player.EquipHandItem(ItemType.TrayFlourDiscMill);
        currentState = MachineState.NoTray;
        UpdateVisuals();
        
        Debug.Log("Đã đợi 1s, lấy khay bột và trả cần gạt lên.");
        
        // Mở khoá máy lại
        isRunning = false; 
    }

    private void OpenLid()
    {
        isLidOpen = true;
        if (lidAnimator != null)
            lidAnimator.SetTrigger("OpenLid");
        Debug.Log("Mở nắp máy.");
    }

    private void CloseLid()
    {
        isLidOpen = false;
        currentPlayerTransform = null;
        if (lidAnimator != null)
            lidAnimator.SetTrigger("CloseLid");
        Debug.Log("Đóng nắp máy.");
    }
    
    private void UpdateVisuals()
    {
        if (TrayEmpty != null) TrayEmpty.SetActive(currentState == MachineState.Empty);
        if (TrayRock != null) TrayRock.SetActive(currentState == MachineState.Rock);
        if (TrayFlour != null) TrayFlour.SetActive(currentState == MachineState.Finished);
    }
}