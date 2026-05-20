using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IObjectPool
{
    public async UniTask Startup(ObjectPool.PreloadedPrefab[] prefabs) {}
    public GameObject GetInstance(AssetReference key);
    public void HideObject(GameObject target);
}
