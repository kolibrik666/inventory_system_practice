using HaulSystem;

using UnityEngine;

namespace InteractionSystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ExtractionExit : MonoBehaviour, IInteractable
    {
        [SerializeField] private HaulRoundController _roundController;

        public string Id => "exit";
        public bool CanInteract => isActiveAndEnabled && _roundController != null && _roundController.CanExtract;

        private void Awake()
        {
            if (_roundController != null) return;
            Debug.LogError("ExtractionExit requires a haul round controller reference.", this);
            enabled = false;
        }

        public bool TryInteract(Inventory inventory)
        {
            return CanInteract && _roundController.TryExtract(inventory);
        }
    }
}
