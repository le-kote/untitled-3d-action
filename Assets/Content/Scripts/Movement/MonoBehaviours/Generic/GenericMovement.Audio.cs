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

    private AudioSource _footstepAudioSource;

    private AudioSource _jumpAudioSource;

    private AudioSource _landAudioSource;

    private float _stepTimer = 0f;

    private void InitializeAudio()
    {
        _footstepAudioSource = gameObject.AddComponent<AudioSource>();
        _jumpAudioSource = gameObject.AddComponent<AudioSource>();
        _landAudioSource = gameObject.AddComponent<AudioSource>();

        _footstepAudioSource.spatialBlend = 1f;
        _footstepAudioSource.volume = _footstepVolume;

        _jumpAudioSource.spatialBlend = 1f;
        _jumpAudioSource.volume = _jumpVolume;

        _landAudioSource.spatialBlend = 1f;
        _landAudioSource.volume = _landVolume;
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
        _footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);
        _footstepAudioSource.PlayOneShot(clip, _footstepVolume);
    }

    private void PlayJumpSound()
    {
        if (_jumpSound == null || _jumpAudioSource == null)
            return;

        _jumpAudioSource.pitch = Random.Range(0.9f, 1.1f);
        _jumpAudioSource.PlayOneShot(_jumpSound, _jumpVolume);
    }

    private void PlayLandSound()
    {
        if (_landSound == null || _landAudioSource == null)
            return;

        _landAudioSource.pitch = Random.Range(0.8f, 1.2f);
        _landAudioSource.PlayOneShot(_landSound, _landVolume);
    }
}
