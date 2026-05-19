using UnityEngine;

public record struct MoveStateChangedEvent(MoveState PrevState, MoveState State);
