using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public sealed class AudioParams
{
    public AudioMixerGroup Output;
    public bool BypassEffects = false;
    public bool BypassListenerEffects = false;
    [Range(0, 256)] public int Priority = 128;
    [Range(0f, 1f)] public float Volume = 1f;
    [Range(-3f, 3f)] public float Pitch = 1f;
    [Range(0f, 1f)] public float SpatialBlend = 0f;
    [Range(0f, 1.1f)] public float ReverbZoneMix = 1f;
}
