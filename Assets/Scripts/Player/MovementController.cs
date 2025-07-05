using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{



    [Header("Hareket Ayarları")]
    private float moveSpeed = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    Vector3 boxVelocity;

    [Header("Zıplama Ayarları")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpBufferTime = 0.2f;  // zıplama tuşuna yere değmeden 200ms önce basılırsa zıplayabilir
    private float jumpBufferCounter = 0f;
    [SerializeField] private float coyoteTime = 0.2f;  // yere temas ettikten sonra bu kadar süre zıplayabilir
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.25f;
    [SerializeField] LayerMask groundMask;
    float coyoteTimer = 0f;



    private Rigidbody rb;
    [SerializeField] private bool isGrounded;
    private bool isFalling;
    private bool isSprinting;
    private bool isInElevator;


    public Vector2 moveValue { get; private set; }

    public bool LookingRight { get; private set; }


    private ElevatorBox elevator;
    private bool isTouchingWall = false;




    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Fizik ayarları
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
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

        isFalling = rb.linearVelocity.y < -0.1f;




        // Zıplama tuşuna basıldıysa buffer sayacı başlat
        if (InputManager.WasJumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }



        isGrounded = IsTouchingGround();

        //Yerden ayrıldıktan sonra sayacı başlat
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if ((isGrounded && jumpBufferCounter > 0) || (coyoteTimer > 0 && InputManager.WasJumpPressed))
        {
            boxVelocity = ReturnBoxVelocity();
            AddForce(jumpForce, Vector3.up);

            // Sıfırlamalar
            jumpBufferCounter = 0;
            coyoteTimer = 0;
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

        AnimationManager.UpdateStates(Mathf.Abs(moveValue.x), isGrounded, isFalling, isSprinting);



        //Asansör dışında bir yere indiğimizde kutunun velocitysi kullanmamak için sıfırlıyoruz.
        if (isGrounded && !isInElevator)
        {
            boxVelocity = Vector3.zero;
        }


    }

    void FixedUpdate()
    {


        Vector3 velocity = rb.linearVelocity;
        velocity.x = boxVelocity.x + (moveValue.x * moveSpeed);

        if (isTouchingWall && !isGrounded)
        {

            velocity.x = 0;
        }


        rb.linearVelocity = velocity;






    }

    Vector3 ReturnBoxVelocity()
    {
        if (isInElevator)
        {
            if (elevator.isBoosted) return elevator.Velocity;
        }

        return Vector3.zero;
    }



    public void AddForce(float jumpForce, Vector3 direction)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f); // yukarı hız sıfırlama
        rb.AddForce(direction * jumpForce, ForceMode.Impulse);
        AnimationManager.PlayJumpTrigger();
    }


    void OnCollisionStay(Collision collision)
    {


        ContactPoint[] contacts = new ContactPoint[collision.contactCount];
        collision.GetContacts(contacts);

        foreach (ContactPoint contact in contacts)
        {
            Vector3 normal = contact.normal;

            // Eğer temas yönü yatay (sağ/sol)
            if (Mathf.Abs(normal.x) > 0.5f)
            {
                isTouchingWall = true;
                return; // biri bile duvarsa yeter
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        boxVelocity = Vector3.zero;

        if (collision.gameObject.CompareTag("ElevatorBox"))
        {
            isInElevator = true;
            elevator = collision.gameObject.GetComponent<ElevatorBox>();
        }




    }
    void OnCollisionExit(Collision collision)
    {
        //isGrounded = false;

        if (collision.gameObject.CompareTag("ElevatorBox"))
        {
            isInElevator = false;
            elevator = null;
        }


        isTouchingWall = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }

    bool IsTouchingGround()
    {
        return Physics.CheckSphere(
        groundCheck.position,
        checkRadius,
        groundMask);
    }



}
