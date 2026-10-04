using System;

using SciptableObjects;

namespace InventorySystem
{
    public sealed class InventoryEntry
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ItemData Item { get; }
        public int Quantity { get; internal set; }
        public bool IsEquipped { get; internal set; }

        internal decimal UnitWeight { get; }

        internal InventoryEntry(ItemData item, int quantity, decimal unitWeight)
        {
            Item = item;
            Quantity = quantity;
            UnitWeight = unitWeight;
        }
    }
}
