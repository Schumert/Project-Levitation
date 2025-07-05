using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    [SerializeField] public float customGravity = -30f;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(Vector3.up * customGravity, ForceMode.Acceleration);
    }
}
