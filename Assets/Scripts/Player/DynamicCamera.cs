using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public Transform target;
    public MovementManager controller;
    public float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    private float moveOffsetX = 10f; // Hareket ederken uzaklık
    private float idleOffsetX = 2f; // Durduğunda kullanılacak sabit x
    private Vector3 baseOffset = new Vector3(0f, 7.69f, -10.97f);

    private Quaternion rightRot = Quaternion.Euler(20f, -8f, 0f);
    private Quaternion leftRot = Quaternion.Euler(20f, 8f, 0f);

    void LateUpdate()
    {
        bool lookingRight = controller.LookingRight;
        bool isMoving = Mathf.Abs(controller.moveValue.x) > 0.01f;

       

        // Kamera offset X, duruma göre belirleniyor
        float offsetX;

        if (isMoving)
        {
            offsetX = lookingRight ? moveOffsetX : 5;
        }
        else
        {
            offsetX = lookingRight ? idleOffsetX : idleOffsetX;
        }

        Vector3 targetPos = target.position + new Vector3(offsetX, baseOffset.y, baseOffset.z);
        Quaternion targetRot = lookingRight ? rightRot : rightRot;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f);
    }
}
