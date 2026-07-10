namespace MazeRunner.Core
{
    public interface IInteractable
    {
        void Interact(Player.PlayerInteraction player);
        string GetPromptText();
        bool CanInteract(Player.PlayerInteraction player);
    }
}
