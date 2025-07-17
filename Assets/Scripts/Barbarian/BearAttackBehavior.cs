using System;
using System.Collections;
using UnityEngine;

public class BearAttackBehavior : IAttackBehavior
{
    private readonly BarbarianController owner;
    private readonly Transform player;
    private readonly Rigidbody rb;
    private readonly Vector3 launchDirection;
    private readonly float xForce;
    private readonly float yForce;
    private readonly float waitAfterLand;
    public int stunPoints { get; private set; }
    private bool canHop = true;
    private readonly float stunDuration;
    private readonly float attackDuration;
    public float damage { get; private set; }
    public LayerMask targetLayer { get; private set; }

    public BearAttackBehavior(
        BarbarianController owner,
        Rigidbody rb,
        Transform player,
        float xForce,
        float yForce,
        int stunPoints,
        float stunDuration,
        float waitAfterLand,
        float attackDuration,
        float damage,
        LayerMask targetLayer)
    {
        this.owner = owner;
        this.rb = rb;
        this.player = player;
        // direction towards player on XZ plane
        Vector3 flat = player.position - rb.position;
        flat.y = 0;
        launchDirection = flat.normalized;
        this.xForce = xForce;
        this.yForce = yForce;
        this.stunPoints = stunPoints;
        this.stunDuration = stunDuration;
        this.waitAfterLand = waitAfterLand;
        this.attackDuration = attackDuration;
        this.damage = damage;
        this.targetLayer = targetLayer;
    }

    public IEnumerator Execute()
    {
        float endTime = Time.time + attackDuration;

        // 1) İlk zıplama
        Jump();

        // 2) Süre dolana kadar her zıplama adımı:
        while (Time.time < endTime)
        {
            // a) Yere inene kadar bekle
            //yield return new WaitUntil(() => owner.isGrounded);

            // b) İniş sonrası bekleme
            yield return new WaitForSeconds(waitAfterLand);

            // c) Süre dolmadıysa tekrar zıpla
            if (Time.time < endTime)
                Jump();
        }

        // 3) Süre bittiğinde saldırıyı tamamla
        Debug.Log("Bear attack ended by time");
        owner.OnAttackComplete();

    }


    // Zıplama işini tek bir yerde toplayan yardımcı metot
    private void Jump()
    {
        float dir = Mathf.Sign(player.position.x - rb.position.x);
        SetFacing(dir);
        // Önce yere tema kontrolü
        if (!owner.isGrounded) return;
        rb.AddForce(new Vector3(dir * xForce, yForce, 0), ForceMode.Impulse);
    }

    private void SetFacing(float dirX)
    {
        float yAngle = dirX < 0f ? 270f : 90f;
        owner.transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
    }


    public void OnCollisionEnter(Collision collision)
    {
        int layer = collision.gameObject.layer;

        if (collision.collider.CompareTag("ElevatorBox"))
        {
            Debug.Log("Bear attack collided with ElevatorBox");
            foreach (Transform child in collision.collider.transform)
                if (child.CompareTag("Player")) { child.SetParent(null); break; }
            UnityEngine.Object.Destroy(collision.collider.gameObject);


            stunPoints--;
            if (stunPoints <= 0)
            {
                Debug.Log("Bear attack ended, applying stun");
                owner.Stun(stunDuration);

            }
        }
        else if (collision.collider.GetComponent<IDamageable>() is IDamageable dmgTarget)
        {
            dmgTarget.TakeDamage(damage);

            // Artık StartCoroutine burada yok, TriggerKnockback çağırıyoruz:
            if (collision.collider.TryGetComponent<DamageKnockback>(out var kn))
            {
                kn.TriggerKnockback(owner.gameObject);
            }

            owner.OnAttackComplete();
        }
        else if ((owner.getGroundMask().value & (1 << layer)) == 0)
        {
            Debug.Log("Bear attack hit obstacle, ending attack");
            owner.OnAttackComplete();
        }
    }
}