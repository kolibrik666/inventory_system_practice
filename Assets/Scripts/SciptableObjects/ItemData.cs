using System;
using System.Collections.Generic;

using UnityEngine;

namespace SciptableObjects
{
    [CreateAssetMenu(fileName = "New Item Data", menuName = "ScriptableObjects/ItemData", order = 1)]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string _itemID;
        [SerializeField] private bool _isStackable = true;
        [SerializeField] private string _itemName;
        [SerializeField] private Sprite _itemIcon;
        [SerializeField] private string _itemDesc;
        [SerializeField] private float _itemWeightValue;
        [SerializeField] private int _itemMoneyValue;
        [Tooltip("Optional data modules stored inside this item. Add module types in the Inspector.")]
        [SerializeReference] private List<ItemModuleData> _modules = new();

        public string ItemID => _itemID;
        public bool IsStackable => _isStackable;
        public string ItemName => _itemName;
        public Sprite ItemIcon => _itemIcon;
        public string ItemDesc => _itemDesc;
        public float ItemWeightValue => _itemWeightValue;
        public int ItemMoneyValue => _itemMoneyValue;
        public IReadOnlyList<ItemModuleData> Modules => _modules;

        // Returns the first matching module; use Modules to process every attached module.
        public bool TryGetModule<T>(out T module) where T : ItemModuleData
        {
            foreach (ItemModuleData candidate in _modules)
            {
                if (candidate != null && candidate is T matchingModule)
                {
                    module = matchingModule;
                    return true;
                }
            }

            module = null;
            return false;
        }

        private void OnValidate()
        {
            // Unity's standard list can duplicate the previous entry when adding a slot.
            // Keep the first module of each type and leave new duplicate slots empty.
            HashSet<Type> moduleTypes = new();
            for (int index = 0; index < _modules.Count; index++)
            {
                ItemModuleData module = _modules[index];
                if (module != null && !moduleTypes.Add(module.GetType()))
                {
                    _modules[index] = null;
                }
            }

            // Generates a GUID if one hasn't been set yet
            if (string.IsNullOrEmpty(_itemID))
            {
                _itemID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }
    }
}
