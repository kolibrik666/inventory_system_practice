using System;

using InventorySystem;

using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InventoryItemView : TextButton
{
    [SerializeField] private GameObject _equipmentToggle;
    [SerializeField] private GameObject _equippedIndicator;

    private Guid _entryId;
    private bool _selected;

    public Guid EntryId => _entryId;

    public event Action<Guid> Hovered;
    public event Action<Guid> Clicked;

    public void Render(InventoryRowData data)
    {
        _entryId = data.Id;
        PrimaryText.text = data.Name;
        SecondaryText.text = data.Quantity.ToString();
        _equipmentToggle.SetActive(data.CanEquip);
        _equippedIndicator.SetActive(data.Equipped);
        SetSelected(data.Selected);
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        DoStateTransition(SelectionState.Normal, true);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (IsInteractable()) Hovered?.Invoke(_entryId);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable() || eventData.button != PointerEventData.InputButton.Left) return;
        Clicked?.Invoke(_entryId);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (_selected && state is SelectionState.Normal or SelectionState.Selected)
        {
            state = SelectionState.Highlighted;
        }

        base.DoStateTransition(state, instant);
    }
}
