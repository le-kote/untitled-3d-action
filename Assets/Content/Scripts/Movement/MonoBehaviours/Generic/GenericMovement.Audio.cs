using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Zenject;

public partial class GenericMovement
{
    [Header("Audio Settings", order = 2)]

    [SerializeField]
    private AudioCompound _footstepSounds;

    [SerializeField]
    private AudioCompound _jumpSound;

    [SerializeField]
    private AudioCompound _landSound;

    [SerializeField]
    private AudioCompound _bigJumpSelectedSound;

    [SerializeField]
    private AudioCompound _smallJumpSelectedSound;

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

    [Inject]
    private IAudioSystem _audio;

    private float _stepTimer = 0f;

    private void UpdateFootsteps()
    {
        float interval = CurrentMoveState switch
        {
            MoveState.Running => _sprintStepInterval,
            MoveState.Crouching => _crouchStepInterval,
            _ => _walkStepInterval
        };

        if (!IsGrounded || Input.magnitude < 0.1f || Velocity.Horizontal().magnitude < 0.5f)
        {
            _stepTimer = interval * .7f;
            return;
        }

        _stepTimer += Time.deltaTime;

        if (_stepTimer >= interval)
        {
            _stepTimer = 0f;
            PlayFootstepSound();
        }
    }

    private void PlayFootstepSound()
    {
        if (_footstepSounds.Generator == null)
            return;

        _audio.PlayFollowed(_footstepSounds.Generator, transform, _footstepSounds.Params);
    }

    private void PlayJumpSound()
    {
        if (_jumpSound.Generator == null)
            return;

        _audio.PlayFollowed(_jumpSound.Generator, transform, _jumpSound.Params);
    }

    private void PlayLandSound()
    {
        if (_landSound.Generator == null)
            return;

        _audio.PlayFollowed(_landSound.Generator, transform, _landSound.Params);
    }
}
