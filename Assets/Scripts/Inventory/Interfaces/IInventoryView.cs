using System;
using System.Collections.Generic;

using SciptableObjects;

namespace InventorySystem
{
    public interface IInventoryView
    {
        bool IsVisible { get; }

        event Action<Guid> ItemHovered;
        event Action<Guid> ItemClicked;
        event Action<Guid> ItemRightClicked;

        void SetVisible(bool visible);
        void ShowRows(IReadOnlyList<InventoryRowData> rows);
        void ShowSelection(Guid previousId, Guid currentId);
        void ShowDetails(ItemData item, IReadOnlyList<InventoryDetailData> details, bool canConsume, bool canEquip, bool equipped, bool canDrop);
        void ShowWeight(float current, float capacity);
    }

    public readonly struct InventoryRowData
    {
        public Guid Id { get; }
        public string Name { get; }
        public int Quantity { get; }
        public bool CanEquip { get; }
        public bool Equipped { get; }
        public bool Selected { get; }

        public InventoryRowData(InventoryEntry entry, bool selected, bool canEquip)
        {
            Id = entry.Id;
            Name = entry.Item.ItemName;
            Quantity = entry.Quantity;
            CanEquip = canEquip;
            Equipped = entry.IsEquipped;
            Selected = selected;
        }
    }

    public readonly struct InventoryDetailData
    {
        public string Label { get; }
        public string Value { get; }

        public InventoryDetailData(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}
