using UnityEngine;

public interface IEventSystem
{
    public void Subscribe<T>(EventCallback<T> callback) where T : struct;
    public void Unsubscribe<T>(EventCallback<T> callback) where T : struct;
    public void RaiseEvent<T>(ref T eventData) where T : struct;
}
