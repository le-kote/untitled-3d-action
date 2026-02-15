using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This component defines game object health
/// </summary>
public class Damageable : MonoBehaviour
{
    [SerializeField]
    private float _health = 100f;

    [SerializeField]
    private float _damage = 0f;

    public UnityAction<float, float> OnDamageChanged;
    public UnityAction OnMaxDamageReached;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnDamageChanged?.Invoke(_damage, _health);
    }

    public void TryChangeDamage(float damage)
    {
        _damage += damage;

        OnDamageChanged?.Invoke(_damage, _health);

        if (_damage >= _health)
            OnMaxDamageReached?.Invoke();
    }
}
