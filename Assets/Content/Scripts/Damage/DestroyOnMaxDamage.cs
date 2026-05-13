using UnityEngine;

/// <summary>
/// This component destroys owner when it reaches max damage
/// </summary>
[RequireComponent(typeof(Damageable))]
public class DestroyOnMaxDamage : MonoBehaviour
{
    public void OnMaxDamageReached(Component sender, object eventData)
    {
        if (sender.gameObject != gameObject)
            return;

        Destroy(gameObject);
    }
}
