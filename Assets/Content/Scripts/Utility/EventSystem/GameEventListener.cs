using UnityEngine;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent _gameEvent;
    [SerializeField] private CustomGameEvent _onEventRaised;

    private void OnEnable()
    {
        _gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        _gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised(Component sender, object eventData = null)
    {
        _onEventRaised?.Invoke(sender, eventData);
    }
}
