using UnityEngine;

public class BoxMovingState : IBoxState
{
    public void Enter(ElevatorBox box) { }

    public void FixedUpdate(ElevatorBox box)
    {

        box.Move();
    }
}
