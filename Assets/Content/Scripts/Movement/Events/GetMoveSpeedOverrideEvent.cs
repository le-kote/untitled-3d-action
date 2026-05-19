using UnityEngine;

public record struct GetMoveSpeedOverrideEvent()
{
    public float Speed = 0f;
    public bool Handled = false;
}
