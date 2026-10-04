namespace InventorySystem
{
    public enum InventoryResult
    {
        Success,
        InvalidItem,
        InvalidQuantity,
        InvalidWeight,
        CapacityExceeded,
        EntryNotFound,
        NotEquippable,
        InvalidSlot,
        AlreadyEquipped,
        NotEquipped,
        NotConsumable,
        EquippedUnitReserved
    }
}
