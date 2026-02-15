using UnityEngine;

[RequireComponent(typeof(GenericMovement))]
public class MultipleJumps : MonoBehaviour, IEventSubscribedComponent
{
    [SerializeField]
    private int MaxAirJumps = 1;
    private int JumpsSpent = 0;

    private GenericMovement _movement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_movement.IsGrounded)
            JumpsSpent = 0;
    }

    public void ReceiveMessage(GameEventArgs args)
    {        
        if (args is CanJumpEvent ev)
            OnCanJump(ev);

        if (args is RefreshAirJumpsEvent)
            JumpsSpent = 0;
    }

    private void OnCanJump(CanJumpEvent ev)
    {
        if (_movement.IsGrounded)
            return;

        if (JumpsSpent >= MaxAirJumps)
            return;

        JumpsSpent++;
        ev.CanJump = true;
        ev.Handled = true;
    }
}
