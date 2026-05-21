using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DG.Tweening;
using UnityEngine;
using Zenject;

#nullable enable

/// <summary>
/// This component allows user to wallrun
/// </summary>
[RequireComponent(typeof(GenericMovement), typeof(CharacterController))]
public class WallRunner : FancyBehaviour
{
    [Header("Wallrunning")]

    [SerializeField]
    private float _verticalVelocityDeceleration = 4;

    [SerializeField]
    private float _rotationLerpAmount = 4;

    [SerializeField]
    private float _wallrunAcceleration = 6;

    [SerializeField]
    private float _wallrunDeceleration = 2;

    [SerializeField]
    private float _sideCameraRotationDuration = 0.15f; // Уменьшил с 0.25f до 0.15f

    [Header("Jumping")]

    [SerializeField]
    private float _jumpSpeed = 9f;

    [Header("Limits")]

    [SerializeField]
    private float _maxWallRunTime = 3;

    [SerializeField]
    private float _lastNormalResetDuration = 0.8f; // Уменьшил с 1.2f до 0.8f

    [SerializeField]
    private float _maxWallRunSlope = 60f;

    [SerializeField]
    private bool _requireSprint = true;

    private float _lastNormalResetTimer = 0;

    [Header("Detection")]

    [SerializeField]
    private LayerMask _wallLayer;

    [SerializeField]
    private LayerMask _groundLayer;

    [SerializeField]
    private float _maxWallDistance = 0.7f;

    [SerializeField]
    private float _minJumpHeight = 0.4f;

    [SerializeField]
    private List<Vector3> _leftRayDirections;

    [SerializeField]
    private List<Vector3> _rightRayDirections;

    [Header("Camera")]

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private float _onWallAngle = 30f;

    [Header("Audio Settings")]
    [SerializeField] private AudioCompound _wallrunStartSounds;
    [SerializeField] private AudioCompound _wallrunEndSounds;
    [SerializeField] private AudioCompound _wallJumpSounds;
    [SerializeField] private AudioCompound _wallrunFootstepSounds;
    [SerializeField] private float _footstepInterval = 0.3f;

    private RaycastHit _leftWallRay;
    private RaycastHit _rightWallRay;
    private bool _wallLeft;
    private bool _wallRight;

    private GenericMovement _movement;
    private CharacterController _cc;
    private Vector3 _direction;

    private bool _wallRunning;
    private Vector3? _lastNormal = null;
    private bool _jumping;

    [Inject]
    private IAudioSystem _audio = default!;

