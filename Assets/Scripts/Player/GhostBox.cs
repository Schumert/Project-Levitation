using System;
using System.Collections.Generic;
using UnityEngine;

public class GhostBox : MonoBehaviour
{

    private Rigidbody rb;
    private Vector2 moveValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        moveValue = InputManager.BoxMoveInput;
    }

    void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;




        if (Math.Abs(transform.position.y - transform.position.y) <= 20)
        {
            velocity.y = moveValue.y * 10;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, 0);
        }

        if (Math.Abs(transform.position.x - transform.position.x) <= 20)
        {
            velocity.x = moveValue.x * 10;

        }
        else
        {
            transform.position = new Vector3(transform.position.x - 1, transform.position.y, 0);
        }


        rb.linearVelocity = velocity;
    }
}
