using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public record struct MoveStateChangeData(MoveState Previous, MoveState State);

public sealed class BoolEventData
{
    public bool Value;
    public bool Handled = false;

    public BoolEventData(bool startingValue)
    {
        Value = startingValue;
    }
}

public sealed class MoveDirectionOverrideData
{
    public Vector3 Dir;
    public bool Handled = false;
}

public sealed class MoveSpeedOverrideData
{
    public float Speed;
    public bool Handled = false;
}

public sealed class MoveSpeedModifiersData
{
    public float Modifier = 1f;
    public bool Handled = false;
}

public sealed class AccelOverrideData
{
    public float Accel;
    public float Decel;
    public bool Handled = false;
}

public record struct DamageChangedData(float CurrentDamage, float MaxHealth);

public sealed class CanJumpData
{
    public bool CanJump = false;
    public bool Grounded = false;
    public bool Handled = false;
}
