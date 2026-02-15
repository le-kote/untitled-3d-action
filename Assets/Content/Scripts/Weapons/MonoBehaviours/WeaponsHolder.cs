using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#nullable enable

[RequireComponent(typeof(GenericMovement))]
public class WeaponsHolder : MonoBehaviour
{
    [SerializeField]
    private BaseWeapon? _weapon1;

    [SerializeField]
    private BaseWeapon? _weapon2;

    [SerializeField]
    private bool _firstWeapon;

    [SerializeField]
    private Collider _collider;

    private GenericMovement _movement;

    private float _attackTimer = 0f;
    private bool _isAttacking = false;
    private List<float> _queuedAttacks = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_attackTimer > 0f)
            _attackTimer -= Time.deltaTime;

        if (!TryGetCurrentWeapon(out var weapon))
            return;

        if (_isAttacking && _attackTimer <= 0f && weapon.Automatic)
        {
            _queuedAttacks.Add(Time.time + weapon.DelayBeforeAttack);
            _attackTimer = weapon.AttackRate;
        }

        for (var i = _queuedAttacks.Count - 1; i >= 0; i--)
        {
            var item = _queuedAttacks[i];

            if (item > Time.time)
                continue;

            _queuedAttacks.RemoveAt(i);
            TryAttack(weapon);
        }
    }

    void FixedUpdate()
    {
    }

    #region Private API
    private void TryAttack(BaseWeapon weapon)
    {
        switch (weapon)
        {
            case MeleeWeapon melee:
                AttackMelee(melee);
                break;
            case ProjectileWeapon projectile:
                AttackProjectile(projectile);
                break;
        }
    }

    private void AttackMelee(MeleeWeapon weapon)
    {
        var camTransform = _movement.CameraHolder.transform;

        var extents = new Vector3(weapon.AttackWidth / 2, weapon.AttackHeight / 2, weapon.AttackRange / 2);
        var attackPos = camTransform.position + camTransform.forward * weapon.AttackRange / 2;

        var cast = Physics.OverlapBox(attackPos, extents, transform.rotation, weapon.Layers);
        cast.OrderBy(x => (x.transform.position - transform.position).magnitude);

        var targetsHit = 0;
        foreach (var item in cast)
        {
            if (targetsHit >= weapon.MaxTargets)
                break;

            if (!item.TryGetComponent<Damageable>(out var damageable))
                continue;

            damageable.TryChangeDamage(weapon.Damage);
            targetsHit++;
        }
    }

    private void AttackProjectile(ProjectileWeapon weapon)
    {
        var camTransform = _movement.CameraHolder.transform;
        var direction = camTransform.forward;

        for (var i = 0; i < weapon.FiredProjectiles; i++)
        {
            var resuldDir = direction;
            var resultPos = camTransform.position;

            if (i > 0)
            {
                Quaternion rotation = camTransform.rotation;
                var x = Random.Range(-weapon.Spread.x, weapon.Spread.x);
                var y = Random.Range(-weapon.Spread.y, weapon.Spread.y);
                rotation.eulerAngles = new(x, y, rotation.eulerAngles.z);

                resuldDir = rotation * direction;
                resultPos += camTransform.right * x / 100 + camTransform.up * y / 100;
            }

            var instance = Instantiate(weapon.Projectile);
            SceneManager.MoveGameObjectToScene(instance, SceneManager.GetActiveScene());
            instance.transform.position = resultPos + camTransform.forward * 0.75f - Vector3.up * 0.12f;

            var proj = instance.GetOrAddComponent<Projectile>();
            proj.Launch(_collider, resuldDir.normalized * weapon.ProjectileSpeed);
            Destroy(instance, weapon.ProjectileLifetime);
        }

    }

    private bool TryGetCurrentWeapon([NotNullWhen(true)] out BaseWeapon? weapon)
    {
        if (!_firstWeapon && _weapon2 == null)
            _firstWeapon = true;

        weapon = _firstWeapon ? _weapon1 : _weapon2;
        return weapon != null;
    }
    #endregion

    #region Visualization

    #endregion

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled)
            return;

        if (!TryGetCurrentWeapon(out var weapon))
            return;

        if (!context.performed)
        {
            if (weapon.StopAttacksOnButtonUp)
                _queuedAttacks.Clear();

            _isAttacking = false;

            return;
        }

        if (_attackTimer > 0f)
            return;

        _queuedAttacks.Add(Time.time + weapon.DelayBeforeAttack);
        _attackTimer = weapon.AttackRate;
        _isAttacking = weapon.Automatic;
    }

    public void OnWeaponSwitch(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled)
            return;

        if (!context.performed)
            return;

        _firstWeapon = !_firstWeapon;

        _isAttacking = false;
        _queuedAttacks.Clear();

        TryGetCurrentWeapon(out _);
    }
}
