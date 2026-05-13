using UnityEngine;

/// <summary>
/// This component allows user to perform air jumps
/// </summary>
[RequireComponent(typeof(GenericMovement))]
public class MultipleJumps : MonoBehaviour
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

    public void OnCanJump(Component sender, object data)
    {
        if (data is not CanJumpData ev)
            return;

        if (sender.gameObject != gameObject)
            return;

        if (ev.Grounded)
            return;

        if (JumpsSpent >= MaxAirJumps)
            return;

        JumpsSpent++;
        ev.CanJump = true;
        ev.Handled = true;
    }

    public void OnRefreshAirJumps(Component sender, object data)
    {
        if (sender.gameObject != gameObject)
            return;

        JumpsSpent = 0;
    }
}
