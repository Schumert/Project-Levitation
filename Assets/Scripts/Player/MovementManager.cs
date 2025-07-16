using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementManager : MonoBehaviour
{

    private IJumpResponse _jumpResponse;

    [Header("Hareket Ayarları")]
    private float moveSpeed = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;





    private Rigidbody rb;
    [SerializeField] private bool isFalling;
    private bool isSprinting;
    private bool isInElevator;


    public Vector2 moveValue { get; private set; }

    public bool LookingRight { get; private set; }

    private GameObject elevator;
    private Vector3 boxVelocity;

    private Vector3 residualPlatformVelocity = Vector3.zero;
    [SerializeField, Tooltip("Platformdan ayrıldıktan sonra etki eden velocity ne hızla düşsün")] private float platformVelocityDecayRate = 3f; // platformdan ayrıldıktan sonra etki eden velocity ne hızla düşsün
    private bool applyResidualVelocity = false;

    void Awake()
    {
        _jumpResponse = GetComponent<IJumpResponse>();
        if (_jumpResponse == null)
            Debug.LogError($"[{name}] IJumpResponse bulunamadı!");

        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {

        moveSpeed = walkSpeed;

        rb.useGravity = false;
    }

    void Update()
    {
        moveValue = InputManager.MoveInput;

        // Yön ve döndürme
        if (moveValue.x < 0)
        {
            transform.Find("Mage").rotation = Quaternion.Euler(0f, 270f, 0f);
            LookingRight = false;
        }
        else if (moveValue.x > 0)
        {
            transform.Find("Mage").rotation = Quaternion.Euler(0f, 90f, 0f);
            LookingRight = true;
        }

        isFalling = !_jumpResponse.isGrounded && rb.linearVelocity.y < -0.1f && !isInElevator;



        //Zıplama
        bool jumpPressed = InputManager.WasJumpPressed;
        bool didJump = _jumpResponse.TryJump(jumpPressed);
        if (didJump)
        {
            AnimationManager.PlayJumpTrigger();

        }





        // Sprint kontrolü
        if (InputManager.IsSprintHeld && Math.Abs(moveValue.x) > 0)
        {
            moveSpeed = sprintSpeed;
            isSprinting = true;
        }
        else
        {
            moveSpeed = walkSpeed;
            isSprinting = false;
        }

        AnimationManager.UpdateStates(Mathf.Abs(moveValue.x), _jumpResponse.isGrounded, isFalling, isSprinting);





    }

    void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveValue.x * moveSpeed;

        if (elevator != null)
            boxVelocity = elevator.GetComponent<Rigidbody>().linearVelocity;

        if (_jumpResponse.isGrounded && isInElevator && elevator != null)
        {
            // Platformdayken direkt velocity aktarımı
            velocity = boxVelocity + new Vector3(moveValue.x * moveSpeed, 0, 0);
            applyResidualVelocity = false;
        }
        else if (applyResidualVelocity)
        {
            // Ters yönde input verildiyse daha hızlı azalsın
            bool isCounteringResidual =
                (residualPlatformVelocity.x > 0 && moveValue.x < 0) ||
                (residualPlatformVelocity.x < 0 && moveValue.x > 0);

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
            velocity.x = moveValue.x * moveSpeed;
        }




        rb.linearVelocity = velocity;
    }




    public void AddForce(float jumpForce, Vector3 direction)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f); // yukarı hız sıfırlama
        rb.AddForce(direction * jumpForce, ForceMode.Impulse);

    }


    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("ElevatorBox"))
        {
            isInElevator = true;
            elevator = collision.gameObject;
        }




    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ElevatorBox"))
        {
            isInElevator = false;
            elevator = null;

            // Çıkarken platform hızını kaydet
            residualPlatformVelocity = boxVelocity;
            applyResidualVelocity = true;
        }
    }








}
