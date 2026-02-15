using UnityEngine;

[RequireComponent(typeof(GenericMovement))]
public class Sliding : MonoBehaviour, IEventSubscribedComponent
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

    public void ReceiveMessage(GameEventArgs args)
    {
        if (args is MoveStateChangedEvent ev)
            OnMoveStateChanged(ev);

        if (args is GetMoveDirectionOverrideEvent dirEv)
            OnMoveDirOverride(dirEv);

        if (args is GetMoveAccelerationOverrideEvent accel)
        {
            if (!_isSliding || !_movement.IsGrounded)
                return;

            accel.Acceleration = _slideAcceleration;
            accel.Deceleration = _slideDeceleration;
            accel.Handled = true;
        }

        if (args is GetMoveSpeedOverrideEvent speedEv)
        {
            if (!_isSliding)
                return;

            speedEv.Speed = _slideSpeed;
            speedEv.Handled = true;
        }

        if (args is JumpEvent)
        {
            if (!_isSliding)
                return;

            _movement.ApplyForce(_movement.Velocity.normalized * _slideJumpSpeed);
            _movement.SetMoveState(MoveState.Running);
            StopSlide();
        }
    }

    private void OnMoveStateChanged(MoveStateChangedEvent ev)
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

    private void OnMoveDirOverride(GetMoveDirectionOverrideEvent ev)
    {
        if (!_isSliding)
            return;

        ev.Handled = true;
        ev.Dir = _direction;
    }
}
