using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public Vector3 Velocity = Vector3.zero;

    [SerializeField]
    private Rigidbody _rb;

    [SerializeField]
    private Collider _collider;

    public GameObject Player;

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(Velocity.x, _rb.linearVelocity.y, Velocity.z);
    }

    public void Launch(GameObject player, Vector3 force)
    {
        Player = player;
        Velocity = force;
        _rb.linearVelocity = force;
    }
}
