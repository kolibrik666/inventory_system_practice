using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0)] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -20f;

    private CharacterController _characterController;
    private InputAction _moveAction;
    private float _verticalVelocity;
    private bool _inputEnabled = true;

    private void Awake()
    {
        if (!TryGetComponent(out _characterController))
        {
            Debug.LogError("PlayerController requires a CharacterController on the same GameObject.", this);
            enabled = false;
            return;
        }

        _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    private void OnEnable()
    {
        _moveAction?.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
        _verticalVelocity = 0f;
    }

    private void OnDestroy()
    {
        _moveAction?.Dispose();
    }

    private void Update()
    {
        if (_characterController == null || !_characterController.enabled) return;

        Vector2 input = _inputEnabled && Application.isFocused
            ? Vector2.ClampMagnitude(_moveAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 movement = (right * input.x + forward * input.y) * _moveSpeed;

        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
        movement.y = _verticalVelocity;
        CollisionFlags collisions = _characterController.Move(movement * Time.deltaTime);

        if ((collisions & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
        {
            _verticalVelocity = 0f;
        }

        if ((collisions & CollisionFlags.Below) != 0 && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }
    }

    public void SetInputEnabled(bool inputEnabled)
    {
        _inputEnabled = inputEnabled;
    }

    private void OnValidate()
    {
        _moveSpeed = Mathf.Max(0f, _moveSpeed);
        _gravity = Mathf.Min(-0.01f, _gravity);
    }
}
