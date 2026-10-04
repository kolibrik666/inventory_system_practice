using System;

using InventorySystem;
using SciptableObjects;

using UnityEngine;

namespace InteractionSystem
{
    [DisallowMultipleComponent]
    public sealed class WorldItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField, Min(1)] private int _quantity = 1;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private BoxCollider _pickupCollider;
        [SerializeField] private Sprite _fallbackIcon;
        [SerializeField, Min(0.01f)] private float _displaySize = 0.5f;
        [SerializeField, Min(0)] private float _hoverHeight = 0.4f;
        [SerializeField, Min(0)] private float _rotationSpeed = 90f;

        private bool _collected;

        public string Id => _itemData != null ? _itemData.ItemID : string.Empty;
        public string InteractionPrompt => "E) ADD TO INVENTORY";
        public bool CanInteract => isActiveAndEnabled && !_collected && _itemData != null && _quantity > 0;
        public Bounds PickupBounds => new(Vector3.up * _hoverHeight, Vector3.one * _displaySize);

        private void Awake()
        {
            if (_spriteRenderer == null || _pickupCollider == null || _spriteRenderer.transform.parent != transform || _pickupCollider.transform != transform)
            {
                Debug.LogError("WorldItem requires a direct child sprite renderer and a pickup collider on its root.", this);
                enabled = false;
                return;
            }

            ApplyAppearance();
        }

        private void Update()
        {
            if (!CanInteract) return;
            _spriteRenderer.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.Self);
        }

        public void Initialize(ItemData item, int quantity = 1)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (_collected) throw new InvalidOperationException("A collected world item cannot be initialized.");

            _itemData = item;
            _quantity = quantity;
            ApplyAppearance();
        }

        public bool TryInteract(Inventory inventory)
        {
            if (!CanInteract || inventory == null || inventory.IsOpen) return false;

            _collected = true;
            InventoryResult result = inventory.TryAddItem(Id, _quantity);
            if (result != InventoryResult.Success)
            {
                _collected = false;
                Debug.LogWarning($"Cannot pick up {_itemData.ItemName}: {result}.", this);
                return false;
            }

            _pickupCollider.enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
            return true;
        }

        private void ApplyAppearance()
        {
            if (_spriteRenderer == null || _pickupCollider == null) return;

            Sprite icon = _itemData != null ? _itemData.ItemIcon : null;
            _spriteRenderer.sprite = icon != null ? icon : _fallbackIcon;
            _spriteRenderer.enabled = _itemData != null && _spriteRenderer.sprite != null;
            _spriteRenderer.transform.localPosition = Vector3.up * _hoverHeight;
            if (_spriteRenderer.sprite != null)
            {
                Vector3 size = _spriteRenderer.sprite.bounds.size;
                float longestSide = Mathf.Max(size.x, size.y, 0.01f);
                _spriteRenderer.transform.localScale = Vector3.one * (_displaySize / longestSide);
            }

            _pickupCollider.center = Vector3.up * _hoverHeight;
            _pickupCollider.size = Vector3.one * _displaySize;
            _pickupCollider.isTrigger = true;
            _pickupCollider.enabled = _itemData != null && !_collected;
        }

        private void OnValidate()
        {
            _quantity = Mathf.Max(1, _quantity);
            _displaySize = Mathf.Max(0.01f, _displaySize);
            _hoverHeight = Mathf.Max(0f, _hoverHeight);
            _rotationSpeed = Mathf.Max(0f, _rotationSpeed);
        }
    }
}
