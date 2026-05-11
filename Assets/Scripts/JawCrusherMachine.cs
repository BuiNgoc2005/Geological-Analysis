using UnityEngine;

public class JawCrusherMachine : MonoBehaviour
{
    public enum MachineState { NoTray, Empty, HasRockInFeeder, Finished }
    
    [Header("Trạng thái")]
    public MachineState currentState = MachineState.Empty;
    public bool isRunning = false; // Máy đang chạy hay không

    [Header("Các vật thể hiển thị")]
    public GameObject trayEmpty;       // Khay rỗng dưới gầm
    public GameObject trayCrushed;     // Khay đá vụn dưới gầm
    public GameObject rockInFeeder;    // Viên đá to ở phễu trên

    [Header("Cấu hình khác")]
    public Animator machineAnimator;

    void Start() {
        UpdateVisuals();
    }

    // --- TƯƠNG TÁC VỚI THÂN MÁY (Bỏ đá / Lấy khay) ---
    public void InteractWithMachine(PlayerInteract player) {
        // Nếu máy đang CHẠY thì KHÔNG cho lấy khay hay bỏ đá
        if (isRunning) {
            Debug.Log("Máy đang hoạt động! Phải bấm STOP trước.");
            return;
        }

        // 1. Logic Lấy Khay ĐÁ VỤN (Khi máy đã dừng và đã nghiền xong)
        if (currentState == MachineState.Finished && player.currentHandObject == null) {
            player.EquipHandItem(ItemType.TrayRockJawCrusher); // Cầm khay đá vụn lên tay
            machineAnimator.SetTrigger("TakeTrayRock"); // Animation lấy khay đá vụn
            currentState = MachineState.NoTray; // Chuyển sang trạng thái không có khay
            UpdateVisuals();
            Debug.Log("Đã lấy khay đá vụn.");
            return;
        }

        // 2. Logic Lấy Khay TRỐNG từ máy (Khi máy có khay trống và tay không cầm gì)
        if (currentState == MachineState.Empty && player.currentHandObject == null) {
            player.EquipHandItem(ItemType.TrayJawCrusher); // Cầm khay trống lên tay
            machineAnimator.SetTrigger("TakeTrayEmpty"); // Animation lấy khay trống (cần tạo trigger này trong Animator)
            currentState = MachineState.NoTray; // Chuyển sang trạng thái không có khay
            UpdateVisuals();
            Debug.Log("Đã lấy khay trống từ máy.");
            return;
        }

        // 3. Logic Đặt Khay trống vào máy (Khi không có khay và tay cầm khay trống)
        if (currentState == MachineState.NoTray && player.currentHandObject != null) {
            ItemInfo info = player.currentHandObject.GetComponent<ItemInfo>();
            if (info != null && info.itemType == ItemType.TrayJawCrusher) {
                player.ClearHand(); // Xóa khay trên tay
                machineAnimator.SetTrigger("PutTrayEmpty"); // Animation đặt khay trống
                currentState = MachineState.Empty; // Chuyển về trạng thái có khay trống
                UpdateVisuals();
                Debug.Log("Đã đặt khay trống vào máy.");
                return;
            }
        }

        // 4. Logic Bỏ Đá (Khi máy có khay trống và tay cầm Granite)
        if (currentState == MachineState.Empty && player.currentHandObject != null) {
            ItemInfo info = player.currentHandObject.GetComponent<ItemInfo>();
            if (info != null && info.itemType == ItemType.Granite) {
                player.ClearHand(); // Xóa đá trên tay
                machineAnimator.SetTrigger("PutRock"); // Anim đá rơi vào phễu
                currentState = MachineState.HasRockInFeeder;
                UpdateVisuals();
                Debug.Log("Đã bỏ đá vào phễu. Chờ bấm Start.");
            }
        }
    }

    // --- TƯƠNG TÁC VỚI NÚT BẤM (START/STOP) ---
    public void ToggleMachine(string buttonName) {
        if (buttonName == "Start") {
            if (currentState == MachineState.HasRockInFeeder && !isRunning) {
                isRunning = true;
                rockInFeeder.SetActive(false); 
                machineAnimator.SetTrigger("StartCrush");
                Invoke("ProcessCrushing", 3f); 
                Debug.Log("Máy bắt đầu nghiền...");
            }
        }
        else if (buttonName == "Stop") {
            if (isRunning) {
                isRunning = false; // Tắt máy
                machineAnimator.SetTrigger("StopCrush"); // Chạy Anim dừng máy
                Debug.Log("Máy đã dừng. Giờ có thể lấy khay.");
            }
        }
    }

    // Hàm này giả lập việc đá đã rơi xuống khay sau khi nghiền
    void ProcessCrushing() {
        if (isRunning) {
            currentState = MachineState.Finished; // Chuyển trạng thái sang "Đã có đá vụn dưới khay"
            UpdateVisuals(); 
            Debug.Log("Nghiền xong! Đá đã xuống khay. Hãy bấm Stop để lấy.");
        }
    }

    void UpdateVisuals() {
        // Khay rỗng chỉ hiện khi máy có khay trống hoặc có đá trong phễu
        if (trayEmpty != null)
            trayEmpty.SetActive(currentState == MachineState.Empty || currentState == MachineState.HasRockInFeeder);
        
        // Khay đá vụn hiện khi trạng thái là Finished
        if (trayCrushed != null)
            trayCrushed.SetActive(currentState == MachineState.Finished);

        // Viên đá to ở phễu hiện khi vừa bỏ vào
        if (rockInFeeder != null)
            rockInFeeder.SetActive(currentState == MachineState.HasRockInFeeder);
    }
}