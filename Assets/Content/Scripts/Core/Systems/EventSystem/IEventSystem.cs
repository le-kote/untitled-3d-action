using UnityEngine;

public interface IEventSystem
{
    public void Subscribe<T>(EventCallback<T> callback) where T : struct;
    public void Unsubscribe<T>(EventCallback<T> callback) where T : struct;
    public void SubscribeLocal<T>(GameObject target, EventCallback<T> callback) where T : struct;
    public void UnsubscribeLocal<T>(GameObject target, EventCallback<T> callback) where T : struct;

    public void RaiseEvent<T>(ref T eventData) where T : struct;
    public void RaiseLocalEvent<T>(GameObject target, ref T eventData) where T : struct;
}
