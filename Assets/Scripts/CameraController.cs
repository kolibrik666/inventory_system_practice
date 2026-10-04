using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField, Min(0)] private float _mouseSensitivity = 0.1f;
    [SerializeField, Range(0, 89)] private float _pitchLimit = 85f;

    private InputAction _lookAction;
    private InputAction _releaseCursorAction;
    private InputAction _captureCursorAction;
    private float _pitch;
    private int _captureFrame = -1;
    private bool _inputEnabled = true;

    private void Awake()
    {
        if (_playerTransform == null || _playerTransform == transform || !transform.IsChildOf(_playerTransform))
        {
            Debug.LogError("CameraController requires a player transform that is an ancestor of this camera.", this);
            enabled = false;
            return;
        }

        _pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, transform.localEulerAngles.x), -_pitchLimit, _pitchLimit);
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        _lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");
        _releaseCursorAction = new InputAction("Release Cursor", InputActionType.Button, "<Keyboard>/escape");
        _captureCursorAction = new InputAction("Capture Cursor", InputActionType.Button, "<Mouse>/leftButton");
    }

    private void OnEnable()
    {
        if (_lookAction == null) return;

        _lookAction.Enable();
        _releaseCursorAction.Enable();
        _captureCursorAction.Enable();

        if (_inputEnabled && Application.isFocused)
        {
            CaptureCursor();
        }
    }

    private void OnDisable()
    {
        if (_lookAction == null) return;

        _lookAction.Disable();
        _releaseCursorAction.Disable();
        _captureCursorAction.Disable();
        ReleaseCursor();
    }

    private void OnDestroy()
    {
        _lookAction?.Dispose();
        _releaseCursorAction?.Dispose();
        _captureCursorAction?.Dispose();
    }

    private void Update()
    {
        if (!_inputEnabled || !Application.isFocused || _captureFrame == Time.frameCount) return;

        if (_releaseCursorAction.WasPressedThisFrame())
        {
            ReleaseCursor();
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (_captureCursorAction.WasPressedThisFrame())
            {
                CaptureCursor();
            }

            return;
        }

        Vector2 look = _lookAction.ReadValue<Vector2>() * _mouseSensitivity;
        _playerTransform.Rotate(Vector3.up, look.x, Space.World);
        _pitch = Mathf.Clamp(_pitch - look.y, -_pitchLimit, _pitchLimit);
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public void SetInputEnabled(bool inputEnabled)
    {
        _inputEnabled = inputEnabled;
        if (!isActiveAndEnabled || _lookAction == null) return;

        if (_inputEnabled && Application.isFocused)
        {
            CaptureCursor();
        }
        else
        {
            ReleaseCursor();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!isActiveAndEnabled || _lookAction == null) return;

        if (hasFocus && _inputEnabled)
        {
            CaptureCursor();
        }
        else
        {
            ReleaseCursor();
        }
    }

    private void CaptureCursor()
    {
        _captureFrame = Time.frameCount;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnValidate()
    {
        _mouseSensitivity = Mathf.Max(0f, _mouseSensitivity);
        _pitchLimit = Mathf.Clamp(_pitchLimit, 0f, 89f);
    }
}
