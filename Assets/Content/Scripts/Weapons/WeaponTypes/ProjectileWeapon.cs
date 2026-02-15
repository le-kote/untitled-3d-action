using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Weapon", menuName = "Scriptable Objects/Weapons/Projectile Weapon")]
public sealed class ProjectileWeapon : BaseWeapon
{
    public GameObject Projectile;
    public float ProjectileSpeed;
    public int FiredProjectiles;
    public Vector2 Spread;

    public int Capacity = 15;
    public float ReloadDuration;
}
