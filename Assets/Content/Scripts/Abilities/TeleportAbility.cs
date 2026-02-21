using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenericMovement), typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class TeleportAbility : MonoBehaviour
{
    [SerializeField]
    private float _distance = 12f;

    [SerializeField]
    private LayerMask _targetMask;

    [SerializeField]
    private LayerMask _obstacleMask;

    [Header("Audio Settings")]
    [SerializeField]
    private AudioSource _audioSource;
    
    [SerializeField]
    private AudioClip[] _teleportSounds;
    
    [SerializeField]
    [Range(0f, 1f)]
    private float _teleportVolume = 0.8f;
    
    [SerializeField]
    private bool _randomizePitch = true;
    
    [SerializeField]
    [Range(0.5f, 2f)]
    private float _minPitch = 0.8f;
    
    [SerializeField]
    [Range(0.5f, 2f)]
    private float _maxPitch = 1.2f;

    private GenericMovement _movement;

    void Start()
    {
        _movement = GetComponent<GenericMovement>();
        
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
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

        PlayTeleportSound();
        target.DoTeleport(_movement);
    }

    private void PlayTeleportSound()
    {
        if (_audioSource == null || _teleportSounds == null || _teleportSounds.Length == 0)
            return;
        
        // Выбираем случайный звук из массива
        int randomIndex = Random.Range(0, _teleportSounds.Length);
        AudioClip clipToPlay = _teleportSounds[randomIndex];
        
        // Рандомизируем высоту тона (pitch) если включено
        if (_randomizePitch)
        {
            _audioSource.pitch = Random.Range(_minPitch, _maxPitch);
        }
        else
        {
            _audioSource.pitch = 1f;
        }
        
        // Воспроизводим звук
        _audioSource.PlayOneShot(clipToPlay, _teleportVolume);
    }
}