using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable enable

public class WeaponsHolder : MonoBehaviour
{
    [SerializeField]
    private BaseWeapon? _weapon1;

    [SerializeField]
    private BaseWeapon? _weapon2;

    [SerializeField]
    private bool _firstWeapon;

    private float _attackTimer = 0f;
    private float _untilAttack = -1f;
    private bool _isAttacking = false;
    private List<float> _queuedAttacks = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_attackTimer > 0f)
            _attackTimer -= Time.deltaTime;

        if (!TryGetCurrentWeapon(out var weapon))
            return;

        if (_attackTimer <= 0f && weapon.Automatic)
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

    private void TryAttack(BaseWeapon weapon)
    {
        switch (weapon)
        {
            case MeleeWeapon melee:
                AttackMelee(melee);
                break;
        }
    }

    private void AttackMelee(MeleeWeapon weapon)
    {
        var extents = new Vector3(weapon.AttackWidth / 2, weapon.AttackHeight / 2, weapon.AttackRange / 2);
        var attackPos = transform.position + transform.forward * weapon.AttackRange / 2;

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

    private bool TryGetCurrentWeapon([NotNullWhen(true)] out BaseWeapon? weapon)
    {
        if (!_firstWeapon && _weapon2 == null)
            _firstWeapon = true;

        weapon = _firstWeapon ? _weapon1 : _weapon2;
        return weapon != null;
    }

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

            return;
        }

        if (_attackTimer > 0f)
            return;

        _queuedAttacks.Add(Time.time + weapon.DelayBeforeAttack);
        _attackTimer = weapon.AttackRate;
    }
}
