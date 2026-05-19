using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component allows user to temporarily modify their movement speed
/// </summary>
public class SpeedModifyAbility : FancyBehaviour
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

    protected override void InitializeEvents()
    {
        base.InitializeEvents();

        SubscribeLocalEvent<GetMoveSpeedModifiersEvent>(OnGetMoveSpeedModifiers);
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

    private void OnGetMoveSpeedModifiers(ref GetMoveSpeedModifiersEvent ev)
    {
        if (!_active)
            return;

        ev.Modifier *= _modifier;
    }
}
