using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(MovementManager))]
public class DamageKnockback : MonoBehaviour
{
    [Header("Knockback Ayarları")]
    [Tooltip("Yatay başlangıç hızı.")]
    [SerializeField] float horizontalVelocity = 8f;
    [Tooltip("Dikey başlangıç hızı.")]
    [SerializeField] float verticalVelocity = 6f;
    [Tooltip("Yatay hızın saniyede ne kadar azalacağını kontrol eder.")]
    [SerializeField] float horizontalDecay = 3f;
    [Tooltip("Zemin layer’ları.")]
    [SerializeField] LayerMask groundMask;
    [Tooltip("Zeminle temas öncesi bekleme (sn).")]
    [SerializeField] float groundIgnoreTime = 0.1f;

    Rigidbody rb;
    MovementResponse movementResponse;
    MovementManager movementManager;
    bool isKnockbackActive;
    bool justStart;
    Vector3 kickVector;
    float knockbackStartTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movementResponse = GetComponent<MovementResponse>();
        movementManager = GetComponent<MovementManager>();

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;



    }

    void FixedUpdate()
    {
        if (justStart)
        {
            // Disable player movement scripts to avoid conflicts
            movementManager.enabled = false;

            // Apply impulse scaled by mass
            rb.AddForce(kickVector * rb.mass, ForceMode.Impulse);

            knockbackStartTime = Time.time;
            isKnockbackActive = true;
            justStart = false;
        }

        if (!isKnockbackActive) return;

        // Gradually decay horizontal velocity
        Vector3 v = rb.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, 0f, horizontalDecay * Time.fixedDeltaTime);
        rb.linearVelocity = v;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isKnockbackActive) return;
        if (Time.time - knockbackStartTime < groundIgnoreTime) return;

        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isKnockbackActive = false;
                EndKnockback();
                break;
            }
        }
    }

    /// <summary>
    /// Bir sonraki FixedUpdate'te knockback uygulanacak şekilde işaretler.
    /// </summary>
    public void TriggerKnockback(GameObject damageSource)
    {
        if (isKnockbackActive || justStart) return;

        float dir = Mathf.Sign(transform.position.x - damageSource.transform.position.x);
        kickVector = new Vector3(dir * horizontalVelocity, verticalVelocity, 0f);
        justStart = true;
    }

    void EndKnockback()
    {
        // Re-enable movement scripts
        movementManager.enabled = true;
    }
}
