using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Weapon", menuName = "Scriptable Objects/Weapons/Projectile Weapon")]
public sealed class ProjectileWeapon : BaseWeapon
{
    /// <summary>
    /// Used projectile prefab
    /// </summary>
    public GameObject Projectile;

    /// <summary>
    /// Projectile launch speed
    /// </summary>
    public float ProjectileSpeed;

    /// <summary>
    /// Count of projectiles launched at the same time
    /// for shotguns
    /// </summary>
    public int FiredProjectiles;

    /// <summary>
    /// How long will projectile last
    /// </summary>
    public float ProjectileLifetime;

    /// <summary>
    /// Max spread for projectiles
    /// </summary>
    public Vector2 Spread;

    /// <summary>
    /// Should we apply spread for first projectile or it should always be fired at the center
    /// </summary>
    public bool ApplySpreadToFirstProjectile;

    public float PhysicalRecoil;
}
