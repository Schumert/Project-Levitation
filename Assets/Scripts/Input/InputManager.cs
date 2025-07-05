using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;
    public static Vector2 MoveInput;
    public static bool WasJumpPressed;
    public static bool WasJumpReleased;
    public static bool IsSprintHeld;
    public static bool WasInteractPressed;
    public static bool WasSpawnActionPressed;
    public static bool WasQuickSpawnActionPressed;
    public static bool WasBoostBoxActionPressed;
    public static Vector2 BoxMoveInput;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction spawnAction;
    private InputAction boxMoveAction;
    private InputAction boostBoxAction;
    private InputAction quickSpawnAction;


    void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        moveAction = PlayerInput.actions.FindAction("Move");
        jumpAction = PlayerInput.actions.FindAction("Jump");
        sprintAction = PlayerInput.actions.FindAction("Sprint");
        interactAction = PlayerInput.actions.FindAction("Interaction");
        spawnAction = PlayerInput.actions.FindAction("SpawnBox");
        boxMoveAction = PlayerInput.actions.FindAction("MoveBox");
        boostBoxAction = PlayerInput.actions.FindAction("BoostBox");
        quickSpawnAction = PlayerInput.actions.FindAction("QuickSpawnBox");
    }

    void Update()
    {
        MoveInput = moveAction.ReadValue<Vector2>();
        BoxMoveInput = boxMoveAction.ReadValue<Vector2>();
        WasJumpPressed = jumpAction.WasPerformedThisFrame();
        WasJumpReleased = jumpAction.WasReleasedThisFrame();
        IsSprintHeld = sprintAction.IsPressed();
        WasInteractPressed = interactAction.WasPressedThisFrame();
        WasSpawnActionPressed = spawnAction.WasPressedThisFrame();
        WasBoostBoxActionPressed = boostBoxAction.WasPressedThisFrame();
        WasQuickSpawnActionPressed = quickSpawnAction.WasPressedThisFrame();
    }

    public static void DeactivatePlayerControls()
    {
        PlayerInput.currentActionMap.Disable();
    }

    public static void ActivatePlayerControls()
    {
        PlayerInput.currentActionMap.Enable();
    }
}
