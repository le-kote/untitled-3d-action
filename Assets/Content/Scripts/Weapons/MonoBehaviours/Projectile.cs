using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public Vector3 Velocity = Vector3.zero;

    [SerializeField]
    private Rigidbody _rb;

    [SerializeField]
    private Collider _collider;

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(Velocity.x, _rb.linearVelocity.y, Velocity.z);
    }

    public void Launch(Collider userCollider, Vector3 force)
    {
        Physics.IgnoreCollision(userCollider, _collider, true);

        Velocity = force;
        _rb.linearVelocity = force;
    }
}
