using UnityEngine;

public sealed class CanJumpEvent : HandledGameEventArgs
{
    public bool CanJump = false;
    public bool Grounded = false;

    public CanJumpEvent(bool isGrounded)
    {
        Grounded = isGrounded;
    }
}
