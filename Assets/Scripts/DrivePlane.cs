using UnityEngine;
using UnityEngine.InputSystem;

public class DrivePlane : MonoBehaviour
{
    [Header("Cài đặt tốc độ")]
    [SerializeField, Range(0f, 20f)]
    private float speed = 10f;       // Tốc độ tiến/lùi (Trục X)
    [SerializeField, Range(0f, 20f)]
    private float liftForce = 8f;    // Tốc độ lên/xuống (Trục Y)

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // KHAI BÁO BIẾN - Để hết lỗi đỏ CS0103 trong ảnh của bạn
    private bool isLanded = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Khóa xoay để máy bay không bị lật ngược khi va chạm
        rb.freezeRotation = true;
    }

    // Nhận tín hiệu từ phím WASD / Mũi tên
    public void OnMove(InputValue value)
    {
        if (!isDead) moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (isLanded || isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // KHÓA BAY LÙI: 
        // Chúng ta chỉ lấy giá trị di chuyển X từ 0 trở lên.
        // Nếu moveInput.x < 0 (nhấn phím A), nó sẽ bị ép về 0.
        float moveX = Mathf.Max(0, moveInput.x * speed);

        float moveY;
        if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            // Nếu đang bấm W/S, ưu tiên di chuyển theo phím bấm
            moveY = moveInput.y * liftForce;
        }
        else
        {
            // Nếu KHÔNG bấm, giữ nguyên vận tốc rơi hiện tại của Rigidbody (để trọng lực làm việc)
            moveY = rb.linearVelocity.y;
        }
        // Áp dụng vận tốc
        rb.linearVelocity = new Vector2(moveX, moveY);

        // Vẫn giữ nghiêng đầu khi bay lên/xuống để trông chuyên nghiệp
        float targetAngle = moveInput.y * 20f;
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * 5f));
    }
}