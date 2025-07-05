using System.Collections;
using UnityEngine;

public class ElevatorBox : MonoBehaviour
{
    [Header("Hız Ayarları")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float boostMultiplier = 5f;


    [Header("Yaşam Süresi")]
    [SerializeField] private float lifeTime = 60f;

    private float currentSpeed;
    private Vector3 moveDirection;
    private IBoxState currentState;

    private Vector3 lastPosition;
    private bool wasOutOfRange = false;
    private LayerMask mask;


    public Vector3 Velocity { get; private set; }




    public bool isBoosted { get; private set; }

    // Başlangıçta baseSpeed ile başlar ve yaşam süresi sayacı başlar
    private void Start()
    {
        currentSpeed = baseSpeed;
        StartCoroutine(SelfDestructAfterDelay(lifeTime));
        lastPosition = transform.position;


        mask = ~(1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Elevator"));

        DontDestroyOnLoad(this);



    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdate(this);

        Vector3 newPosition = transform.position;
        Velocity = (newPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = newPosition;
        Vector3 origin = transform.position - new Vector3(0, GetComponent<BoxCollider>().bounds.extents.y, 0);
        if (BoxSpawner.IsOutOfRange(origin, mask) && wasOutOfRange)
        {
            Transform playerTransform = FindPlayerIfInside();
            if (playerTransform != null && isBoosted)
            {
                MovementController player = playerTransform.GetComponent<MovementController>();
                player.AddForce(20, GetMoveDirection().normalized);
            }


            ReverseDirection();
            wasOutOfRange = false;
        }

        if (Physics.Raycast(transform.position - new Vector3(0, GetComponent<BoxCollider>().bounds.extents.y, 0), Vector3.down, 5, mask))
        {
            wasOutOfRange = true;

        }



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
        transform.position += moveDirection * baseSpeed * Time.deltaTime;

    }
    public void MoveFast()
    {
        transform.position += moveDirection * (baseSpeed * boostMultiplier) * Time.deltaTime;
        isBoosted = true;
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
            collision.transform.SetParent(transform);
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
            //print("Player kutudan ayrıldı.");


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
        if (!isBoosted) return;

        if (other.CompareTag("Breakable"))
        {
            BreakableFloorTile tile = other.GetComponent<BreakableFloorTile>();
            if (tile != null)
            {
                tile.Break();
                print("Breakable floor kırıldı!");
            }
        }
    }



}
