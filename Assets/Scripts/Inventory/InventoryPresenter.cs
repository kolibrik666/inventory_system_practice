using System;
using System.Collections.Generic;

using SciptableObjects;

namespace InventorySystem
{
    public sealed class InventoryPresenter : IDisposable
    {
        private readonly InventoryModel _model;
        private readonly IInventoryView _view;
        private readonly Func<Guid, InventoryResult> _dropItem;
        private readonly List<InventoryRowData> _rows = new();

        private Guid _selectedId;
        private int _selectedIndex;
        private bool _disposed;

        public InventoryPresenter(InventoryModel model, IInventoryView view, Func<Guid, InventoryResult> dropItem)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _dropItem = dropItem ?? throw new ArgumentNullException(nameof(dropItem));
            _model.Changed += Refresh;
            _view.ItemHovered += Select;
            _view.ItemClicked += Consume;
            _view.ItemRightClicked += Drop;
            Refresh();
        }

        public void SetVisible(bool visible)
        {
            if (_disposed) return;
            _view.SetVisible(visible);
        }

        public void ToggleEquipment()
        {
            if (_disposed || !_view.IsVisible) return;
            InventoryEntry entry = _model.FindEntry(_selectedId);
            if (entry == null) return;

            if (entry.IsEquipped)
            {
                _model.TryUnequip(entry.Id);
            }
            else
            {
                _model.TryEquip(entry.Id);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _model.Changed -= Refresh;
            _view.ItemHovered -= Select;
            _view.ItemClicked -= Consume;
            _view.ItemRightClicked -= Drop;
        }

        private void Select(Guid id)
        {
            if (!_view.IsVisible || id == _selectedId) return;

            for (int index = 0; index < _model.Entries.Count; index++)
            {
                InventoryEntry entry = _model.Entries[index];
                if (entry.Id != id) continue;

                Guid previousId = _selectedId;
                _selectedId = id;
                _selectedIndex = index;
                _view.ShowSelection(previousId, id);
                ShowSelectedDetails(entry);
                return;
            }
        }

        private void Consume(Guid id)
        {
            if (!_view.IsVisible) return;
            Select(id);
            _model.TryConsume(id);
        }

        private void Drop(Guid id)
        {
            if (!_view.IsVisible) return;
            Select(id);
            _dropItem(id);
        }

        private void Refresh()
        {
            InventoryEntry selected = _model.FindEntry(_selectedId);
            if (selected == null && _model.Entries.Count > 0)
            {
                selected = _model.Entries[Math.Min(_selectedIndex, _model.Entries.Count - 1)];
            }

            _selectedId = selected?.Id ?? Guid.Empty;
            _rows.Clear();
            for (int index = 0; index < _model.Entries.Count; index++)
            {
                InventoryEntry entry = _model.Entries[index];
                bool isSelected = entry.Id == _selectedId;
                if (isSelected) _selectedIndex = index;
                bool canEquip = InventoryModel.GetEquipmentEligibility(entry.Item, out _) == InventoryResult.Success;
                _rows.Add(new InventoryRowData(entry, isSelected, canEquip));
            }

            _view.ShowRows(_rows);
            _view.ShowWeight(_model.CurrentWeight, _model.Capacity);
            ShowSelectedDetails(selected);
        }

        private void ShowSelectedDetails(InventoryEntry selected)
        {
            if (selected == null)
            {
                _view.ShowDetails(null, Array.Empty<InventoryDetailData>(), false, false, false, false);
                return;
            }

            bool canConsume = selected.Item.TryGetModule(out ConsumableData _)
                && (!selected.IsEquipped || selected.Quantity > 1);
            bool canEquip = InventoryModel.GetEquipmentEligibility(selected.Item, out _) == InventoryResult.Success;
            _view.ShowDetails(selected.Item, InventoryDetailsFormatter.Create(selected.Item), canConsume, canEquip, selected.IsEquipped, true);
        }
    }
}
