using NUnit.Framework;
using UnityEngine;

/// <summary>
/// This component allows user to slide on sprint-crouching
/// </summary>
[RequireComponent(typeof(GenericMovement))]
public class Sliding : MonoBehaviour
{
    [SerializeField]
    private float _slideDuration = 3f;

    [SerializeField]
    private float _slideSpeed = 12f;

    [SerializeField]
    private float _slideJumpSpeed = 12f;

    [SerializeField]
    private float _slideDeceleration = 6f;

    [SerializeField]
    private float _slideAcceleration = 30f;

    private GenericMovement _movement;
    private Vector3 _direction = Vector3.zero;
    private float _slideAccumulator = 0f;
    private bool _isSliding = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        SlideMovement();
    }

    private void StartSlide()
    {
        _isSliding = true;
        _slideAccumulator = 0f;
        _direction = _movement.Input.y * transform.forward + _movement.Input.x * transform.right;
    }

    private void SlideMovement()
    {
        _slideAccumulator += Time.deltaTime;

        if (!_movement.IsGrounded)
            _slideAccumulator = 0f;

        if (_slideAccumulator >= _slideDuration)
            StopSlide();
    }

    private void StopSlide()
    {
        _isSliding = false;
        _direction = Vector3.zero;
    }

    public void OnAccelOverride(Component sender, object data)
    {
        if (data is not AccelOverrideData ev)
            return;

        if (sender.gameObject != gameObject)
            return;

        if (!_isSliding || !_movement.IsGrounded)
            return;

        ev.Accel = _slideAcceleration;
        ev.Decel = _slideDeceleration;
        ev.Handled = true;
    }

    public void OnMoveSpeedOverride(Component sender, object data)
    {
        if (data is not MoveSpeedOverrideData ev)
            return;

        if (sender.gameObject != gameObject)
            return;

        ev.Speed = _slideSpeed;
        ev.Handled = true;
    }

    public void OnJump(Component sender, object data)
    {
        if (sender.gameObject != gameObject)
            return;

        if (!_isSliding)
            return;

        _movement.SetVelocity(_movement.Velocity.normalized * _slideSpeed);
        _movement.SetMoveState(MoveState.Running);
        StopSlide();
    }

    public void OnMoveStateChanged(Component sender, object data)
    {
        if (data is not MoveStateChangeData ev)
            return;

        if (sender.gameObject != gameObject)
            return;

        if (ev.Previous == MoveState.Crouching)
        {
            _isSliding = false;
            _direction = Vector3.zero;
            return;
        }

        if (ev.Previous != MoveState.Running || ev.State != MoveState.Crouching)
            return;

        StartSlide();
    }

    public void OnMoveDirOverride(Component sender, object data)
    {
        if (data is not MoveDirectionOverrideData ev)
            return;

        if (sender.gameObject != gameObject)
            return;

        if (!_isSliding)
            return;

        ev.Handled = true;
        ev.Dir = _direction;
    }
}
