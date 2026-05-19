using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component handles the most common movement, such as walking, running, crouching and jumping
/// </summary>
[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public partial class GenericMovement : FancyBehaviour
{
    private CharacterController _cc;

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

        InitializeAudio();
    }

    // Update is called once per frame
    void Update()
    {
        if (!MovementEnabled)
            return;

        UpdateGrounded();
        UpdateFootsteps();
        UpdateHeight();
        UpdateMovement();
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        _groundNormal = hit.normal;
    }

    #region Public API
    public void SetMoveState(MoveState moveState)
    {
        if (CurrentMoveState == moveState)
            return;

        var ev = new MoveStateChangedEvent(CurrentMoveState, moveState);
        RaiseLocalEvent(ref ev);

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

        var canJump = IsGrounded || _coyoteTimer <= _coyoteTime;

        var ev = new CanJumpEvent(IsGrounded, canJump);
        RaiseLocalEvent(ref ev);

        if (ev.Handled ? ev.CanJump : canJump)
        {
            RaiseLocalEvent<JumpEvent>();
            _jumping = true;
            PlayJumpSound();
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

    public void OnToggleJumpHeight(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        _bigJumpSelected = !_bigJumpSelected;

        if (_audioSource == null || _bigJumpSelectedSound == null || _smallJumpSelectedSound == null)
            return;

        if (_bigJumpSelected)
            _audioSource.PlayOneShot(_bigJumpSelectedSound, _jumpVolume);
        else
            _audioSource.PlayOneShot(_smallJumpSelectedSound, _jumpVolume);
    }

    public void ReleaseCursor(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _cameraMove = Vector2.zero;
    }
    #endregion
}

public enum MoveState
{
    Walking,
    Running,
    Crouching
}
