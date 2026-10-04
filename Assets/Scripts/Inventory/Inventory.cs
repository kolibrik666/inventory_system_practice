using System;
using System.Collections.Generic;

using InteractionSystem;
using InventorySystem;
using SciptableObjects;

using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Inventory : MonoBehaviour
{
    [SerializeField] private InventoryView _view;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private ItemCatalog _itemCatalog;
    [SerializeField] private WorldItemDropper _itemDropper;
    [SerializeField, Min(0)] private float _capacity = 20;
    [SerializeField] private bool _startVisible;
    [SerializeField] private bool _debugMode;
    [SerializeField] private List<DebugInventoryItem> _debugItems = new();

    private readonly Dictionary<string, ItemData> _itemsById = new(StringComparer.Ordinal);

    private InventoryPresenter _presenter;
    private InputAction _toggleAction;
    private InputAction _closeAction;
    private InputAction _equipAction;

    public InventoryModel Model { get; private set; }
    public bool IsOpen => _view != null && _view.IsVisible;

    private void Awake()
    {
        if (_view == null)
        {
            Debug.LogError("Inventory requires a view reference.", this);
            enabled = false;
            return;
        }

        Model = new InventoryModel(_capacity);
        InitializeCatalog();
        _toggleAction = new InputAction("Toggle Inventory", InputActionType.Button, "<Keyboard>/i");
        _closeAction = new InputAction("Close Inventory", InputActionType.Button, "<Keyboard>/escape");
        _equipAction = new InputAction("Equip Item", InputActionType.Button, "<Keyboard>/space");
        _view.SetVisible(_startVisible);
        SeedDebugItems();
    }

    private void OnEnable()
    {
        if (Model == null) return;
        _presenter = new InventoryPresenter(Model, _view, TryDropItem);
        _view.VisibilityChanged += OnVisibilityChanged;
        _toggleAction.performed += OnToggle;
        _closeAction.performed += OnClose;
        _equipAction.performed += OnEquip;
        _toggleAction.Enable();
        _closeAction.Enable();
        _equipAction.Enable();
        OnVisibilityChanged(_view.IsVisible);
    }

    private void OnDisable()
    {
        if (_toggleAction == null) return;
        if (_view != null)
        {
            _view.SetVisible(false);
            _view.VisibilityChanged -= OnVisibilityChanged;
        }

        _toggleAction.performed -= OnToggle;
        _closeAction.performed -= OnClose;
        _equipAction.performed -= OnEquip;
        _toggleAction.Disable();
        _closeAction.Disable();
        _equipAction.Disable();
        _presenter?.Dispose();
        _presenter = null;
    }

    private void OnDestroy()
    {
        _toggleAction?.Dispose();
        _closeAction?.Dispose();
        _equipAction?.Dispose();
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        _presenter.SetVisible(!_view.IsVisible);
    }

    private void OnVisibilityChanged(bool visible)
    {
        if (_cameraController == null) return;
        _cameraController.SetInputEnabled(!visible);
    }

    private void OnClose(InputAction.CallbackContext context)
    {
        _presenter.SetVisible(false);
    }

    private void OnEquip(InputAction.CallbackContext context)
    {
        _presenter.ToggleEquipment();
    }

    public InventoryResult TryAddItem(string itemId, int quantity = 1)
    {
        if (Model == null || string.IsNullOrWhiteSpace(itemId) || !_itemsById.TryGetValue(itemId, out ItemData item))
        {
            return InventoryResult.InvalidItem;
        }

        return Model.TryAdd(item, quantity);
    }

    public InventoryResult TryDropItem(Guid entryId)
    {
        InventoryResult result = _itemDropper != null
            ? _itemDropper.TryDrop(Model, entryId) : InventoryResult.DropUnavailable;
        if (result != InventoryResult.Success)
        {
            Debug.LogWarning($"Cannot drop inventory item: {result}.", this);
        }

        return result;
    }

    private void InitializeCatalog()
    {
        if (_itemCatalog == null) return;

        foreach (ItemData item in _itemCatalog.Items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemID)) continue;
            if (_itemsById.TryAdd(item.ItemID, item)) continue;

            _itemsById[item.ItemID] = null;
            Debug.LogError($"Duplicate item ID in {_itemCatalog.name}: {item.ItemID}.", this);
        }
    }

    private void SeedDebugItems()
    {
        if (!_debugMode) return;

        foreach (DebugInventoryItem entry in _debugItems)
        {
            if (entry == null) continue;
            InventoryResult result = Model.TryAdd(entry.Item, entry.Quantity);
            if (result != InventoryResult.Success)
            {
                Debug.LogWarning($"Could not seed {entry.Item?.name ?? "missing item"}: {result}.", this);
            }
        }
    }

    [Serializable]
    private sealed class DebugInventoryItem
    {
        [SerializeField] private ItemData _item;
        [SerializeField, Min(1)] private int _quantity = 1;

        public ItemData Item => _item;
        public int Quantity => _quantity;
    }
}
