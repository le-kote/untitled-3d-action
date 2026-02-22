using UnityEngine;

/// <summary>
/// This component makes owner deal damage on collide
/// </summary>
public class DamageOnCollide : MonoBehaviour
{
    [SerializeField]
    private float _dealtDamage;

    void OnCollisionEnter(Collision collision)
    {
        if (gameObject.TryGetComponent<Projectile>(out var proj) && collision.collider == proj.Player)
            return;

        if (!collision.gameObject.TryGetComponent<Damageable>(out var damageable))
            return;

        damageable.TryChangeDamage(_dealtDamage);
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameObject.TryGetComponent<Projectile>(out var proj) && other == proj.Player)
            return;

        if (!other.gameObject.TryGetComponent<Damageable>(out var damageable))
            return;

        damageable.TryChangeDamage(_dealtDamage);
    }
}
