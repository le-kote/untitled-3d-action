using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class GenericMovement
{
    [Header("Movement", order = 1)]
    [SerializeField]
    private float _moveSpeed = 7;

    [SerializeField]
    private float _sprintSpeed = 10;

    [SerializeField]
    private float _crouchSpeed = 4;

    [SerializeField]
    private float _smallJumpHeight = 2;

    [SerializeField]
    private float _bigJumpHeight = 4;

    [SerializeField]
    private bool _bigJumpSelected = false;

    [SerializeField]
    private float _gravity = -9.8f;

    [SerializeField]
    public bool UseGravity = true;

    [SerializeField]
    private float _slopeFriction = 0.3f;

    [SerializeField]
    private float _jumpReducionOnRelease = 0.5f;

    [SerializeField]
    private float _jumpReducionThreshold = 5f;

    [SerializeField]
    private float _coyoteTime = .1f;

    [Header("Crouching")]

    [SerializeField]
    private float _crouchHeight = 0.4f;

    [SerializeField]
    private float _height = 2f;

    [SerializeField]
    private float _heightChangeSpeed = 25f;

    [Header("Acceleration")]

    [SerializeField]
    [Tooltip("How fast player accelerates when walking or crouching on ground")]
    private float _acceleration = 15f;

    [SerializeField]
    [Tooltip("How fast player accelerates in air")]
    private float _airAcceleration = 8f;

    [SerializeField]
    [Tooltip("How fast player decelerates when walking or crouching on ground")]
    private float _deceleration = 12f;

    [SerializeField]
    [Tooltip("How fast player decelerates in air")]
    private float _airDeceleration = 0f;

    [SerializeField]
    [Tooltip("How fast player accelerates when sprinting on ground")]
    private float _sprintAcceleration = 20f;

    [SerializeField]
    [Tooltip("How fast player decelerates when sprinting on ground")]
    private float _sprintDeceleration = 15f;

    private float _jumpHeight => _bigJumpSelected ? _bigJumpHeight : _smallJumpHeight;
    private bool _jumping = false;
    private float _coyoteTimer = 0f;
    private Vector3 _groundNormal = Vector3.up;

    private void UpdateGrounded()
    {
        var grounded = _cc.isGrounded && Vector3.Angle(_groundNormal, Vector3.up) <= _cc.slopeLimit;

        if (!IsGrounded && grounded)
        {
            PlayLandSound();
            _coyoteTimer = 0f;
        }

        IsGrounded = grounded;
    }

    private void UpdateMovement()
    {
        if (Input.y <= 0 && CurrentMoveState == MoveState.Running && IsGrounded)
            SetMoveState(MoveState.Walking);

        if (!IsGrounded)
            _coyoteTimer += Time.deltaTime;

        GetVelocity();
        var targetVelocity = new Vector3(Velocity.x, Velocity.y, Velocity.z);

        _cc.Move(targetVelocity * Time.deltaTime);
    }

    private void UpdateHeight()
    {
        var startingHeight = _cc.height;
        var targetHeight = CurrentMoveState == MoveState.Crouching ? _crouchHeight : _height;

        if (startingHeight == targetHeight)
            return;

        var height = Mathf.Lerp(startingHeight, targetHeight, _heightChangeSpeed * Time.deltaTime);
        var diff = Mathf.Abs(startingHeight - height);
        if (diff < 0.001f)
            height = targetHeight;

        var change = _heightChangeSpeed * Time.deltaTime;

        _cc.height = Mathf.Lerp(_cc.height, height, change);
        transform.localScale = new Vector3(transform.localScale.x, height / 2, transform.localScale.z);

        if (diff > 0.001f && IsGrounded)
            _cc.Move(Vector3.up * -change);
    }

    private void GetVelocity()
    {
        var horizontalVelocity = GetHorizontalVelocity();

        var vel = new Vector3(horizontalVelocity.x, GetVerticalVelocity(), horizontalVelocity.z);

        Velocity = vel;
    }

    private float GetCurSpeed()
    {
        var speed = CurrentMoveState switch
        {
            MoveState.Walking => _moveSpeed,
            MoveState.Running => _sprintSpeed,
            MoveState.Crouching => _crouchSpeed,
            _ => _moveSpeed
        };

        var ev = new GetMoveSpeedOverrideEvent();
        this.RaiseEvent(ev);

        var modifiersEv = new GetMoveSpeedModifiersEvent();
        this.RaiseEvent(modifiersEv);

        var resultSpeed = (ev.Handled ? ev.Speed : speed) * modifiersEv.Modifier;
        return resultSpeed;
    }

    public Vector3 GetMoveDirection()
    {
        // Calculate desired direction in world space from local input
        Vector3 desiredDirection = Vector3.zero;
        if (Input.magnitude > 0.01f)
            desiredDirection = (transform.forward * Input.y + transform.right * Input.x).normalized;

        var dirEv = new GetMoveDirectionOverrideEvent();
        this.RaiseEvent(dirEv);

        if (dirEv.Handled)
            desiredDirection = dirEv.Dir.normalized;

        return desiredDirection;
    }

    private (float, float) GetAccelerationDeceleration()
    {
        var currentAccel = 0f;
        var currentDecel = 0f;

        if (!IsGrounded)
        {
            currentAccel = _airAcceleration;
            currentDecel = _airDeceleration;
        }
        else
        {
            currentAccel = CurrentMoveState == MoveState.Running ? _sprintAcceleration : _acceleration;
            currentDecel = CurrentMoveState == MoveState.Running ? _sprintDeceleration : _deceleration;
        }

        var ev = new GetMoveAccelerationOverrideEvent();
        this.RaiseEvent(ev);

        if (ev.Handled)
            (currentAccel, currentDecel) = (ev.Acceleration, ev.Deceleration);

        return (currentAccel, currentDecel);
    }

    private Vector3 GetHorizontalVelocity()
    {
        var speed = GetCurSpeed();
        var desiredDirection = GetMoveDirection();
        var (currentAccel, currentDecel) = GetAccelerationDeceleration();

        // Get horizontal velocity (world space)
        Vector3 horizontalVelocity = Velocity.Horizontal();

        // Add slope sliding force to velocity
        if (_cc.isGrounded && !IsGrounded)
        {
            horizontalVelocity += new Vector3(
                (1f - _groundNormal.y) * _groundNormal.x * (1f - _slopeFriction),
                0f,
                (1f - _groundNormal.y) * _groundNormal.z * (1f - _slopeFriction)
            );
        }

        // Apply acceleration/deceleration
        if (desiredDirection.magnitude > 0.01f)
        {
            Vector3 acceleration = desiredDirection * speed * currentAccel * Time.deltaTime;
            horizontalVelocity += acceleration;

            // Clamp to max speed
            if (horizontalVelocity.magnitude > speed)
            {
                horizontalVelocity = horizontalVelocity.normalized * speed;
            }
        }
        else
        {
            // Decelerate
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, currentDecel * Time.deltaTime);

            // Stop completely if velocity is negligible
            if (horizontalVelocity.magnitude < 0.01f)
                horizontalVelocity = Vector3.zero;
        }

        return horizontalVelocity;
    }

    private float GetVerticalVelocity()
    {
        if (_jumping)
        {
            _jumping = false;
            return Mathf.Sqrt(_jumpHeight * -2 * _gravity);
        }
        else if (IsGrounded)
        {
            return -0.1f;
        }
        else
        {
            return Velocity.y + ((UseGravity ? _gravity : 0) * Time.deltaTime);
        }
    }
}
