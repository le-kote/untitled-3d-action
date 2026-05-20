using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameStarter : FancyBehaviour
{
    [SerializeField] private ObjectPool.PreloadedPrefab[] _preloadedPrefabs;

    [Inject]
    private IObjectPool _pool;

    private async void Start()
    {
        await _pool.Startup(_preloadedPrefabs);
    }
}
