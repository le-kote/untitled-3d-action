using UnityEngine;

/// <summary>
/// This component allows user to perform directional horizontal dash
/// </summary>
[RequireComponent(typeof(GenericMovement), typeof(CharacterController))]
public class Dash : MonoBehaviour
{
    [SerializeField]
    private float _impulse = 6f;

    [SerializeField]
    private float _cooldown = 3f;

    [SerializeField]
    private float _duration = 3f;

    private float _cooldownAccumulator = 3f;

    private GenericMovement _movement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_cooldownAccumulator > 0)
            _cooldownAccumulator -= Time.deltaTime;
    }

    public void DoDash()
    {
        if (_cooldownAccumulator > 0)
            return;

        var dashTo = _movement.Input != Vector2.zero ? _movement.GetMoveDirection() : transform.forward;
        _movement.ApplyForce(dashTo.normalized * _impulse);
        _cooldownAccumulator = _cooldown;
    }
}
