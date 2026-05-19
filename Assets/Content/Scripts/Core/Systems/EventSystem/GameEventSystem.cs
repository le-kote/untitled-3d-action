using System.Collections.Generic;
using UnityEngine;

public class GameEventSystem : IEventSystem
{
    private class LocalSubscriber
    {
        public GameObject Target;
        public System.Delegate Callback;
    }

    private Dictionary<System.Type, List<LocalSubscriber>> localEventSubscribers =
        new Dictionary<System.Type, List<LocalSubscriber>>();

    private Dictionary<System.Type, List<System.Delegate>> eventSubscribers =
        new Dictionary<System.Type, List<System.Delegate>>();

    public void Subscribe<T>(EventCallback<T> callback) where T : struct
    {
        System.Type eventType = typeof(T);
        if (!eventSubscribers.ContainsKey(eventType))
        {
            eventSubscribers[eventType] = new List<System.Delegate>();
        }
        eventSubscribers[eventType].Add(callback);
    }

    public void Unsubscribe<T>(EventCallback<T> callback) where T : struct
    {
        System.Type eventType = typeof(T);
        if (eventSubscribers.ContainsKey(eventType))
        {
            eventSubscribers[eventType].Remove(callback);
        }
    }

    public void SubscribeLocal<T>(GameObject target, EventCallback<T> callback) where T : struct
    {
        var eventType = typeof(T);

        if (!localEventSubscribers.ContainsKey(eventType))
            localEventSubscribers[eventType] = new List<LocalSubscriber>();

        localEventSubscribers[eventType].Add(new LocalSubscriber
        {
            Target = target,
            Callback = callback
        });
    }

    public void UnsubscribeLocal<T>(GameObject target, EventCallback<T> callback) where T : struct
    {
        var eventType = typeof(T);
        if (!localEventSubscribers.TryGetValue(eventType, out var list))
            return;

        list.RemoveAll(x => x.Target == target && x.Callback == (System.Delegate)callback);
    }

    public void RaiseEvent<T>(ref T eventData) where T : struct
    {
        System.Type eventType = typeof(T);
        if (!eventSubscribers.ContainsKey(eventType))
            return;

        foreach (System.Delegate del in eventSubscribers[eventType])
        {
            if (del is not EventCallback<T> callback)
                continue;

            ((EventCallback<T>)callback).Invoke(ref eventData);
        }
    }

    public void RaiseLocalEvent<T>(GameObject target, ref T eventData) where T : struct
    {
        var eventType = typeof(T);
        if (!localEventSubscribers.TryGetValue(eventType, out var list))
            return;

        foreach (var sub in list)
        {
            if (sub.Target != target)
                continue;

            if (sub.Callback is EventCallback<T> callback)
                callback.Invoke(ref eventData);
        }
    }
}
