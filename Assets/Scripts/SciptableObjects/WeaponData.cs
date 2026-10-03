using System;

using UnityEngine;

namespace SciptableObjects
{
    [Serializable]
    public sealed class WeaponData : ItemModuleData
    {
        [SerializeField, Min(0)] private int _damageValue;
        [Tooltip("Seconds between attacks.")]
        [SerializeField, Min(0.01f)] private float _attackInterval = 1f;
        [SerializeField, Min(0)] private float _range = 1f;

        public int DamageValue => _damageValue;
        public float AttackInterval => _attackInterval;
        public float Range => _range;
    }
}
