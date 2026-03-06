using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#nullable enable

[RequireComponent(typeof(GenericMovement))]
[RequireComponent(typeof(AudioSource))]
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

    [Header("Audio Settings")]
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioClip _switchWeaponSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float _switchWeaponVolume = 0.5f;

    private GenericMovement _movement;

    private float _attackTimer = 0f;
    private bool _isAttacking = false;
    private List<float> _queuedAttacks = new();

    private BaseWeapon? _currentWeapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
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
            DoAttack(weapon);
        }
    }

    void FixedUpdate()
    {
    }

    #region Private API
    private void DoAttack(BaseWeapon weapon)
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

        PlayWeaponSound(weapon);
    }

    private void PlayWeaponSound(BaseWeapon weapon)
    {
        if (_audioSource == null) return;

        AudioClip? soundToPlay = null;
        float volume = 1f;
        bool is3D = true;
        UnityEngine.Audio.AudioMixerGroup? mixerGroup = null;

        switch (weapon)
        {
            case ProjectileWeapon projectile:
                soundToPlay = projectile.FireSound;
                volume = projectile.FireSoundVolume;
                is3D = projectile.IsFireSound3D;
                mixerGroup = projectile.AudioMixerGroup;
                break;
        }

        if (soundToPlay == null) return;

        _audioSource.spatialBlend = is3D ? 1f : 0f;
        _audioSource.volume = volume;

        if (mixerGroup != null)
            _audioSource.outputAudioMixerGroup = mixerGroup;

        _audioSource.PlayOneShot(soundToPlay);
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

            // Modifies spread for every projectile
            if (i > 0 || weapon.ApplySpreadToFirstProjectile)
            {
                var x = Random.Range(-weapon.Spread.x, weapon.Spread.x);
                var y = Random.Range(-weapon.Spread.y, weapon.Spread.y);

                // Apply spread as rotations around the camera's local axes
                // X rotation (pitch) around camera's right axis, Y rotation (yaw) around camera's up axis
                Quaternion spreadRotation = Quaternion.AngleAxis(x, camTransform.right) * Quaternion.AngleAxis(y, camTransform.up);
                resuldDir = spreadRotation * direction;
                resultPos += camTransform.right * x / 100 + camTransform.up * y / 100;
            }

            // Instantiate and move projectile to scene
            var instance = Instantiate(weapon.Projectile);
            SceneManager.MoveGameObjectToScene(instance, SceneManager.GetActiveScene());
            instance.transform.position = resultPos + camTransform.forward * 0.75f - Vector3.up * 0.12f;

            // Launch projectile
            var proj = instance.GetOrAddComponent<Projectile>();
            proj.Launch(gameObject, resuldDir.normalized * weapon.ProjectileSpeed);
            Destroy(instance, weapon.ProjectileLifetime);
        }

        if (weapon.PhysicalRecoil > 0f)
            _movement.ApplyForce(-direction * weapon.PhysicalRecoil);
    }

    /// <summary>
    /// Tries to get currently active weapon scriptable object
    /// </summary>
    /// <param name="weapon"></param>
    /// <returns>True if weapon exsists</returns>
    private bool TryGetCurrentWeapon([NotNullWhen(true)] out BaseWeapon? weapon)
    {
        if (!_firstWeapon && _weapon2 == null)
            _firstWeapon = true;

        weapon = _firstWeapon ? _weapon1 : _weapon2;

        if (weapon != _currentWeapon)
        {
            _currentWeapon = weapon;
            PlaySwitchWeaponSound();
        }

        return weapon != null;
    }

    private void PlaySwitchWeaponSound()
    {
        if (_audioSource != null && _switchWeaponSound != null)
        {
            _audioSource.PlayOneShot(_switchWeaponSound, _switchWeaponVolume);
        }
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
            // For delayed melee weapons
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
