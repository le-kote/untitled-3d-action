using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "Projectile Weapon", menuName = "Scriptable Objects/Weapons/Projectile Weapon")]
public sealed class ProjectileWeapon : BaseWeapon
{
    /// <summary>
    /// Used projectile prefab
    /// </summary>
    public AssetReference Projectile;

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

    [Header("Damage Settings")]
    /// <summary>
    /// Base damage dealt by projectile
    /// </summary>
    public float Damage = 10f;

    /// <summary>
    /// Optional: Damage falloff over distance
    /// </summary>
    public AnimationCurve DamageFalloff = AnimationCurve.Linear(0, 1, 100, 0.5f);

    /// <summary>
    /// Maximum distance for damage calculation
    /// </summary>
    public float MaxDamageRange = 100f;

    [Header("Audio Settings")]
    /// <summary>
    /// Sound played when weapon fires
    /// </summary>
    public AudioCompound FireSound = new();
}
