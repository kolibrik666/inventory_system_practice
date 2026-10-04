using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractionSystem
{
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private InteractionTipView _tipView;
        [SerializeField, Min(0.1f)] private float _interactionDistance = 3f;
        [SerializeField] private LayerMask _raycastLayers = Physics.DefaultRaycastLayers;

        private InputAction _interactAction;

        private void Awake()
        {
            if (_camera == null || _inventory == null)
            {
                Debug.LogError("PlayerInteractor requires camera and inventory references.", this);
                enabled = false;
                return;
            }

            if (_tipView == null)
            {
                Debug.LogWarning("PlayerInteractor requires a tip view reference to display interaction prompts.", this);
            }

            _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
        }

        private void OnEnable()
        {
            _interactAction?.Enable();
        }

        private void OnDisable()
        {
            _interactAction?.Disable();
            _tipView?.Hide();
        }

        private void OnDestroy()
        {
            _interactAction?.Dispose();
        }

        private void LateUpdate()
        {
            if (!Application.isFocused || _inventory == null || !_inventory.isActiveAndEnabled
                || _inventory.IsOpen || Cursor.lockState != CursorLockMode.Locked)
            {
                _tipView?.Hide();
                return;
            }

            IInteractable target = FindTarget();
            if (target == null)
            {
                _tipView?.Hide();
                return;
            }

            _tipView?.Show(target.InteractionPrompt);
            if (_interactAction.WasPressedThisFrame() && target.TryInteract(_inventory))
            {
                _tipView?.Hide();
            }
        }

        private IInteractable FindTarget()
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            ray.origin = _camera.transform.position;
            if (!Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _raycastLayers, QueryTriggerInteraction.Collide)) return null;

            IInteractable target = hit.collider.GetComponentInParent<IInteractable>();
            return target != null && target.CanInteract && !string.IsNullOrWhiteSpace(target.Id) ? target : null;
        }

        private void OnValidate()
        {
            _interactionDistance = Mathf.Max(0.1f, _interactionDistance);
        }
    }
}
