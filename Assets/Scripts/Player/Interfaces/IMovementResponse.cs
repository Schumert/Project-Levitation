using UnityEngine;

public interface IMovementResponse
{
    void SetMoveInput(Vector2 moveInput, bool isSprinting);
    void ApplyMovement(); // FixedUpdate'te çağrılır

    void SetPlatformState(bool isOnPlatform, GameObject elevator);
    void NotifyPlatformExit();
}
