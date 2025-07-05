using System.Collections;
using UnityEngine;

public interface IAttackBehavior
{

    int stunPoints { get; }
    float damage { get; }
    LayerMask targetLayer { get; }


    /// <summary>
    /// Executes the attack behavior as a coroutine.
    /// </summary>
    IEnumerator Execute();

    /// <summary>
    /// Called from owner when a collision occurs during attack.
    /// </summary>
    void OnCollisionEnter(Collision collision);
}