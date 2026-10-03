using System;

using UnityEngine;

namespace SciptableObjects
{
    [Serializable]
    public sealed class ConsumableData : ItemModuleData
    {
        [SerializeField, Min(0)] private int _healthValue;
        [SerializeField, Min(0)] private int _staminaValue;
        [SerializeField, Min(0)] private int _manaValue;

        public int HealthValue => _healthValue;
        public int StaminaValue => _staminaValue;
        public int ManaValue => _manaValue;
    }
}
