using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class GameStarter : FancyBehaviour
{
    [SerializeField] private ObjectPool.PreloadedPrefab[] _preloadedPrefabs;
    [SerializeField] private AssetReference _audioObject;

    [Inject]
    private IObjectPool _pool;

    [Inject]
    private IAudioSystem _audio;

    private async void Awake()
    {
        _audio.Startup(_audioObject);
        await _pool.Startup(_preloadedPrefabs);
    }
}
