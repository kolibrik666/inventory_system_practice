namespace InteractionSystem
{
    public interface IInteractable
    {
        string Id { get; }
        string InteractionPrompt { get; }
        bool CanInteract { get; }

        bool TryInteract(Inventory inventory);
    }
}
