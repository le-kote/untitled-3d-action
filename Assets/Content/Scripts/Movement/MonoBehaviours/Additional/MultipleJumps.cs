using UnityEngine;

/// <summary>
/// This component allows user to perform air jumps
/// </summary>
[RequireComponent(typeof(GenericMovement))]
public class MultipleJumps : FancyBehaviour
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

    protected override void InitializeEvents()
    {
        base.InitializeEvents();

        SubscribeLocalEvent<CanJumpEvent>(OnCanJump);
        SubscribeLocalEvent<RefreshAirJumpsEvent>(OnRefreshJumps);
    }

    private void OnCanJump(ref CanJumpEvent ev)
    {
        if (ev.Grounded)
            return;

        if (JumpsSpent >= MaxAirJumps)
            return;

        JumpsSpent++;
        ev.CanJump = true;
        ev.Handled = true;
    }

    private void OnRefreshJumps(ref RefreshAirJumpsEvent ev)
    {
        JumpsSpent = 0;
    }
}
