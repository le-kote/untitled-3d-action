using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public sealed class AudioCompound
{
    [SerializeField]
    private IAudioGenerator.Serializable _generator;

    public IAudioGenerator Generator => _generator.definition;
    public AudioParams Params = new();
}
