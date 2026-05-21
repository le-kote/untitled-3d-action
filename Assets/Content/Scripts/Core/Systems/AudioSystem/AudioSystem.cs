using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using Zenject;

public class AudioSystem : IAudioSystem
{
    private const string AudioObjectAddress = "Core/AudioPseusoObject";
    private AssetReference _audioObject;

    [Inject]
    private IObjectPool _pool;

    public void Startup(AssetReference audioObject)
    {
        _audioObject = audioObject;
    }

    public void PlayPosition(IAudioGenerator clip, Vector3 position, AudioParams audioParams)
    {
        if (_audioObject == null)
        {
            Debug.LogError("AudioSystem: audio object reference not set. Ensure GameStarter calls Startup in Awake.");
            return;
        }

        var sourceObject = _pool.GetInstance(_audioObject);

        if (!sourceObject.TryGetComponent<AudioSource>(out var source))
        {
            Debug.LogError("Audio object instance does not have audio source component!");
            source = sourceObject.AddComponent<AudioSource>();
        }

        source.generator = clip;
        ApplyParams(source, audioParams);

        source.transform.position = position;
        source.Play();
    }

    public void PlayFollowed(IAudioGenerator clip, Transform target, AudioParams audioParams)
    {
        if (_audioObject == null)
        {
            Debug.LogError("AudioSystem: audio object reference not set. Ensure GameStarter calls Startup in Awake.");
            return;
        }

        var sourceObject = _pool.GetInstance(_audioObject);

        if (!sourceObject.TryGetComponent<AudioSource>(out var source))
        {
            Debug.LogError("Audio object instance does not have audio source component!");
            source = sourceObject.AddComponent<AudioSource>();
        }

        source.generator = clip;
        ApplyParams(source, audioParams);

        source.transform.SetParent(target);
        source.transform.localPosition = Vector3.zero;
        source.Play();
    }

    public void ApplyParams(AudioSource source, AudioParams audioParams)
    {
        source.outputAudioMixerGroup = audioParams.Output;
        source.bypassEffects = audioParams.BypassEffects;
        source.bypassListenerEffects = audioParams.BypassListenerEffects;
        source.priority = audioParams.Priority;
        source.volume = audioParams.Volume;
        source.pitch = audioParams.Pitch;
        source.spatialBlend = audioParams.SpatialBlend;
        source.reverbZoneMix = audioParams.ReverbZoneMix;
    }
}
