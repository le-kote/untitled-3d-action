using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Game Event")]
public class GameEvent : ScriptableObject
{
    private readonly System.Collections.Generic.List<GameEventListener> listeners =
        new System.Collections.Generic.List<GameEventListener>();

    public Type Type;

    public void Raise(Component sender, object eventData)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised(sender, eventData);
    }

    public void RegisterListener(GameEventListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
}

[System.Serializable]
public class CustomGameEvent : UnityEvent<Component, object>
{
}
