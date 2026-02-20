using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenericMovement), typeof(CharacterController))]
public class TeleportAbility : MonoBehaviour
{
    [SerializeField]
    private float _distance = 12f;

    [SerializeField]
    private float _lerpSpeed = 2f;

    [SerializeField]
    private GameObject _effectInstance;

    [SerializeField]
    private LayerMask _layerMask;

    [SerializeField]
    private float _maxTeleportDuration = 0.5f;

    [Header("Teleport Settings")]
    [SerializeField]
    private bool _requireGrounded = true;

    [SerializeField]
    private bool _requireSurfaceComponent = true;
    
    [SerializeField]
    private float _surfaceOffset = 1.5f;

    [Header("Collision Detection")]
    [SerializeField]
    private LayerMask _obstacleMask = -1;
    
    [SerializeField]
    private float _obstacleCheckRadius = 0.5f;

    private GenericMovement _movement;
    private CharacterController _cc;

    private bool _isTeleporting = false;
    private GameObject _effect;
    private TeleportSurface _currentTeleportSurface;
    private Vector3 _targetPosition;
    private Vector3 _surfaceNormal;

    void Start()
    {
        _movement = GetComponent<GenericMovement>();
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        ClampVelocity();
        UpdateEffect();
    }

    private void ClampVelocity()
    {
        if (!_isTeleporting)
            return;

        if (_movement.IsGrounded)
            return;

        if (_movement.Velocity.y >= 0)
            return;

        var velocity = Vector3.Lerp(_movement.Velocity, Vector3.zero, _lerpSpeed * Time.deltaTime);
        if (velocity.magnitude < 0.01f)
            velocity = Vector3.zero;

        _movement.SetVelocity(velocity);
    }

    private void UpdateEffect()
    {
        if (!_isTeleporting)
            return;

        var cameraTransform = _movement.CameraHolder.transform;
        var forward = cameraTransform.forward.normalized;
        var startPos = cameraTransform.position;

        _currentTeleportSurface = null;
        _targetPosition = startPos + (forward * _distance);

        if (Physics.CapsuleCast(startPos + Vector3.down * _cc.height / 4,
                               startPos + Vector3.up * _cc.height / 4,
                               _cc.radius, forward, out RaycastHit hit, _distance, _layerMask))
        {
            TeleportSurface surface = hit.collider.GetComponent<TeleportSurface>();
            
            if (surface != null && surface.CanTeleport())
            {
                _surfaceNormal = hit.normal;
                
                Vector3 teleportPos = hit.point + (_surfaceNormal * _surfaceOffset);
                
                teleportPos += Vector3.up * (_cc.height / 2);
                
                if (IsPositionClear(teleportPos))
                {
                    _targetPosition = teleportPos;
                    _currentTeleportSurface = surface;
                }
            }
        }

        _effect.transform.position = _targetPosition;
    }

    private bool IsPositionClear(Vector3 position)
    {
        return !Physics.CheckSphere(position + Vector3.up * (_cc.height / 2 - 0.1f), 
                                   _obstacleCheckRadius, 
                                   _obstacleMask);
    }

    private bool IsTeleportPathClear(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        
        return !Physics.CapsuleCast(start + Vector3.down * _cc.height / 4,
                                   start + Vector3.up * _cc.height / 4,
                                   _cc.radius, direction.normalized, 
                                   distance, _obstacleMask);
    }

    public void DoTeleport(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled)
            return;

        if (context.performed)
        {
            if (_requireGrounded && !_movement.IsGrounded)
            {
                return;
            }

            _isTeleporting = true;
            _effect = Instantiate(_effectInstance);
            SceneManager.MoveGameObjectToScene(_effect, SceneManager.GetActiveScene());
        }
        else if (context.canceled)
        {
            if (!_isTeleporting)
                return;

            _isTeleporting = false;

            if (_requireSurfaceComponent && _currentTeleportSurface == null)
            {
                Destroy(_effect);
                return;
            }

            Vector3 startPos = transform.position;
            
            if (!IsTeleportPathClear(startPos, _targetPosition))
            {
                Destroy(_effect);
                return;
            }

            if (!IsPositionClear(_targetPosition))
            {
                Destroy(_effect);
                return;
            }

            _movement.MovementEnabled = false;

            float distanceDivided = (_targetPosition - _movement.CameraHolder.transform.position).magnitude / _distance;
            float duration = _maxTeleportDuration * distanceDivided;
            
            _movement.SetVelocity(_movement.Velocity * Mathf.Min(1, 1.5f - distanceDivided));

            transform.DOMove(_targetPosition, duration).onComplete += () => 
            {
                _movement.MovementEnabled = true;
                
                if (_currentTeleportSurface != null)
                {
                    _currentTeleportSurface.OnTeleportArrival(gameObject);
                }
            };

            Destroy(_effect);
        }
    }

    public void CancelTeleport()
    {
        if (!isActiveAndEnabled)
            return;

        if (!_isTeleporting)
            return;

        _isTeleporting = false;
        _currentTeleportSurface = null;
        Destroy(_effect);
    }
}