using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementManager : MonoBehaviour
{

    private IJumpResponse _jumpResponse;
    private IMovementResponse _movementResponse;





    private Rigidbody rb;
    [SerializeField] private bool isFalling;
    private bool isSprinting;
    private bool isOnElevator;


    public Vector2 moveValue { get; private set; }

    public bool LookingRight { get; private set; }

    private GameObject elevator;


    void Awake()
    {
        _jumpResponse = GetComponent<IJumpResponse>();
        if (_jumpResponse == null)
            Debug.LogError($"[{name}] IJumpResponse bulunamadı!");

        _movementResponse = GetComponent<IMovementResponse>();
        if (_movementResponse == null)
            Debug.LogError("IMovementResponse bulunamadı!");

        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {

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

        isFalling = !_jumpResponse.isGrounded && rb.linearVelocity.y < -0.1f && !isOnElevator;



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
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        _movementResponse.SetMoveInput(moveValue, isSprinting);

        AnimationManager.UpdateStates(Mathf.Abs(moveValue.x), _jumpResponse.isGrounded, isFalling, isSprinting);





    }

    void FixedUpdate()
    {
        _movementResponse.ApplyMovement();
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
            elevator = collision.gameObject;
            _movementResponse.SetPlatformState(true, elevator);
        }




    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ElevatorBox"))
        {
            _movementResponse.NotifyPlatformExit();
            elevator = null;
        }
    }








}
