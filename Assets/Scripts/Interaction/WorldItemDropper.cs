using System;

using InventorySystem;

using UnityEngine;

namespace InteractionSystem
{
    [DisallowMultipleComponent]
    public sealed class WorldItemDropper : MonoBehaviour
    {
        [SerializeField] private WorldItem _worldItemPrefab;
        [SerializeField] private Transform _dropOrigin;
        [SerializeField, Min(0.1f)] private float _dropDistance = 1.5f;
        [SerializeField, Min(0.1f)] private float _groundProbeDistance = 5f;
        [SerializeField] private LayerMask _placementLayers = Physics.DefaultRaycastLayers;

        public InventoryResult TryDrop(InventoryModel inventory, Guid entryId)
        {
            if (!isActiveAndEnabled || inventory == null || _worldItemPrefab == null || _dropOrigin == null)
            {
                return InventoryResult.DropUnavailable;
            }

            InventoryEntry entry = inventory.FindEntry(entryId);
            if (entry == null) return InventoryResult.EntryNotFound;
            if (entry.Item == null) return InventoryResult.InvalidItem;
            if (!TryGetDropPosition(out Vector3 position)) return InventoryResult.DropUnavailable;

            WorldItem pickup = Instantiate(_worldItemPrefab, position, Quaternion.identity);
            pickup.gameObject.SetActive(false);
            if (!pickup.enabled)
            {
                Destroy(pickup.gameObject);
                return InventoryResult.DropUnavailable;
            }

            pickup.Initialize(entry.Item, 1);
            InventoryResult result = inventory.TryRemove(entryId);
            if (result != InventoryResult.Success)
            {
                Destroy(pickup.gameObject);
                return result;
            }

            pickup.gameObject.SetActive(true);
            return InventoryResult.Success;
        }

        private bool TryGetDropPosition(out Vector3 position)
        {
            position = default;
            Vector3 forward = Vector3.ProjectOnPlane(_dropOrigin.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f) return false;

            Bounds bounds = _worldItemPrefab.PickupBounds;
            Vector3 halfExtents = bounds.extents + Vector3.one * 0.02f;
            float distance = _dropDistance;
            if (Physics.Raycast(_dropOrigin.position, forward, out RaycastHit obstacle, distance + halfExtents.magnitude,
                _placementLayers, QueryTriggerInteraction.Ignore))
            {
                distance = Mathf.Clamp(obstacle.distance - halfExtents.magnitude - 0.02f, 0f, _dropDistance);
            }

            Vector3 probeOrigin = _dropOrigin.position + forward * distance;
            if (!Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit ground, _groundProbeDistance,
                _placementLayers, QueryTriggerInteraction.Ignore)) return false;

            position = ground.point + Vector3.up * Mathf.Max(0.02f, 0.02f - bounds.min.y);
            return !Physics.CheckBox(position + bounds.center, halfExtents, Quaternion.identity,
                _placementLayers, QueryTriggerInteraction.Ignore);
        }

        private void OnValidate()
        {
            _dropDistance = Mathf.Max(0.1f, _dropDistance);
            _groundProbeDistance = Mathf.Max(0.1f, _groundProbeDistance);
        }
    }
}
