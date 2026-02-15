using System.Linq;
using DG.Tweening;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// This ability allows user to teleport on certain distance
/// </summary>
[RequireComponent(typeof(GenericMovement), typeof(CharacterController))]
public class TeleportAbility : MonoBehaviour, IEventSubscribedComponent
{
    [SerializeField]
    private float _distance = 12f;

    [SerializeField]
    private float _lerpSpeed = 2f;

    [SerializeField]
    private GameObject _effectInstance;

    [SerializeField]
    private LayerMask _layerMask;

    private GenericMovement _movement;
    private CharacterController _cc;

    private bool _isTeleporting = false;
    // private Vector3 _teleportPoint = Vector3.zero;
    private GameObject _effect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();
        _cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
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

        Debug.Log($"Velocity - {_movement.Velocity.y}, clamping");
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

        var point = cameraTransform.position + (forward * _distance);
        var linearCollider = Physics.Raycast(cameraTransform.position, forward, out var result, _distance, _layerMask);

        if (linearCollider)
        {
            point = result.point;
            point.x -= transform.forward.x * _cc.radius;
            point.z -= transform.forward.z * _cc.radius;
            Debug.Log($"Raycast point: {result.point}, result point: {point}");
        }

        var colliders = Physics.OverlapCapsule(point,
                                               point + (Vector3.up * _cc.height),
                                               _cc.radius, _layerMask);

        var pointForRays = (linearCollider ? result.point : point) - forward * 0.25f;

        foreach (var item in colliders)
        {
            var vec = item.ClosestPoint(point) - pointForRays;

            if (Physics.Raycast(pointForRays, vec, out var colliderResult, vec.magnitude + 0.25f))
            {
                var useHeight = colliderResult.normal == Vector3.up || colliderResult.normal == Vector3.down;
                point += colliderResult.normal * (useHeight ? _cc.height / 2 : _cc.radius + 0.1f);
            }
        }

        _effect.transform.position = point;
    }

    public void DoTeleport(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled)
            return;

        if (context.performed)
        {
            _isTeleporting = true;
            _effect = Instantiate(_effectInstance);
            SceneManager.MoveGameObjectToScene(_effect, SceneManager.GetActiveScene());
        }
        else
        {
            if (!_isTeleporting)
                return;

            _isTeleporting = false;
            _movement.MovementEnabled = false;

            _movement.SetVelocity(Vector3.zero);

            var move = transform.DOLocalMove(_effect.transform.position, 0.5f);
            move.onComplete += () => _movement.MovementEnabled = true;

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
        Destroy(_effect);
    }

    public void ReceiveMessage(GameEventArgs args)
    {
        if (!_isTeleporting)
            return;

        // if (args is CanJumpEvent canJump)
        // {
        //     canJump.Handled = true;
        //     canJump.CanJump = false;
        // }
    }
}
