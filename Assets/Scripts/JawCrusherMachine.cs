using UnityEngine;
using System.Collections;

public class JawCrusherMachine : MonoBehaviour
{
    public enum MachineState { NoTray, Empty, HasRockInFeeder, Finished }
    
    [Header("Trạng thái")]
    public MachineState currentState = MachineState.Empty;
    private bool isRunning = false; // Máy đang chạy hay không

    [Header("Các vật thể hiển thị")]
    public GameObject TrayEmpty;       // Khay rỗng dưới gầm
    public GameObject TrayRock;        // Khay đá vụn dưới gầm
    public GameObject rockInFeeder;    // Viên đá to ở phễu trên

    [Header("Cấu hình khác")]
    public Animator machineAnimator;

    // Biến tạm để lưu thông tin cho Animation Event
    private PlayerInteract tempPlayer;
    private ItemType tempItemType;

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
            tempPlayer = player; // Lưu tạm player
            tempItemType = ItemType.TrayRockJawCrusher; // Lưu tạm loại item
            machineAnimator.SetTrigger("TakeTrayRock"); // Animation sẽ gọi OnAnimationTakeTray()
            currentState = MachineState.NoTray; // Chuyển sang trạng thái không có khay
            //UpdateVisuals();
            Debug.Log("Đang lấy khay đá vụn...");
            return;
        }

        // 2. Logic Lấy Khay TRỐNG từ máy (Khi máy có khay trống và tay không cầm gì)
        if (currentState == MachineState.Empty && player.currentHandObject == null) {
            tempPlayer = player;
            tempItemType = ItemType.TrayJawCrusher;
            machineAnimator.SetTrigger("TakeTray"); // Animation sẽ gọi OnAnimationTakeTray()
            currentState = MachineState.NoTray; // Chuyển sang trạng thái không có khay
            //UpdateVisuals();
            Debug.Log("Đang lấy khay trống từ máy...");
            return;
        }

        // 3. Logic Đặt Khay trống vào máy (Khi không có khay và tay cầm khay trống)
        if (currentState == MachineState.NoTray && player.currentHandObject != null) {
            ItemInfo info = player.currentHandObject.GetComponent<ItemInfo>();
            if (info != null && info.itemType == ItemType.TrayJawCrusher) {
                player.ClearHand(); // Xóa khay trên tay
                machineAnimator.SetTrigger("PutTray"); // Animation đặt khay trống
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

    // --- HÀM CALLBACK CHO ANIMATION EVENT ---
    // Thêm Animation Event vào khung hình mà tay chạm vào khay trong Animation Clip
    public void OnAnimationTakeTray() {
    if (tempPlayer != null) {

        tempPlayer.EquipHandItem(tempItemType);

        // Chỉ ẩn khay sau khi animation đã chạy tới frame event
        currentState = MachineState.NoTray;
        UpdateVisuals();

        Debug.Log($"Item {tempItemType} đã được trang bị qua Animation Event");

        tempPlayer = null;
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
                currentState = MachineState.Finished;
                UpdateVisuals();
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
        if (TrayEmpty != null)
            TrayEmpty.SetActive(currentState == MachineState.Empty || currentState == MachineState.HasRockInFeeder);
        
        // Khay đá vụn hiện khi trạng thái là Finished
        if (TrayRock != null)
            TrayRock.SetActive(currentState == MachineState.Finished);

        // Viên đá to ở phễu hiện khi vừa bỏ vào
        if (rockInFeeder != null)
            rockInFeeder.SetActive(currentState == MachineState.HasRockInFeeder);
    }
}