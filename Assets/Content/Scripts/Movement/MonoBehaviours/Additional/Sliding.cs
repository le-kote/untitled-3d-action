using UnityEngine;

/// <summary>
/// This component allows user to slide on sprint-crouching
/// </summary>
[RequireComponent(typeof(GenericMovement))]
public class Sliding : FancyBehaviour
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

    protected override void InitializeEvents()
    {
        base.InitializeEvents();

        SubscribeLocalEvent<JumpEvent>(OnJump);
        SubscribeLocalEvent<MoveStateChangedEvent>(OnMoveStateChanged);
        SubscribeLocalEvent<GetMoveDirectionOverrideEvent>(OnMoveDirOverride);
        SubscribeLocalEvent<GetMoveAccelerationOverrideEvent>(OnMoveAccelerationOverride);
        SubscribeLocalEvent<GetMoveSpeedOverrideEvent>(OnMoveSpeedOverride);
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

    private void OnJump(ref JumpEvent ev)
    {
        if (!_isSliding)
            return;

        _movement.ApplyForce(_movement.Velocity.normalized * _slideJumpSpeed);
        _movement.SetMoveState(MoveState.Running);
        StopSlide();
    }

    private void OnMoveStateChanged(ref MoveStateChangedEvent ev)
    {
        if (ev.PrevState == MoveState.Crouching)
        {
            _isSliding = false;
            _direction = Vector3.zero;
            return;
        }

        if (ev.PrevState != MoveState.Running || ev.State != MoveState.Crouching)
            return;

        StartSlide();
    }

    private void OnMoveDirOverride(ref GetMoveDirectionOverrideEvent ev)
    {
        if (!_isSliding)
            return;

        ev.Handled = true;
        ev.Dir = _direction;
    }

    private void OnMoveAccelerationOverride(ref GetMoveAccelerationOverrideEvent ev)
    {
        if (!_isSliding || !_movement.IsGrounded)
            return;

        ev.Acceleration = _slideAcceleration;
        ev.Deceleration = _slideDeceleration;
        ev.Handled = true;
    }

    private void OnMoveSpeedOverride(ref GetMoveSpeedOverrideEvent ev)
    {
        if (!_isSliding)
            return;

        ev.Speed = _slideSpeed;
        ev.Handled = true;
    }
}
