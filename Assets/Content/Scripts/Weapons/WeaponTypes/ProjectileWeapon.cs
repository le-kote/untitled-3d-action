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
    public AudioClip FireSound;

    /// <summary>
    /// Volume of the fire sound (0-1)
    /// </summary>
    [Range(0f, 1f)]
    public float FireSoundVolume = 0.5f;

    /// <summary>
    /// Should the fire sound be played at the weapon position (3D) or globally (2D)
    /// </summary>
    public bool IsFireSound3D = true;

    /// <summary>
    /// Audio mixer group for weapon sounds (optional)
    /// </summary>
    public UnityEngine.Audio.AudioMixerGroup AudioMixerGroup;
}