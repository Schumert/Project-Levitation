using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DamageKnockback : MonoBehaviour
{
    [Header("Knockback Ayarları")]
    [Tooltip("Knockback süresi (sn).")]
    [SerializeField] float duration = 0.4f;
    [Tooltip("Yatay kuvvet çarpanı.")]
    [SerializeField] float horizontalForce = 5f;
    [Tooltip("Dikey kuvvet (yukarı doğru).")]
    [SerializeField] float verticalForce = 3f;

    Rigidbody rb;
    bool isKnockbackActive;
    float timer;
    Vector3 velocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isKnockbackActive) return;

        rb.linearVelocity = velocity;
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
            EndKnockback();
    }

    void EndKnockback()
    {
        isKnockbackActive = false;
        InputManager.ActivatePlayerControls();
        GetComponent<MovementController>().enabled = true;
    }

    /// <summary>
    /// Hasar kaynağı tarafından çağrıldığında tetiklenir.
    /// </summary>
    public void HandleKnockback(GameObject damageSource)
    {
        InputManager.DeactivatePlayerControls();
        GetComponent<MovementController>().enabled = false;

        // X ekseninde vurulduğumuz yönü hesapla
        float dir = Mathf.Sign(transform.position.x - damageSource.transform.position.x);


        velocity.x = dir * horizontalForce;
        velocity.y = verticalForce;
        timer = duration;
        isKnockbackActive = true;
    }
}
