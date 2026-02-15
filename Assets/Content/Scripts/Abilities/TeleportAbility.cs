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
        var linearCollider = Physics.CapsuleCast(cameraTransform.position + Vector3.down * _cc.height / 4,
                                                 cameraTransform.position + Vector3.up * _cc.height / 4,
                                                 _cc.radius, forward, out var result, _distance, _layerMask);

        if (linearCollider)
        {
            var pointBefore = cameraTransform.position + forward * (result.distance - 0.05f);
            point = pointBefore - (forward * _cc.height / 2);

            Debug.Log($"Raycast point: {result.point}, result point: {point}");
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


            var distanceDivided = (_effect.transform.position - _movement.CameraHolder.transform.position).magnitude / _distance;

            var duration = _maxTeleportDuration * distanceDivided;
            _movement.SetVelocity(_movement.Velocity * Mathf.Min(1, 1.5f - distanceDivided));

            var move = transform.DOLocalMove(_effect.transform.position, duration);
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
}
