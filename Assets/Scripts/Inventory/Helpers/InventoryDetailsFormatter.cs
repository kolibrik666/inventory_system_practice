using System.Collections.Generic;
using System.Globalization;

using SciptableObjects;

namespace InventorySystem
{
    public static class InventoryDetailsFormatter
    {
        public static IReadOnlyList<InventoryDetailData> Create(ItemData item)
        {
            List<InventoryDetailData> details = new()
            {
                new InventoryDetailData("Weight", Format(item.ItemWeightValue)),
                new InventoryDetailData("Value", item.ItemMoneyValue.ToString(CultureInfo.InvariantCulture))
            };

            if (InventoryModel.GetEquipmentEligibility(item, out EquipmentSlot slot) == InventoryResult.Success)
            {
                details.Add(new InventoryDetailData("Slot", slot.ToString()));
            }

            foreach (ItemModuleData module in item.Modules)
            {
                switch (module)
                {
                    case EquipmentData equipment:
                        details.Add(new InventoryDetailData("Defense", equipment.DefenseValue.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case WeaponData weapon:
                        details.Add(new InventoryDetailData("Damage", weapon.DamageValue.ToString(CultureInfo.InvariantCulture)));
                        details.Add(new InventoryDetailData("Attack interval", $"{Format(weapon.AttackInterval)} s"));
                        details.Add(new InventoryDetailData("Range", Format(weapon.Range)));
                        break;
                    case ConsumableData consumable:
                        AddEffect(details, "Health", consumable.HealthValue);
                        AddEffect(details, "Stamina", consumable.StaminaValue);
                        AddEffect(details, "Mana", consumable.ManaValue);
                        break;
                }
            }

            return details;
        }

        private static void AddEffect(List<InventoryDetailData> details, string label, int value)
        {
            if (value == 0) return;
            details.Add(new InventoryDetailData(label, value.ToString(CultureInfo.InvariantCulture)));
        }

        private static string Format(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
