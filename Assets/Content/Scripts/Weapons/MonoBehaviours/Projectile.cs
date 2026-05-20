using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : FancyBehaviour
{
    public GameObject Player { get; private set; }

    private Vector3 _velocity = Vector3.zero;
    private Rigidbody _rb;

    private float _lifeTimer = 0f;

    void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(_velocity.x, _rb.linearVelocity.y, _velocity.z);

        _lifeTimer -= Time.fixedDeltaTime;

        if (_lifeTimer <= 0)
        {
            PoolHide(gameObject);
        }
    }

    public void Launch(GameObject player, Vector3 force, float lifetime)
    {
        Player = player;
        _velocity = force;
        _lifeTimer = lifetime;

        _rb.linearVelocity = force;
    }
}
