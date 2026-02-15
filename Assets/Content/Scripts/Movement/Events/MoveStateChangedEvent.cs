using UnityEngine;

public sealed class MoveStateChangedEvent : GameEventArgs
{
    public MoveState PrevState;
    public MoveState State;

    public MoveStateChangedEvent (MoveState prevState, MoveState moveState)
    {
        PrevState = prevState;
        State = moveState;
    }
}
