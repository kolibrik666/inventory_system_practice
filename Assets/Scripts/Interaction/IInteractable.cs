namespace InteractionSystem
{
    public interface IInteractable
    {
        string Id { get; }
        bool CanInteract { get; }

        bool TryInteract(Inventory inventory);
    }
}
