using UnityEngine;

public interface IInteractable
{
    GameObject Player { get; set; }

    bool canInteract { get; set; }

    void Interact();
}
