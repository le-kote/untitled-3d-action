using UnityEngine;

public interface IEventSubscribedComponent
{
    void ReceiveMessage(GameEventArgs args);
}

public static class EventsExtensions
{
    public static void RaiseEvent(this MonoBehaviour mono, GameEventArgs args)
    {
        var components = mono.GetComponents<IEventSubscribedComponent>();

        foreach (var item in components)
        {
            if ((item as MonoBehaviour).isActiveAndEnabled)
                item.ReceiveMessage(args);
        }
    }
}

public abstract class GameEventArgs
{
}

public abstract class HandledGameEventArgs : GameEventArgs
{
    public bool Handled = false;
}
