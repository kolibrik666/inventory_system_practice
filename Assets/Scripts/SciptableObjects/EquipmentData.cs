using System;

using UnityEngine;

namespace SciptableObjects
{
    public enum EquipmentSlot
    {
        Head,
        Body,
        Accessory,
        Weapon
    }

    [Serializable]
    public sealed class EquipmentData : ItemModuleData
    {
        [SerializeField] private EquipmentSlot _slot;
        [SerializeField, Min(0)] private int _defenseValue;

        public EquipmentSlot Slot => _slot;
        public int DefenseValue => _defenseValue;
    }
}
