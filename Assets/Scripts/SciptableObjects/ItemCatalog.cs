using System.Collections.Generic;

using UnityEngine;

namespace SciptableObjects
{
    [CreateAssetMenu(fileName = "Item Catalog", menuName = "ScriptableObjects/ItemCatalog")]
    public sealed class ItemCatalog : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new();

        public IReadOnlyList<ItemData> Items => _items;
    }
}
