public interface IInteractable
{
    bool RequiresCameraFocus { get; }
    bool RequiresMovementStop { get; }

    void Interact();
    void ExitInteraction();
}