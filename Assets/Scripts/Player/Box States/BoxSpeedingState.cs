using UnityEngine;
public class BoxSpeedingState : IBoxState
{
    public void Enter(ElevatorBox box)
    {
    }

    public void FixedUpdate(ElevatorBox box)
    {
        box.MoveFast();


    }
}