    private float _footstepTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();
        _cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckWalls();
        CheckNormalReset();
        SwitchStates();
        HandleMovement();
        UpdateFootsteps();
    }

    void FixedUpdate()
    {
        if (_lastNormal != null)
            _lastNormalResetTimer += Time.fixedDeltaTime;
    }

    protected override void InitializeEvents()
    {
        base.InitializeEvents();

        SubscribeLocalEvent<GetMoveAccelerationOverrideEvent>(OnAccelerationOverride);
        SubscribeLocalEvent<GetMoveDirectionOverrideEvent>(OnMoveDirectionOverride);
        SubscribeLocalEvent<JumpEvent>(OnJump);
        SubscribeLocalEvent<CanJumpEvent>(OnCanJump);
    }

    private void CheckWalls()
    {
        foreach (var item in _leftRayDirections)
        {
            var vec = (transform.forward * item.z + transform.right * item.x).normalized;
            if (Physics.Raycast(transform.position, vec, out _leftWallRay, _maxWallDistance, _wallLayer))
            {
                _wallLeft = true;
                _wallRight = false;
                return;
            }
        }
        foreach (var item in _rightRayDirections)
        {
            var vec = (transform.forward * item.z + transform.right * item.x).normalized;
            if (Physics.Raycast(transform.position, vec, out _rightWallRay, _maxWallDistance, _wallLayer))
            {
                _wallRight = true;
                _wallLeft = false;
                return;
            }
        }

        _wallRight = false;
        _wallLeft = false;
    }

    #region State switching
    private void SwitchStates()
    {

        if (CanWallRun())
        {
            if (!_wallRunning)
                StartWallRunning();
        }
        else
        {
            if (_wallRunning)
                StopWallRunning();
        }
    }

    private void StartWallRunning()
    {
        _wallRunning = true;
        _movement.UseGravity = false;

        LerpCameraRotation();

        // Play random start sound
        _audio.PlayFollowed(_wallrunStartSounds.Generator, transform, _wallrunStartSounds.Params);

        var ev = new RefreshAirJumpsEvent();
        RaiseLocalEvent(ref ev);
    }

    private void StopWallRunning()
    {
        _wallRunning = false;
        _movement.UseGravity = true;

        LerpCameraRotation();

        // Play random end sound
        _audio.PlayFollowed(_wallrunEndSounds.Generator, transform, _wallrunEndSounds.Params);
    }
    #endregion

    #region Audio
    private void UpdateFootsteps()
    {
        if (!_wallRunning || _movement.Velocity.magnitude < 0.5f)
        {
            _footstepTimer = 0f;
            return;
        }

        _footstepTimer += Time.deltaTime;

        if (_footstepTimer >= _footstepInterval && _wallrunFootstepSounds.Generator != null)
        {
            _footstepTimer = -Random.Range(0f, 0.05f);
            _audio.PlayFollowed(_wallrunFootstepSounds.Generator, transform, _wallrunFootstepSounds.Params);
        }
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        if (!_wallRunning)
            return;

        if (_jumping)
            HandleWallJumping();
        else
            HandleWallRunning();
    }

    private void CheckNormalReset()
    {
        if (_movement.IsGrounded || _lastNormalResetTimer >= _lastNormalResetDuration)
        {
            _lastNormal = null;
            _lastNormalResetTimer = 0f;
        }
    }

    private void HandleWallJumping()
    {
        Vector3 wallNormal = _wallRight ? _rightWallRay.normal : _leftWallRay.normal;

        _jumping = false;
        _movement.ApplyForce(wallNormal * _jumpSpeed);
        _movement.ApplyForce(transform.forward * (_jumpSpeed / 2));

        // Play random jump sound
        _audio.PlayFollowed(_wallJumpSounds.Generator, transform, _wallJumpSounds.Params);

        var ev = new RefreshAirJumpsEvent();
        RaiseLocalEvent(ref ev);

        _lastNormal = _wallRight ? _rightWallRay.normal : _leftWallRay.normal;
    }

    private void HandleWallRunning()
    {
        Vector3 wallNormal = _wallRight ? _rightWallRay.normal : _leftWallRay.normal;

        // Choose a direction along the wall surface and align it with player input
        var input = _movement.Input;
        input.y = Mathf.Max(0f, input.y);

        var desiredMove = transform.forward * input.y + transform.right * input.x;
        if (desiredMove.sqrMagnitude < 0.0001f)
            desiredMove = transform.forward;

        var dirAlongWall = Vector3.Cross(wallNormal, Vector3.up);
        dirAlongWall.y = 0f;
        if (dirAlongWall.sqrMagnitude < 0.0001f)
            return;

        dirAlongWall.Normalize();
        if (Vector3.Dot(dirAlongWall, desiredMove) < 0f)
            dirAlongWall = -dirAlongWall;

        _direction = dirAlongWall * input.y;

        ClampVerticalVelocity();
        LerpRotation();
    }

    private void ClampVerticalVelocity()
    {
        if (!_wallRunning)
            return;

        var velocity = Mathf.Lerp(_movement.Velocity.y, 0f, _verticalVelocityDeceleration * Time.deltaTime);
        _movement.SetVerticalVelocity(velocity);
    }

    private void LerpRotation()
    {
        if (_direction.sqrMagnitude < 0.0001f)
            return;

        var flatDir = _direction;
        flatDir.y = 0f;
        if (flatDir.sqrMagnitude < 0.0001f)
            return;

        var targetRotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationLerpAmount * Time.deltaTime);
    }

    private void LerpCameraRotation()
    {
        var targetAngle = _wallRunning ? (_wallRight ? _onWallAngle : -_onWallAngle) : 0f;

        _camera.transform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), _sideCameraRotationDuration);
    }
    #endregion

    #region API
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position + (Vector3.zero * (_cc.height / 2)), Vector3.down, _minJumpHeight, _groundLayer) && !_movement.IsGrounded;
    }

    private bool CanWallRun()
    {
        var velocity = _movement.Velocity.Horizontal();

        if (!AboveGround())
            return false;

        if (!TryGetRay(out var curRay) || (_lastNormal != null && curRay.Value.normal == _lastNormal))
            return false;

        if (velocity.magnitude < 0.01f && !_wallRunning)
            return false;

        if (_movement.CurrentMoveState != MoveState.Running && _requireSprint)
            return false;

        if (!IsWallSlopeValid(curRay.Value.normal))
            return false;

        return true;
    }

    private bool IsWallSlopeValid(Vector3 wallNormal)
    {
        float slopeAngle = Mathf.Asin(Mathf.Abs(wallNormal.y)) * Mathf.Rad2Deg;
        return slopeAngle <= _maxWallRunSlope;
    }

    private bool TryGetRay([NotNullWhen(true)] out RaycastHit? ray)
    {
        var rayExists = _wallLeft || _wallRight;
        ray = null;
        if (rayExists)
        {
            ray = _wallRight ? _rightWallRay : _leftWallRay;
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region Event handlers
    private void OnAccelerationOverride(ref GetMoveAccelerationOverrideEvent ev)
    {
        if (!_wallRunning)
            return;

        ev.Acceleration = _wallrunAcceleration;
        ev.Deceleration = _wallrunDeceleration;
        ev.Handled = true;
    }

    private void OnMoveDirectionOverride(ref GetMoveDirectionOverrideEvent ev)
    {
        if (!_wallRunning)
            return;

        ev.Dir = _direction;
        ev.Handled = true;
    }

    private void OnJump(ref JumpEvent ev)
    {
        if (!_wallRunning)
            return;

        _jumping = true;
    }

    private void OnCanJump(ref CanJumpEvent ev)
    {
        if (!_wallRunning)
            return;

        ev.CanJump = true;
        ev.Handled = true;
    }

    #endregion
}
