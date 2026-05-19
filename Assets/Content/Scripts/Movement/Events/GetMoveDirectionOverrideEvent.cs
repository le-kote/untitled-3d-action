using UnityEngine;

public record struct GetMoveDirectionOverrideEvent()
{
    public Vector3 Dir = Vector3.zero;
    public bool Handled = false;
}
