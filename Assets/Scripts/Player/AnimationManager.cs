using UnityEngine;

public class AnimationManager : MonoBehaviour
{

    public static AnimationManager instance;
    [SerializeField] private Animator animator;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public static void UpdateStates(float speed, bool isGrounded, bool isFalling, bool isSprinting)
    {
        if (instance.animator == null)
        {
            Debug.LogWarning("Animator bağlı değil!");
            return;
        }

        instance.animator.SetFloat("Speed", speed);
        instance.animator.SetBool("IsGrounded", isGrounded);
        instance.animator.SetBool("IsFalling", isFalling);
        instance.animator.SetBool("IsSprinting", isSprinting);
    }

    public static void PlayJumpTrigger()
    {
        if (instance.animator == null)
        {
            Debug.LogWarning("Animator bağlı değil!");
            return;
        }

        instance.animator.SetTrigger("JumpStart");
    }
}
