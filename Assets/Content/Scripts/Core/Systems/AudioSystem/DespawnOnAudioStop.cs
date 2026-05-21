using UnityEngine;

public class DespawnOnAudioStop : FancyBehaviour
{
    private AudioSource _source;
    private bool _wasPlaying = false;

    void Start()
    {
        _source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!_wasPlaying)
        {
            _wasPlaying = _source.isPlaying;
            return;
        }

        if (_wasPlaying && !_source.isPlaying)
        {
            _wasPlaying = false;
            PoolHide();
        }
    }
}
