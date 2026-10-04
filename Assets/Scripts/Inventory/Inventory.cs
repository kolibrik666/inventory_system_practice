using System;
using System.Collections.Generic;

using InventorySystem;
using SciptableObjects;

using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Inventory : MonoBehaviour
{
    [SerializeField] private InventoryView _view;
    [SerializeField] private CameraController _cameraController;
    [SerializeField, Min(0)] private float _capacity = 20;
    [SerializeField] private bool _startVisible;
    [SerializeField] private bool _debugMode;
    [SerializeField] private List<DebugInventoryItem> _debugItems = new();

    private InventoryPresenter _presenter;
    private InputAction _toggleAction;
    private InputAction _closeAction;
    private InputAction _equipAction;

    public InventoryModel Model { get; private set; }

    private void Awake()
    {
        if (_view == null)
        {
            Debug.LogError("Inventory requires a view reference.", this);
            enabled = false;
            return;
        }

        Model = new InventoryModel(_capacity);
        _toggleAction = new InputAction("Toggle Inventory", InputActionType.Button, "<Keyboard>/i");
        _closeAction = new InputAction("Close Inventory", InputActionType.Button, "<Keyboard>/escape");
        _equipAction = new InputAction("Equip Item", InputActionType.Button, "<Keyboard>/space");
        _view.SetVisible(_startVisible);
        SeedDebugItems();
    }

    private void OnEnable()
    {
        if (Model == null) return;
        _presenter = new InventoryPresenter(Model, _view);
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
