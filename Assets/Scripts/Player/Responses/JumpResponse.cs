using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class JumpResponse : MonoBehaviour, IJumpResponse
{
    [Header("Zıplama Ayarları")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.2f;

    private Rigidbody rb;
    private float jumpBufferCounter;
    private float coyoteTimer;

    public bool isGrounded =>
        Physics.CheckSphere(groundCheck.position, checkRadius, groundMask);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public bool TryJump(bool jumpPressed)
    {
        // 1) Jump buffer
        if (jumpPressed) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // 2) Coyote time
        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        // 3) Zıplama şartı
        if ((isGrounded && jumpBufferCounter > 0f) ||
            (coyoteTimer > 0f && jumpPressed))
        {
            // Yukarı doğru hızı sıfırla
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Sayaçları sıfırla
            jumpBufferCounter = 0f;
            coyoteTimer = 0f;
            return true;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}