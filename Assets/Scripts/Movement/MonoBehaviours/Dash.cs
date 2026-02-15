using UnityEngine;

[RequireComponent(typeof(GenericMovement), typeof(CharacterController))]
public class Dash : MonoBehaviour
{
    [SerializeField]
    private float _impulse = 6f;

    [SerializeField]
    private float _cooldown = 3f;

    [SerializeField]
    private float _duration = 3f;

    private float _cooldownAccumulator = 3f;
    private float _dashAccumulator = 0f;

    private GenericMovement _movement;
    private CharacterController _cc;
    private Vector3 _dashTo = Vector3.zero;
    private bool _isDashing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movement = GetComponent<GenericMovement>();
        _cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleDash();

        if (_cooldownAccumulator < _cooldown)
            _cooldownAccumulator += Time.deltaTime;
    }

    private void HandleDash()
    {
        if (!_isDashing)
            return;

        if (_dashAccumulator >= _duration)
        {
            _isDashing = false;
            _dashTo = Vector3.zero;
            _dashAccumulator = 0;
            return;
        }

        _dashAccumulator += Time.deltaTime;
        _cc.Move(_dashTo * _impulse * Time.deltaTime);
    }

    public void DoDash()
    {        
        if (_cooldownAccumulator < _cooldown)
            return;

        _isDashing = true;
        _dashTo = _movement.Input != Vector2.zero ? _movement.GetMoveDirection() : transform.forward;   
    }
}
