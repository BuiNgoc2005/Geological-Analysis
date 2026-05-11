using UnityEngine;

// Khai báo sẵn các loại đồ vật có trong game của bạn
public enum ItemType
{
    Granite,
    TrayJawCrusher,
    TrayDiscMill,
    TrayRockJawCrusher,
    TrayRockDiscMill
    // Bạn có thêm đồ gì mới thì cứ phẩy rồi ghi thêm vào đây
}

public class ItemInfo : MonoBehaviour
{
    [Header("Loại đồ vật (Chọn từ danh sách)")]
    public ItemType itemType; 

    [Header("Bản gốc (Prefab) để vứt ra (Chỉ cần điền cho đồ trên bàn)")]
    public GameObject worldPrefab;
}