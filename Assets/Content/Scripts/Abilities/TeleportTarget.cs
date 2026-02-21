using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator), typeof(SphereCollider))]
public class TeleportTarget : MonoBehaviour
{
    [SerializeField]
    private float _teleportDuration = 0.2f;

    [SerializeField]
    private float _appliedForce = 0f;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    [SerializeField]
    private float _cooldown = 5f;

    private float _timer = 0f;

    private Animator _animator;
    private SphereCollider _collider;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        if (_timer <= 0)
            return;

        _timer -= Time.deltaTime;
        _animator.SetBool("Cooldown", _timer > 0);
        _collider.enabled = _timer <= 0;
    }

    public void DoTeleport(GenericMovement movement)
    {
        var startingPos = movement.transform.position;
        var targetPos = transform.position + _offset;

        movement.SetVelocity(Vector3.zero);
        _timer = _cooldown;

        if (_teleportDuration > 0)
        {
            movement.MovementEnabled = false;

            var move = movement.transform.DOMove(targetPos, _teleportDuration);
            move.onComplete += () =>
            {
                movement.MovementEnabled = true;
                if (_appliedForce <= 0)
                    return;

                var direction = (targetPos - startingPos).normalized;
                movement.ApplyForce(direction * _appliedForce);
            };
        }
        else
        {
            movement.transform.position = targetPos;
            if (_appliedForce <= 0)
                return;

            var direction = (targetPos - startingPos).normalized;
            movement.ApplyForce(direction * _appliedForce);
        }
    }
}
