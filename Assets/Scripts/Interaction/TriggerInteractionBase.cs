using Unity.VisualScripting;
using UnityEngine;

public class TriggerInteractionBase : MonoBehaviour, IInteractable
{
    public GameObject Player { get; set; }
    public bool canInteract { get; set; }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (canInteract)
        {
            //when button pressed
            if (InputManager.WasInteractPressed)
            {
                Interact();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {
            canInteract = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Player)
        {
            canInteract = false;
        }
    }

    public virtual void Interact() { }
}
