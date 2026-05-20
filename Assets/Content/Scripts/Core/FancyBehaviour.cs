using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public abstract class FancyBehaviour : MonoBehaviour
{
    [Inject]
    private IEventSystem _eventSys;

    [Inject]
    private IObjectPool _pool;

    private struct SubscribedEvent
    {
        public System.Delegate Callback;
        public System.Type EventType;
    }

    private List<SubscribedEvent> _subscribedEvents = new List<SubscribedEvent>();

    void OnEnable()
    {
        InitializeEvents();
        OnScriptEnabled();
    }

    void OnDisable()
    {
        foreach (var sub in _subscribedEvents)
        {
            UnsubscribeEvent(sub.Callback, sub.EventType);
            UnsubscribeLocalEvent(sub.Callback, sub.EventType);
        }

        _subscribedEvents.Clear();

        OnScriptDisabled();
    }

    protected virtual void OnScriptEnabled()
    {
    }

    protected virtual void OnScriptDisabled()
    {
    }

    #region Events
    protected virtual void InitializeEvents()
    {
    }

    protected void RaiseEvent<T>(ref T eventData) where T : struct
    {
        _eventSys.RaiseEvent(ref eventData);
    }

    protected void RaiseEvent<T>() where T : struct
    {
        var eventData = default(T);
        _eventSys.RaiseEvent(ref eventData);
    }

    protected void RaiseLocalEvent<T>(ref T eventData) where T : struct
    {
        _eventSys.RaiseLocalEvent(gameObject, ref eventData);
    }

    protected void RaiseLocalEvent<T>() where T : struct
    {
        var eventData = default(T);
        _eventSys.RaiseLocalEvent(gameObject, ref eventData);
    }

    protected void SubscribeEvent<T>(EventCallback<T> callback) where T : struct
    {
        _eventSys.Subscribe(callback);
        _subscribedEvents.Add(new SubscribedEvent { Callback = callback, EventType = typeof(T) });
    }

    protected void SubscribeLocalEvent<T>(EventCallback<T> callback) where T : struct
    {
        _eventSys.SubscribeLocal(gameObject, callback);
        _subscribedEvents.Add(new SubscribedEvent { Callback = callback, EventType = typeof(T) });
    }

    private void UnsubscribeEvent(System.Delegate del, System.Type eventType)
    {
        if (del == null || eventType == null)
            return;

        var unsubscribeMethod = _eventSys.GetType().GetMethod("Unsubscribe");
        if (unsubscribeMethod == null)
            return;

        var generic = unsubscribeMethod.MakeGenericMethod(eventType);
        generic.Invoke(_eventSys, new object[] { del });
    }

    private void UnsubscribeLocalEvent(System.Delegate del, System.Type eventType)
    {
        if (del == null || eventType == null)
            return;

        var unsubscribeMethod = _eventSys.GetType().GetMethod("UnsubscribeLocal");

        if (unsubscribeMethod == null)
            return;

        var generic = unsubscribeMethod.MakeGenericMethod(eventType);
        generic.Invoke(_eventSys, new object[] { gameObject, del });
    }
    #endregion

    #region Spawn/Delete
    protected GameObject PoolShow(AssetReference key)
    {
        return _pool.GetInstance(key);
    }

    public void PoolHide(GameObject target)
    {
        _pool.HideObject(target);
    }
    #endregion
}
