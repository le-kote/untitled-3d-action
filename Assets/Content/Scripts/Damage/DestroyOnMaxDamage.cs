using UnityEngine;

/// <summary>
/// This component destroys owner when it reaches max damage
/// </summary>
[RequireComponent(typeof(Damageable))]
public class DestroyOnMaxDamage : MonoBehaviour
{
    private Damageable _damageable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _damageable = GetComponent<Damageable>();
        _damageable.OnMaxDamageReached += () => Destroy(gameObject);
    }

    void OnDisable()
    {
        _damageable.OnMaxDamageReached -= () => Destroy(gameObject);
    }
}
