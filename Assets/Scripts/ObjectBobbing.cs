using UnityEngine;

public class WeaponBobbing : MonoBehaviour
{
    public float walkingBobbingSpeed = 14f;
    public float bobbingAmount = 0.05f;
    public CharacterController controller; // Kéo Character Controller của Player vào đây

    float defaultPosY = 0;
    float timer = 0;

    void Start()
    {
        // Lưu lại vị trí Y ban đầu của cục đá
        defaultPosY = transform.localPosition.y;
    }

    void Update()
{
    // Lấy tín hiệu đầu vào từ bàn phím (WASD hoặc phím mũi tên)
    float horizontal = Input.GetAxis("Horizontal");
    float vertical = Input.GetAxis("Vertical");

    // Nếu người chơi có bấm phím di chuyển
    if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
    {
        // Player đang đi - Cục đá nhấp nhô
        timer += Time.deltaTime * walkingBobbingSpeed;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            defaultPosY + Mathf.Sin(timer) * bobbingAmount,
            transform.localPosition.z
        );
    }
    else
    {
        // Player đứng yên - Đưa cục đá về vị trí cũ
        timer = 0;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed),
            transform.localPosition.z
        );
    }
}
}