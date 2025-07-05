using UnityEngine;

public class BreakableFloorTile : MonoBehaviour
{
    public Animator animator;
    public float destroyDelay = 0.5f;

    public void Break()
    {
        if (animator != null)
            animator.SetTrigger("Break");

        Destroy(gameObject, destroyDelay);
    }
}
