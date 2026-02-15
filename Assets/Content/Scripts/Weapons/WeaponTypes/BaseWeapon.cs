using UnityEngine;

public abstract class BaseWeapon : ScriptableObject
{
    /// <summary>
    /// Delay before attack, applied on attack press
    /// </summary>
    public float DelayBeforeAttack;

    /// <summary>
    /// Delay between attacks
    /// </summary>
    public float AttackRate;

    /// <summary>
    /// Should we auto-attack when holding attack button
    /// </summary>
    public bool Automatic;

    /// <summary>
    /// Should we stop all queued attacks on button up
    /// </summary>
    public bool StopAttacksOnButtonUp;
}
