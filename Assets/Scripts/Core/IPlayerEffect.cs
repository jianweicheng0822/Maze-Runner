namespace MazeRunner.Core
{
    public interface IPlayerEffect
    {
        string EffectName { get; }
        float Duration { get; }
        bool IsStackable { get; }
        void Apply(Player.PlayerController player);
        void Remove(Player.PlayerController player);
        void Tick(Player.PlayerController player, float deltaTime);
    }
}
