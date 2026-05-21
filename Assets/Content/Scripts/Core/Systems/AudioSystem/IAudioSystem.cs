using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public interface IAudioSystem
{
    public void Startup(AssetReference audioObject);
    public void PlayPosition(IAudioGenerator clip, Vector3 position, AudioParams audioParams);
    public void PlayFollowed(IAudioGenerator clip, Transform target, AudioParams audioParams);
    public void ApplyParams(AudioSource source, AudioParams audioParams);
}
