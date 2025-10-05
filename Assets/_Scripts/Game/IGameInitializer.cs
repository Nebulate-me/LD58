using _Scripts.Ships;

namespace _Scripts.Game
{
    public interface IGameInitializer
    {
        TrainController PlayerShip { get; }
    }
}