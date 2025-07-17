using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BarbarianController : MonoBehaviour
{
    public enum AttackType { Boar, Bear }

    [Header("General Stats")]
    [SerializeField] private int bossHP = 10;
    [SerializeField] private float timeBetweenAttacks = 1f;
    [SerializeField] private float customGravity = 10f;

    [Header("Boar Attack Settings")]
    [SerializeField] private float boarSpeed = 5f;
    [SerializeField] private int boarStunPoints = 2;
    [SerializeField] public float boarStunDuration = 1.5f;
    [SerializeField] private float boarDamage = 1f;

    [Header("Bear Attack Settings")]
    [SerializeField] private float bearXForce = 10f;
    [SerializeField] private float bearYForce = 5f;
    [SerializeField] private int bearStunPoints = 2;
    [SerializeField] private float bearStunDuration = 1f;
    [SerializeField] private float waitAfterLand = 0.5f;
    [SerializeField] private float attackDuration = 5f;
    [SerializeField] private float bearDamage = 1f;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] float checkRadius = 0.15f;
    [SerializeField] LayerMask groundMask;

    private Rigidbody rb;
    private IAttackBehavior currentAttack;
    private Coroutine attackRoutine;
    private AttackType currentAttackType;
    public bool isGrounded { get; private set; }

    public bool IsAttacking => attackRoutine != null;
    public bool IsStunned { get; private set; }



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Invoke(nameof(ChooseNextAttack), timeBetweenAttacks);
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.up * customGravity, ForceMode.Acceleration);
        isGrounded = IsTouchingGround();

    }

    private void ChooseNextAttack()
    {
        currentAttackType = (UnityEngine.Random.value < 0.5f) ? AttackType.Boar : AttackType.Bear;
        StartAttack(currentAttackType);
    }

    private void StartAttack(AttackType type)
    {
        Debug.Log("Starting attack: " + Enum.GetName(typeof(AttackType), type));
        StopCurrentAttack();
        switch (type)
        {
            case AttackType.Boar:
                currentAttack = new BoarAttackBehavior(this, playerTransform, rb, boarSpeed, boarStunPoints, boarDamage, targetLayer);
                break;
            case AttackType.Bear:
                currentAttack = new BearAttackBehavior(
                    this,
                    rb,
                    playerTransform,
                    bearXForce,
                    bearYForce,
                    bearStunPoints,
                    bearStunDuration,
                    waitAfterLand,
                    attackDuration,
                    bearDamage,
                    targetLayer);
                break;
        }
        attackRoutine = StartCoroutine(currentAttack.Execute());
    }



    private void StopCurrentAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsAttacking)
            currentAttack?.OnCollisionEnter(collision);
        else if (IsStunned && collision.collider.CompareTag("ElevatorBox"))
        {
            // Handle stun-phase damage
            Debug.Log("Stun-phase collision with ElevatorBox");
            Destroy(collision.collider.gameObject);
            Debug.Log("Damage event in stun-phase");
            TakeDamage(1);
        }
    }

    public void OnAttackComplete()
    {
        Debug.Log("Attack complete: " + Enum.GetName(typeof(AttackType), currentAttackType));
        StopCurrentAttack();
        Invoke(nameof(ChooseNextAttack), timeBetweenAttacks);
    }

    public void Stun(float duration)
    {
        Debug.Log("Applying stun after attack: " + Enum.GetName(typeof(AttackType), currentAttackType));
        StopCurrentAttack();
        IsStunned = true;
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        IsStunned = false;
        Invoke(nameof(ChooseNextAttack), timeBetweenAttacks);
    }

    public void TakeDamage(int damage)
    {
        bossHP -= damage;
        Debug.Log("Boss HP decreased by " + damage + ", current HP: " + bossHP + " during " + Enum.GetName(typeof(AttackType), currentAttackType));
        if (bossHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Boss died");
        Destroy(gameObject);
    }

    bool IsTouchingGround()
    {
        return Physics.CheckSphere(
        groundCheckPoint.position,
        checkRadius,
        groundMask
    );
    }



    public LayerMask getGroundMask()
    {
        return groundMask;
    }

}
