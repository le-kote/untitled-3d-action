using UnityEngine;

[CreateAssetMenu(fileName = "Melee Weapon", menuName = "Scriptable Objects/Weapons/Melee Weapon")]
public sealed class MeleeWeapon : BaseWeapon
{
    /// <summary>
    /// Dealt damage
    /// </summary>
    public float Damage;

    /// <summary>
    /// Max range for this attack
    /// </summary>
    public float AttackRange;

    /// <summary>
    /// Width of this attack
    /// </summary>
    public float AttackWidth;

    /// <summary>
    /// Height of this attack
    /// </summary>
    public float AttackHeight;

    /// <summary>
    /// Max targets that can be damaged per one hit
    /// </summary>
    public int MaxTargets = 1;

    /// <summary>
    /// Layers that will be checked when attacking
    /// </summary>
    public LayerMask Layers;
}
