using UnityEngine;

public record struct CanJumpEvent(bool Grounded, bool CanJump)
{
    public bool Handled = false;
}
