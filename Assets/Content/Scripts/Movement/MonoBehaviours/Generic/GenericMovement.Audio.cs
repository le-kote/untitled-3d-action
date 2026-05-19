using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class GenericMovement
{
    [Header("Audio Settings", order = 2)]

    [SerializeField]
    private AudioClip[] _footstepSounds;

    [SerializeField]
    private AudioClip _jumpSound;

    [SerializeField]
    private AudioClip _landSound;

    [SerializeField]
    private AudioClip _bigJumpSelectedSound;

    [SerializeField]
    private AudioClip _smallJumpSelectedSound;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float _footstepVolume = 0.5f;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float _jumpVolume = 0.7f;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float _landVolume = 0.6f;

    [SerializeField]
    [Tooltip("Time interval between footsteps in seconds when walking")]
    private float _walkStepInterval = 0.5f;

    [SerializeField]
    [Tooltip("Time interval between footsteps in seconds when sprinting")]
    private float _sprintStepInterval = 0.3f;

    [SerializeField]
    [Tooltip("Time interval between footsteps in seconds when crouching")]
    private float _crouchStepInterval = 0.8f;

    private AudioSource _audioSource;

    private float _stepTimer = 0f;

    private void InitializeAudio()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.spatialBlend = 1f;
    }

    private void UpdateFootsteps()
    {
        if (!IsGrounded || Input.magnitude < 0.1f)
        {
            _stepTimer = 0f;
            return;
        }

        float currentSpeed = Velocity.Horizontal().magnitude;
        if (currentSpeed < 0.5f)
        {
            _stepTimer = 0f;
            return;
        }

        float interval = CurrentMoveState switch
        {
            MoveState.Running => _sprintStepInterval,
            MoveState.Crouching => _crouchStepInterval,
            _ => _walkStepInterval
        };

        _stepTimer += Time.deltaTime;

        if (_stepTimer >= interval)
        {
            var ev = new BeforeFootstepSoundEvent();
            this.RaiseEvent(ev);
            _stepTimer = 0f;

            if (!ev.Cancelled)
                PlayFootstepSound();
        }
    }

    private void PlayFootstepSound()
    {
        if (_footstepSounds == null || _footstepSounds.Length == 0)
            return;

        AudioClip clip = _footstepSounds[Random.Range(0, _footstepSounds.Length)];
        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.PlayOneShot(clip, _footstepVolume);
    }

    private void PlayJumpSound()
    {
        if (_jumpSound == null || _audioSource == null)
            return;

        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.PlayOneShot(_jumpSound, _jumpVolume);
    }

    private void PlayLandSound()
    {
        if (_landSound == null || _audioSource == null)
            return;

        _audioSource.pitch = Random.Range(0.8f, 1.2f);
        _audioSource.PlayOneShot(_landSound, _landVolume);
    }
}
