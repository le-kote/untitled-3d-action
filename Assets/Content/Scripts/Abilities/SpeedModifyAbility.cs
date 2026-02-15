using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component allows user to temporarily modify their movement speed
/// </summary>
public class SpeedModifyAbility : MonoBehaviour, IEventSubscribedComponent
{
    [SerializeField]
    private float _modifier = 1.3f;

    [SerializeField]
    private float _duration = 14;

    private float _abilityTimer = 0f;
    private bool _active = false;

    // Update is called once per frame
    void Update()
    {
        if (!_active)
            return;

        _abilityTimer += Time.deltaTime;

        if (_abilityTimer > _duration)
        {
            _active = false;
            _abilityTimer = 0f;
        }
    }

    public void DoEffect(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled)
            return;

        if (!context.performed)
            return;

        _active = true;
        _abilityTimer = 0f;
    }

    public void ReceiveMessage(GameEventArgs args)
    {
        if (!_active)
            return;

        if (args is GetMoveSpeedModifiersEvent ev)
        {
            ev.Modifier *= _modifier;
        }
    }
}
