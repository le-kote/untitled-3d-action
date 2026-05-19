using UnityEngine;

public record struct GetMoveAccelerationOverrideEvent()
{
    public float Acceleration = 0f;
    public float Deceleration = 0f;
    public bool Handled = false;
}
