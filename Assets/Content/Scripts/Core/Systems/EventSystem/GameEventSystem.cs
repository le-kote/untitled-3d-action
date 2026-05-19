using System.Collections.Generic;
using UnityEngine;

public class GameEventSystem : IEventSystem
{
    private Dictionary<System.Type, List<System.Delegate>> eventSubscribers = new Dictionary<System.Type, List<System.Delegate>>();

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

    public void RaiseEvent<T>(ref T eventData) where T : struct
    {
        System.Type eventType = typeof(T);
        if (!eventSubscribers.ContainsKey(eventType))
            return;

        foreach (System.Delegate callback in eventSubscribers[eventType])
        {
            eventData = ((EventCallback<T>)callback).Invoke(ref eventData);
        }
    }
}
