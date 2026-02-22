using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class GenericMovement
{
    [Header("Camera", order = 0)]

    [SerializeField]
    private float _cameraSpeed = 5;

    [SerializeField]
    private float _cameraYLimits = 85f;

    [SerializeField]
    private float _walkFov = 60;

    [SerializeField]
    private float _sprintFov = 70;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private GameObject _cameraHolder;

    public GameObject CameraHolder { get => _cameraHolder; }

    private Quaternion _cameraRot = new();
    private Vector2 _cameraMove = Vector2.zero;
    private float _verticalRotation = 0f;

    private void UpdateCamera()
    {
        if (_cameraMove.sqrMagnitude <= 0.01f)
            return;

        var effectiveSpeed = _cameraSpeed * (60f / _camera.fieldOfView);
        transform.Rotate(0, _cameraMove.x * Time.deltaTime * effectiveSpeed, 0);

        _verticalRotation += -_cameraMove.y * Time.deltaTime * effectiveSpeed;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -_cameraYLimits, _cameraYLimits);

        _cameraRot = Quaternion.Euler(_verticalRotation, 0, 0);
        CameraHolder.transform.localRotation = _cameraRot;
    }
}
