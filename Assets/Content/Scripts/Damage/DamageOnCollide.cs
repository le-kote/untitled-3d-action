using UnityEngine;

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
