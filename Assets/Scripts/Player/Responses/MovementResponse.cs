
using UnityEngine;

public class MovementResponse : MonoBehaviour, IMovementResponse
{
    [Header("Hareket Ayarları")]
    private float moveSpeed = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    private Rigidbody rb;
    private IJumpResponse _jumpResponse;
    private Vector2 currentInput;
    private bool isSprinting;
    private bool isOnElevator;

    private GameObject elevator;
    private Vector3 boxVelocity;

    private Vector3 residualPlatformVelocity = Vector3.zero;
    [SerializeField, Tooltip("Platformdan ayrıldıktan sonra etki eden velocity ne hızla düşsün")] private float platformVelocityDecayRate = 3f; // platformdan ayrıldıktan sonra etki eden velocity ne hızla düşsün
    private bool applyResidualVelocity = false;



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _jumpResponse = GetComponent<IJumpResponse>();
    }

    public void SetMoveInput(Vector2 moveInput, bool isSprinting)
    {
        this.currentInput = moveInput;
        this.isSprinting = isSprinting;
    }

    public void ApplyMovement()
    {
        moveSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 velocity = rb.linearVelocity;






        if (elevator != null)
            boxVelocity = elevator.GetComponent<Rigidbody>().linearVelocity;

        if (_jumpResponse.isGrounded && isOnElevator && elevator != null)
        {

            // Platformdayken direkt velocity aktarımı
            velocity = boxVelocity + new Vector3(currentInput.x * moveSpeed, 0, 0);
            applyResidualVelocity = false;
        }
        else if (applyResidualVelocity)
        {
            velocity.x = 0;
            // Ters yönde input verildiyse daha hızlı azalsın
            bool isCounteringResidual =
                (residualPlatformVelocity.x > 0 && currentInput.x < 0) ||
                (residualPlatformVelocity.x < 0 && currentInput.x > 0);

            float decayRate = platformVelocityDecayRate;
            if (isCounteringResidual)
                decayRate *= 2f;

            residualPlatformVelocity = Vector3.MoveTowards(
                residualPlatformVelocity,
                Vector3.zero,
                decayRate * Time.fixedDeltaTime
            );

            velocity += new Vector3(residualPlatformVelocity.x, 0f, 0f);

            if (residualPlatformVelocity.magnitude <= 0.01f)
            {
                residualPlatformVelocity = Vector3.zero;
                applyResidualVelocity = false;
            }
        }
        else
        {
            velocity.x = currentInput.x * moveSpeed;
        }

        rb.linearVelocity = velocity;
    }



    public void SetPlatformState(bool isOnElevator, GameObject elevator)
    {
        this.isOnElevator = isOnElevator;
        this.elevator = elevator;
        applyResidualVelocity = false;
    }

    public void NotifyPlatformExit()
    {
        isOnElevator = false;
        elevator = null;
        residualPlatformVelocity = boxVelocity;
        applyResidualVelocity = true;
    }
}
