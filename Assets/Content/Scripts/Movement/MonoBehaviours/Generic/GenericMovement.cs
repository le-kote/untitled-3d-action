using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component handles the most common movement, such as walking, running, crouching and jumping
/// </summary>
[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public partial class GenericMovement : MonoBehaviour
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

    public ReactiveProperty<MoveState> CurrentMoveState { get; private set; } = new(MoveState.Walking);

    public BoolReactiveProperty IsGrounded { get; private set; } = new(false);

    private CompositeDisposable _disposable = new();

    [Header("Events")]
    [SerializeField] private GameEvent _moveSpeedOverrideEvent;
    [SerializeField] private GameEvent _moveSpeedModifierEvent;
    [SerializeField] private GameEvent _accelOverrideEvent;
    [SerializeField] private GameEvent _moveDirectionOverrideEvent;

    [SerializeField] private GameEvent _moveStateChangedEvent;

    [SerializeField] private GameEvent _jumpAttemptEvent;
    [SerializeField] private GameEvent _jumpEvent;
    [SerializeField] private GameEvent _groundedStateChangedEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cc = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InitializeReactives();
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

    #region Reactives
    private void OnDisable()
    {
        _disposable.Dispose();
    }

    private void InitializeReactives()
    {
        IsGrounded
            .Pairwise()
            .Subscribe(x => OnGroundedState(x.Previous, x.Current))
            .AddTo(_disposable);

        CurrentMoveState
            .Pairwise()
            .Subscribe(x => OnMoveStateSet(x.Previous, x.Current))
            .AddTo(_disposable);
    }
    #endregion

    #region Public API
    public void SetMoveState(MoveState moveState)
    {
        if (CurrentMoveState.Value == moveState)
            return;

        CurrentMoveState.Value = moveState;
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

        var canJump = IsGrounded.Value || _coyoteTimer <= _coyoteTime;

        var jumpData = new CanJumpData { CanJump = canJump, Grounded = IsGrounded.Value };
        _jumpAttemptEvent.Raise(this, jumpData);

        if (jumpData.Handled ? jumpData.CanJump : canJump)
        {
            _jumpEvent.Raise(this, null);
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

        var newState = CurrentMoveState.Value == MoveState.Crouching ? MoveState.Walking : MoveState.Crouching;

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
