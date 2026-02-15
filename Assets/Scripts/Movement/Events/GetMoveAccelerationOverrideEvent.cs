using UnityEngine;

public sealed class GetMoveAccelerationOverrideEvent : HandledGameEventArgs
{
    public float Acceleration = 0f;
    public float Deceleration = 0f;
}
