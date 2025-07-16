using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ElevatorBox : MonoBehaviour
{
    [Header("Hız Ayarları")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float boostMultiplier = 5f;


    [Header("Yaşam Süresi")]
    [SerializeField] private float lifeTime = 60f;


    private Vector3 moveDirection;
    private IBoxState currentState;
    private bool wasOutOfRange = false;

    [Header("Sınır referansı için GroundLayer")]
    [SerializeField] private LayerMask groundMask;

    [Header("Çarpıp döneceği objeler layeri")]
    [SerializeField] private LayerMask reverseDirectionMask;

    private Rigidbody rb;
    [SerializeField, Tooltip("Bu asansör kutunun çıkabileceği maksimum yükseklik")] private float maxDistance = 15;

    private Vector3 boxVelocityToAdd = Vector3.zero;
    private float boxVelocityTimer = 0;
    private bool isApplyingBoxVelocity = false;
    [SerializeField] private float boxVelocityDuration = 0.5f;

    private Vector3 previousPosition;
    public Vector3 Velocity { get; private set; }




    public bool isBoosted { get; private set; }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Başlangıçta baseSpeed ile başlar ve yaşam süresi sayacı başlar
    private void Start()
    {
        StartCoroutine(SelfDestructAfterDelay(lifeTime));

        DontDestroyOnLoad(this);



    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdate(this);
        Transform playerTransform = FindPlayerIfInside();
        if (IsOutOfRange(transform.position, groundMask) && !wasOutOfRange)
        {


            /*if (playerTransform != null && isBoosted)
            {
                LaunchPlayerIfOnTop(15f);
            }*/

            ReverseDirection();
            wasOutOfRange = true;
        }

        if (!IsOutOfRange(transform.position, groundMask))
        {
            wasOutOfRange = false;
        }



        Velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = transform.position;




        /*if (InputManager.WasJumpPressed && playerTransform != null && math.abs(moveDirection.x) > 0)
        {
            boxVelocityTimer = boxVelocityDuration;
            boxVelocityToAdd = GetCurrentVelocity();
            isApplyingBoxVelocity = true;
            Debug.Log("Oyuncuya hareket eden kutunun üzerinde zıpladığı için hız uygulandı!");
        }


        GameObject player = GameObject.FindWithTag("Player");
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        IJumpResponse jump = player.GetComponent<IJumpResponse>();

        if (isApplyingBoxVelocity)
        {
            if (boxVelocityTimer > 0f)
            {
                // Süre boyunca velocity ekle
                playerRb.linearVelocity += boxVelocityToAdd;
                boxVelocityTimer -= Time.fixedDeltaTime;
            }
            else
            {
                // Azaltarak sıfırla
                boxVelocityToAdd = Vector3.Lerp(boxVelocityToAdd, Vector3.zero, 5f * Time.fixedDeltaTime);
                playerRb.linearVelocity += boxVelocityToAdd;

                if (jump.isGrounded)
                {
                    boxVelocityToAdd = Vector3.Lerp(boxVelocityToAdd, Vector3.zero, 10f * Time.fixedDeltaTime);
                }

                // Eğer zemin temas ettiyse veya çok küçüldüyse kapat
                if (jump.isGrounded && boxVelocityToAdd.magnitude < 0.1f)
                {
                    isApplyingBoxVelocity = false;
                    boxVelocityToAdd = Vector3.zero;
                }




            }
        }*/









    }

    // State yönetimi
    public void SetState(IBoxState newState)
    {
        currentState = newState;
        currentState.Enter(this);
    }

    public void StartMoving(Vector3 direction)
    {
        moveDirection = direction.normalized;
        SetState(new BoxMovingState());
    }

    public void Stop()
    {
        SetState(new BoxIdleState());
    }

    public void Move()
    {
        //transform.position += moveDirection * baseSpeed * Time.deltaTime;
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDirection.x * baseSpeed;
        velocity.y = moveDirection.y * baseSpeed;

        rb.linearVelocity = velocity;

    }

    public void MoveFast()
    {
        Vector3 direction = moveDirection.normalized;
        float speed = baseSpeed * boostMultiplier;

        // Engelleri önceden tespit et
        if (!Physics.Raycast(rb.position, direction, out RaycastHit hit, speed * Time.fixedDeltaTime + 0.1f, reverseDirectionMask))
        {
            //transform.position += direction * speed * Time.deltaTime;
            Vector3 velocity = rb.linearVelocity;
            velocity.x = moveDirection.x * speed;
            velocity.y = moveDirection.y * speed;
            rb.linearVelocity = velocity;
            isBoosted = true;
        }
        else
        {
            Debug.Log("BOOST: Yol kapalı, saplanmamak için hızlı gitmedi.");
            ReverseDirection();
        }
    }



    public void ReverseDirection()
    {
        moveDirection = -moveDirection;
        ResetSpeed();

    }
    public void ResetSpeed()
    {
        SetState(new BoxMovingState());
        isBoosted = false;
    }

    public Vector3 GetMoveDirection() => moveDirection;

    public Transform FindPlayerIfInside()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
                return child;
        }
        return null;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //collision.transform.SetParent(transform);
            //print("Player kutunun üstünde.");
        }
        else
        {
            ReverseDirection();


        }








    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {



            collision.transform.SetParent(null);
            DontDestroyOnLoad(collision.gameObject);




        }
    }


    private IEnumerator SelfDestructAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Oyuncuyu varsa ayır
        Transform player = FindPlayerIfInside();
        if (player != null)
        {
            player.SetParent(null);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Breakable") && isBoosted)
        {
            BreakableFloorTile tile = other.GetComponent<BreakableFloorTile>();
            if (tile != null)
            {
                tile.Break();
                print("Breakable floor kırıldı!");
            }
        }





    }

    private Vector3 CalculateLaunchDirection()
    {
        Vector3 dir = moveDirection.normalized;

        // Eğer neredeyse yukarı gidiyorsa (örneğin asansör yukarı çıkıyorsa)
        if (Vector3.Dot(dir, Vector3.up) > 0.7f)
        {
            return Vector3.up;
        }

        // Aksi halde yukarı + yön birleşimi (sağ/sol + yukarı)
        Vector3 launchDir = (dir + Vector3.up).normalized;
        return launchDir;
    }

    public void LaunchPlayerIfOnTop(float force = 8f)
    {
        Transform player = FindPlayerIfInside();
        if (player == null) return;

        player.SetParent(null);
        DontDestroyOnLoad(player.gameObject);

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null) return;

        Vector3 launchDir = CalculateLaunchDirection();
        playerRb.AddForce(launchDir * force, ForceMode.VelocityChange);
    }


    private bool IsOutOfRange(Vector3 origin, LayerMask mask)
    {
        return !Physics.Raycast(origin, Vector3.down, maxDistance, mask);
    }

    public Vector3 GetCurrentVelocity()
    {
        float currentSpeed = isBoosted ? baseSpeed * boostMultiplier : baseSpeed;
        return moveDirection.normalized * currentSpeed;
    }

    public bool IsTouchingReverseDirectionObjects()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return false;

        Vector3 center = transform.position + col.center;
        Vector3 halfExtents = col.size * 0.5f;

        return Physics.CheckBox(center, halfExtents, transform.rotation, reverseDirectionMask, QueryTriggerInteraction.Ignore);
    }




}
