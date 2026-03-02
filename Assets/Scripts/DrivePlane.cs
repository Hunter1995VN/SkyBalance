using UnityEngine;
using UnityEngine.InputSystem;

public class DrivePlane : MonoBehaviour
{
    [Header("Cài đặt tốc độ")]
    [SerializeField, Range(0f, 20f)] private float speed = 10f;
    [SerializeField, Range(0f, 20f)] private float liftForce = 8f;

    [Header("Hệ thống Cảnh báo & Rơi")]
    [SerializeField, Range(0f, 5f)] private float warningDuration = 2.5f;
    [SerializeField, Range(0f, 10f)] private float shakeIntensity = 4f;

    private float warningTimer = 0f; // Bộ đếm thời gian thực tế
    private bool isWarning = false;
    private bool isDead = false;
    private bool isLanded = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    public void OnMove(InputValue value)
    {
        if (!isDead) moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (isLanded) { rb.linearVelocity = Vector2.zero; return; }
        if (isDead) return;

        // 1. DI CHUYỂN (Giữ nguyên)
        float moveX = Mathf.Max(0, moveInput.x * speed);
        float moveY = (Mathf.Abs(moveInput.y) > 0.1f) ? (moveInput.y * liftForce) : rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(moveX, moveY);

        // 2. LOGIC XOAY CẢI TIẾN
        // Tính toán góc mà người chơi MUỐN hướng tới dựa trên phím bấm
        float targetAngle = moveInput.y * 20f;

        if (isWarning)
        {
            warningTimer += Time.fixedDeltaTime;

            // 1. Tính góc mượt mà máy bay nên hướng tới (theo phím bấm)
            float smoothRotation = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * 5f);

            // 2. TẠO RUNG LẮC MẠNH: Cộng trực tiếp vào kết quả cuối cùng
            // Tăng tần số từ 25f lên 50f để rung gắt hơn
            float shake = Mathf.Sin(Time.time * 50f) * shakeIntensity;

            // Áp dụng góc mượt + độ rung ngẫu nhiên
            rb.MoveRotation(smoothRotation + shake);

            if (warningTimer >= warningDuration)
            {
                TriggerDeath();
            }
        }
        else
        {
            // Bay bình thường khi an toàn
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * 5f));
            warningTimer = 0f;
        }
    }

    // Dùng Stay để liên tục xác nhận máy bay đang ở trong vùng cấm
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Limit"))
        {
            isWarning = true;
        }
    }

    // Quan trọng: Khi bay ra khỏi vùng cấm thì tắt cảnh báo ngay
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Limit"))
        {
            isWarning = false;
            Debug.Log("Hú hồn! Đã quay lại vùng an toàn.");
        }
    }

    void TriggerDeath()
    {
        isDead = true;
        isWarning = false;

        rb.freezeRotation = false;
        rb.angularDrag = 1.5f;

        // Lao tới và chúi đầu
        rb.linearVelocity = new Vector2(speed * 0.4f, -5f);
        rb.AddTorque(-20f, ForceMode2D.Impulse);

        Debug.Log("Quá muộn rồi! Máy bay đã hỏng hoàn toàn.");
    }
}