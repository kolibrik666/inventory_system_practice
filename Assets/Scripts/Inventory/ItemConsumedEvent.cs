using System;

using SciptableObjects;

namespace InventorySystem
{
    public readonly struct ItemConsumedEvent
    {
        public Guid EntryId { get; }
        public ItemData Item { get; }
        public int Health { get; }
        public int Stamina { get; }
        public int Mana { get; }

        internal ItemConsumedEvent(InventoryEntry entry, ConsumableData effects)
        {
            EntryId = entry.Id;
            Item = entry.Item;
            Health = effects.HealthValue;
            Stamina = effects.StaminaValue;
            Mana = effects.ManaValue;
        }
    }
}
