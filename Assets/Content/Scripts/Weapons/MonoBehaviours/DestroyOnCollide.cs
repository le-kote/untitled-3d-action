using UnityEngine;

public class DestroyOnCollide : FancyBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (gameObject.TryGetComponent<Projectile>(out var proj) && collision.gameObject == proj.Player)
            return;

        PoolHide(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameObject.TryGetComponent<Projectile>(out var proj) && other.gameObject == proj.Player)
            return;

        PoolHide(gameObject);
    }
}
