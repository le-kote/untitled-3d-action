using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ObjectPool : IObjectPool
{
    private Dictionary<string, Queue<GameObject>> _pool { get; set; } = new();
    private Dictionary<GameObject, string> _activePool { get; set; } = new();
    private Transform _root;

    public async UniTask Startup(PreloadedPrefab[] prefabs)
    {
        _root = new GameObject("Pool").transform;
        _root.gameObject.SetActive(false);
        SceneManager.MoveGameObjectToScene(_root.gameObject, SceneManager.GetActiveScene());

        foreach (var item in prefabs)
        {
            _pool.TryAdd(item.Prefab.AssetGUID, new());

            for (var i = 0; i < item.InitialPoolSize; i++)
            {
                var result = await Addressables.InstantiateAsync(item.Prefab, parent: _root.transform).Task;

                result.SetActive(false);
                _pool[item.Prefab.AssetGUID].Enqueue(result);
            }
        }
    }

    public GameObject GetInstance(AssetReference key)
    {
        if (!_pool.TryGetValue(key.AssetGUID, out var queue))
        {
            return Addressables.InstantiateAsync(key, instantiateInWorldSpace: true).WaitForCompletion();
        }
        else if (queue.Count <= 0)
        {
            var result = Addressables.InstantiateAsync(key, parent: _root.transform).WaitForCompletion();
            result.SetActive(false);

            _pool[key.AssetGUID].Enqueue(result);

            Debug.LogWarning("Added an addressable to the pool at runtime!");
        }

        var target = _pool[key.AssetGUID].Dequeue();
        target.transform.SetParent(null);

        return target;
    }

    public void HideObject(GameObject target)
    {
        if (_activePool.TryGetValue(target, out var guid))
        {
            _activePool.Remove(target);
            _pool[guid].Enqueue(target);
        }
        else
        {
            GameObject.Destroy(target);
            Debug.Log("Object was not in pool, destroying instead.");
        }
    }

    [System.Serializable]
    public sealed class PreloadedPrefab
    {
        public AssetReference Prefab;
        public int InitialPoolSize = 20;
    }
}
