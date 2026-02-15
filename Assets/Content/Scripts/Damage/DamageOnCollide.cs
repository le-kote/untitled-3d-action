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
        if (!collision.gameObject.TryGetComponent<Damageable>(out var damageable))
            return;

        damageable.TryChangeDamage(_dealtDamage);
    }
}
