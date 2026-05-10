using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Cài đặt Tương tác")]
    public Camera playerCamera; 
    public float interactDistance = 3f; // Khoảng cách tay với tới máy

    void Update()
    {
        // Khi nhấn phím E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // Bắn tia Raycast từ vị trí camera, hướng thẳng về phía trước
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Nếu tia đụng trúng vật thể trong cự ly cho phép
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Kiểm tra xem vật thể đó có dán Tag "Machine" không
            if (hit.collider.CompareTag("DiscMill"))
            {
                // Tìm component Animator trên cái máy đó (hoặc cha của nó)
                Animator machineAnim = hit.collider.GetComponentInParent<Animator>();

                if (machineAnim != null)
                {
                    // Lấy trạng thái hiện tại và đảo ngược nó (đang đóng -> mở, đang mở -> đóng)
                    bool isCurrentlyOpen = machineAnim.GetBool("isOpen");
                    machineAnim.SetBool("isOpen", !isCurrentlyOpen);
                }
                else
                {
                    Debug.LogWarning("Máy có Tag Machine nhưng không tìm thấy Animator!");
                }
            }
        }
    }
}