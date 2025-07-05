using System;
using System.Collections;
using UnityEngine;

public class BoarAttackBehavior : IAttackBehavior
{
    private readonly BarbarianController owner;
    private readonly Transform player;
    private readonly Rigidbody rb;
    private readonly float speed;
    public int stunPoints { get; private set; }
    public float damage { get; private set; }
    public LayerMask targetLayer { get; private set; }



    public BoarAttackBehavior(BarbarianController owner, Transform player, Rigidbody rb, float speed, int stunPoints, float damage, LayerMask targetLayer)
    {
        this.owner = owner;
        this.player = player;
        this.rb = rb;
        this.speed = speed;
        this.stunPoints = stunPoints;
        this.damage = damage;
        this.targetLayer = targetLayer;
    }

    public IEnumerator Execute()
    {

        float initialDir = Mathf.Sign(player.position.x - rb.position.x);
        SetFacing(initialDir);

        while (true)
        {

            float currentDir = Mathf.Sign(player.position.x - rb.position.x);


            if (currentDir != initialDir)
            {
                rb.linearVelocity = Vector2.zero;          // Hızı sıfırla
                owner.OnAttackComplete();
                SetFacing(initialDir);
                yield break;                         // Korutini bitir
            }


            rb.linearVelocity = new Vector2(initialDir * speed, rb.linearVelocity.y);

            yield return new WaitForFixedUpdate();
        }
    }


    private void SetFacing(float dirX)
    {
        float yAngle = dirX < 0f ? 270f : 90f;
        owner.transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
    }


    public void OnCollisionEnter(Collision collision)
    {
        int layer = collision.gameObject.layer;

        // 1) Asansör kutusuyla çarpışma
        if (collision.collider.CompareTag("ElevatorBox"))
        {
            Debug.Log("Boar attack collided with ElevatorBox");
            foreach (Transform child in collision.collider.transform)
                if (child.CompareTag("Player"))
                {
                    child.SetParent(null);
                    break;
                }
            UnityEngine.Object.Destroy(collision.collider.gameObject);

            stunPoints--;
            if (stunPoints <= 0)
            {
                Debug.Log("Boar attack ended, applying stun");
                owner.Stun(owner.boarStunDuration);
            }
        }
        // 2) IDamageable bir şeye çarptıysa → hasar ver, saldırıyı bitir
        else if (collision.collider.GetComponent<IDamageable>() is IDamageable dmgTarget)
        {

            dmgTarget.TakeDamage(damage);


            Debug.Log("Boar attack hit damageable target, dealing damage");

            if (collision.collider.GetComponent<DamageKnockback>() is DamageKnockback knckTarget)
            {
                knckTarget.HandleKnockback(owner.gameObject);
            }

            owner.OnAttackComplete();
        }
        // 3) Zemin maskesi dışındaki her şeye çarptığında saldırıyı bitir
        else if ((owner.getGroundMask().value & (1 << layer)) == 0)
        {
            Debug.Log("Boar attack hit obstacle, ending attack");
            owner.OnAttackComplete();
        }
    }
}