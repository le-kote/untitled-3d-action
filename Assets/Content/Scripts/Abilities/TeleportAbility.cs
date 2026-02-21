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
    private LayerMask _targetMask;

    [SerializeField]
    private LayerMask _obstacleMask;

    private GenericMovement _movement;

    void Start()
    {
        _movement = GetComponent<GenericMovement>();
    }

    public void TryTeleport(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!_movement.MovementEnabled)
            return;

        var camTransform = _movement.CameraHolder.transform;

        if (!Physics.SphereCast(camTransform.position, 1.5f, camTransform.forward, out var hit, _distance, _targetMask))
            return;

        if (Physics.SphereCast(camTransform.position, .35f, hit.transform.position - camTransform.position, out _, hit.distance, _obstacleMask))
            return;

        if (!hit.collider.gameObject.TryGetComponent<TeleportTarget>(out var target))
            return;

        target.DoTeleport(_movement);
    }

}
