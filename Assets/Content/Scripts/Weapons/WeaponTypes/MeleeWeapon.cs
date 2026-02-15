using UnityEngine;

[CreateAssetMenu(fileName = "Melee Weapon", menuName = "Scriptable Objects/Weapons/Melee Weapon")]
public sealed class MeleeWeapon : BaseWeapon
{
    public float Damage;
    public float AttackRange;
    public float AttackWidth;
    public float AttackHeight;
    public int MaxTargets = 1;
    public LayerMask Layers;
}
