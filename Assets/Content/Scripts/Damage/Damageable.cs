using UnityEngine;

/// <summary>
/// This component defines game object health
/// </summary>
public class Damageable : MonoBehaviour
{
    [SerializeField]
    private float _health = 100f;

    [SerializeField]
    private float _damage = 0f;

    [Header("Events")]
    [SerializeField] private GameEvent _damageChangedEvent;
    [SerializeField] private GameEvent _maxDamageReachedEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var damageChangedData = new DamageChangedData { CurrentDamage = _damage, MaxHealth = _health };
        _damageChangedEvent.Raise(this, damageChangedData);
    }

    public void TryChangeDamage(float damage)
    {
        _damage += damage;

        var damageChangedData = new DamageChangedData { CurrentDamage = _damage, MaxHealth = _health };
        _damageChangedEvent.Raise(this, damageChangedData);

        if (_damage >= _health)
        {
            _maxDamageReachedEvent.Raise(this, null);
        }
    }
}
