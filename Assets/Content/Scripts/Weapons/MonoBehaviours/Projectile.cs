using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public Vector3 Velocity = Vector3.zero;
    private Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(Velocity.x, _rb.linearVelocity.y, Velocity.z);
    }

    public void Launch(Vector3 force)
    {
        _rb = gameObject.GetOrAddComponent<Rigidbody>();

        Velocity = force;
        _rb.linearVelocity = force;
    }
}
