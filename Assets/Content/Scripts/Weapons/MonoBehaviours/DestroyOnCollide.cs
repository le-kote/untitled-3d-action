using UnityEngine;

public class DestroyOnCollide : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (gameObject.TryGetComponent<Projectile>(out var proj) && collision.gameObject == proj.Player)
            return;

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameObject.TryGetComponent<Projectile>(out var proj) && other.gameObject == proj.Player)
            return;

        Destroy(gameObject);
    }
}
