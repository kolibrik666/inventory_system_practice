using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SciptableObjects;

namespace InventorySystem
{
    public sealed class InventoryModel
    {
        private readonly List<InventoryEntry> _entries = new();
        private readonly Dictionary<EquipmentSlot, InventoryEntry> _equipment = new();
        private readonly decimal _capacity;

        private decimal _currentWeight;

        public IReadOnlyList<InventoryEntry> Entries { get; }
        public IReadOnlyDictionary<EquipmentSlot, InventoryEntry> Equipment { get; }
        public float Capacity => (float)_capacity;
        public float CurrentWeight => (float)_currentWeight;

        public event Action Changed;
        public event Action<ItemConsumedEvent> ItemConsumed;

        public InventoryModel(float capacity)
        {
            if (!TryGetWeight(capacity, out _capacity))
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Entries = _entries.AsReadOnly();
            Equipment = new ReadOnlyDictionary<EquipmentSlot, InventoryEntry>(_equipment);
        }

        public InventoryEntry FindEntry(Guid id)
        {
            return _entries.Find(entry => entry.Id == id);
        }

        public InventoryResult TryAdd(ItemData item, int quantity)
        {
            if (item == null) return InventoryResult.InvalidItem;
            if (quantity <= 0) return InventoryResult.InvalidQuantity;
            if (!TryGetWeight(item.ItemWeightValue, out decimal unitWeight)) return InventoryResult.InvalidWeight;

            InventoryEntry stack = item.IsStackable ? _entries.Find(entry => entry.Item == item) : null;
            if (stack != null && quantity > int.MaxValue - stack.Quantity) return InventoryResult.InvalidQuantity;
            if (stack == null && !item.IsStackable && quantity > int.MaxValue - _entries.Count)
            {
                return InventoryResult.InvalidQuantity;
            }

            if (unitWeight > (_capacity - _currentWeight) / quantity) return InventoryResult.CapacityExceeded;

            if (stack != null)
            {
                stack.Quantity += quantity;
            }
            else if (item.IsStackable)
            {
                _entries.Add(new InventoryEntry(item, quantity, unitWeight));
            }
            else
            {
                for (int index = 0; index < quantity; index++)
                {
                    _entries.Add(new InventoryEntry(item, 1, unitWeight));
                }
            }

            _currentWeight += unitWeight * quantity;
            Changed?.Invoke();
            return InventoryResult.Success;
        }

        public InventoryResult TryEquip(Guid id)
        {
            InventoryEntry entry = FindEntry(id);
            if (entry == null) return InventoryResult.EntryNotFound;
            InventoryResult eligibility = GetEquipmentEligibility(entry.Item, out EquipmentSlot slot);
            if (eligibility != InventoryResult.Success) return eligibility;
            if (entry.IsEquipped) return InventoryResult.AlreadyEquipped;

            if (_equipment.TryGetValue(slot, out InventoryEntry previous))
            {
                previous.IsEquipped = false;
            }

            _equipment[slot] = entry;
            entry.IsEquipped = true;
            Changed?.Invoke();
            return InventoryResult.Success;
        }

        public InventoryResult TryUnequip(Guid id)
        {
            InventoryEntry entry = FindEntry(id);
            if (entry == null) return InventoryResult.EntryNotFound;
            if (!entry.IsEquipped) return InventoryResult.NotEquipped;

            ClearEquipment(entry);
            Changed?.Invoke();
            return InventoryResult.Success;
        }

        public InventoryResult TryRemove(Guid id, int quantity = 1)
        {
            if (quantity <= 0) return InventoryResult.InvalidQuantity;
            InventoryEntry entry = FindEntry(id);
            if (entry == null) return InventoryResult.EntryNotFound;
            if (quantity > entry.Quantity) return InventoryResult.InvalidQuantity;

            RemoveUnits(entry, quantity);
            Changed?.Invoke();
            return InventoryResult.Success;
        }

        public InventoryResult TryConsume(Guid id)
        {
            InventoryEntry entry = FindEntry(id);
            if (entry == null) return InventoryResult.EntryNotFound;
            if (!entry.Item.TryGetModule(out ConsumableData consumable)) return InventoryResult.NotConsumable;
            if (entry.IsEquipped && entry.Quantity == 1) return InventoryResult.EquippedUnitReserved;

            ItemConsumedEvent consumed = new(entry, consumable);
            RemoveUnits(entry, 1);
            Changed?.Invoke();
            ItemConsumed?.Invoke(consumed);
            return InventoryResult.Success;
        }

        public static InventoryResult GetEquipmentEligibility(ItemData item, out EquipmentSlot slot)
        {
            slot = default;
            if (item == null) return InventoryResult.NotEquippable;
            if (item.TryGetModule(out EquipmentData equipment))
            {
                slot = equipment.Slot;
            }
            else if (item.TryGetModule(out WeaponData _))
            {
                slot = EquipmentSlot.Weapon;
            }
            else
            {
                return InventoryResult.NotEquippable;
            }

            return Enum.IsDefined(typeof(EquipmentSlot), slot)
                ? InventoryResult.Success : InventoryResult.InvalidSlot;
        }

        private void RemoveUnits(InventoryEntry entry, int quantity)
        {
            entry.Quantity -= quantity;
            _currentWeight -= entry.UnitWeight * quantity;
            if (entry.Quantity > 0) return;

            if (entry.IsEquipped) ClearEquipment(entry);
            _entries.Remove(entry);
        }

        private void ClearEquipment(InventoryEntry entry)
        {
            EquipmentSlot occupiedSlot = default;
            foreach (KeyValuePair<EquipmentSlot, InventoryEntry> assignment in _equipment)
            {
                if (assignment.Value != entry) continue;
                occupiedSlot = assignment.Key;
                break;
            }

            _equipment.Remove(occupiedSlot);
            entry.IsEquipped = false;
        }

        private static bool TryGetWeight(float value, out decimal weight)
        {
            weight = 0;
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0) return false;

            try
            {
                weight = (decimal)value;
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}
