using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component handles the most common movement, such as walking, running, crouching and jumping
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class GenericMovement : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField]
    private float _moveSpeed = 7;

    [SerializeField]
    private float _sprintSpeed = 10;

    [SerializeField]
    private float _crouchSpeed = 4;

    [SerializeField]
    private float _jumpHeight = 2;

    [SerializeField]
    private float _gravity = -9.8f;

    [SerializeField]
    public bool UseGravity = true;

    [SerializeField]
    private float _slopeFriction = 0.3f;

    [Header("Crouching")]

    [SerializeField]
    private float _crouchHeight = 0.4f;

    [SerializeField]
    private float _height = 2f;

    [SerializeField]
    private float _heightChangeSpeed = 25f;

    [Header("Acceleration")]

    [SerializeField]
    private float _acceleration = 15f;

    [SerializeField]
    private float _airAcceleration = 8f;

    [SerializeField]
    private float _deceleration = 12f;

    [SerializeField]
    private float _airDeceleration = 0f;

    [SerializeField]
    private float _sprintAcceleration = 20f;

    [SerializeField]
    private float _sprintDeceleration = 15f;

    [Header("Camera")]

    [SerializeField]
    private float _cameraSpeed = 5;

    [SerializeField]
    private float _cameraYLimits = 85f;

    [SerializeField]
    private float _walkFov = 60;

    [SerializeField]
    private float _sprintFov = 70;

    [SerializeField]
    private Camera _camera;

    public GameObject CameraHolder;

    private CharacterController _cc;

    private Quaternion _cameraRot = new();
    private Vector2 _cameraMove = Vector2.zero;
    private float _verticalRotation = 0f;
    private bool _jumping = false;
    private Vector3 _groundNormal = Vector3.up;

    /// <summary>
    /// Should player moving or not
    /// </summary>
    public bool MovementEnabled = true;

    /// <summary>
    /// Current input
    /// </summary>
    public Vector2 Input { get; private set; } = Vector2.zero;

    /// <summary>
    /// Current velocity relative to world
    /// </summary>
    public Vector3 Velocity { get; private set; } = Vector3.zero;

    public MoveState CurrentMoveState { get; private set; } = MoveState.Walking;
    public bool IsGrounded { get; private set; } = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cc = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCamera();
        UpdateGrounded();

        if (!MovementEnabled)
            return;

        UpdateHeight();
        UpdateMovement();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        _groundNormal = hit.normal;
    }

    private void UpdateCamera()
    {
        if (_cameraMove.sqrMagnitude > 0.01f)
        {
            transform.Rotate(0, _cameraMove.x * Time.deltaTime * _cameraSpeed, 0);

            _verticalRotation += -_cameraMove.y * Time.deltaTime * _cameraSpeed;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -_cameraYLimits, _cameraYLimits);

            _cameraRot = Quaternion.Euler(_verticalRotation, 0, 0);
            CameraHolder.transform.localRotation = _cameraRot;
        }
    }

    private void UpdateGrounded()
    {
        IsGrounded = _cc.isGrounded && Vector3.Angle(_groundNormal, Vector3.up) <= _cc.slopeLimit;
    }

    private void UpdateMovement()
    {
        if (Input.y <= 0 && CurrentMoveState == MoveState.Running && IsGrounded)
            SetMoveState(MoveState.Walking);

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

    #region Private API
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
        Vector3 horizontalVelocity = new Vector3(Velocity.x, 0, Velocity.z);

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
    #endregion

    #region Public API
    public void SetMoveState(MoveState moveState)
    {
        if (CurrentMoveState == moveState)
            return;

        var ev = new MoveStateChangedEvent(CurrentMoveState, moveState);
        this.RaiseEvent(ev);

        CurrentMoveState = moveState;
        var fov = CurrentMoveState switch
        {
            MoveState.Running => _sprintFov,
            _ => _walkFov
        };

        _camera.DOFieldOfView(fov, 0.25f);
    }

    public void SetVelocity(Vector3 velocity)
    {
        Velocity = velocity;
    }

    public void SetHorizontalVelocity(Vector3 velocity)
    {
        Velocity = new Vector3(velocity.x, Velocity.y, velocity.z);
    }


    public void SetVerticalVelocity(float velocity)
    {
        Velocity = new Vector3(Velocity.x, velocity, Velocity.z);
    }

    public void SetRelativeHorizontalVelocity(Vector2 velocity)
    {
        Velocity = transform.forward * velocity.y + new Vector3(0, Velocity.y, 0) + transform.right * velocity.x;
    }

    public void ApplyForce(Vector3 force)
    {
        Velocity += force;
    }
    #endregion

    #region Input handlers
    public void OnMove(InputAction.CallbackContext context)
    {
        Input = context.ReadValue<Vector2>().normalized;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        var ev = new CanJumpEvent();
        this.RaiseEvent(ev);

        var canJump = ev.Handled ? ev.CanJump : IsGrounded;

        if (canJump)
        {
            this.RaiseEvent(new JumpEvent());
            _jumping = true;
        }
    }

    public void OnCameraMove(InputAction.CallbackContext context)
    {
        _cameraMove = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (!context.performed || Input.y <= 0)
            return;

        SetMoveState(MoveState.Running);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        var newState = CurrentMoveState == MoveState.Crouching ? MoveState.Walking : MoveState.Crouching;

        SetMoveState(newState);
    }

    public void ReleaseCursor(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion
}

public enum MoveState
{
    Walking,
    Running,
    Crouching
}
