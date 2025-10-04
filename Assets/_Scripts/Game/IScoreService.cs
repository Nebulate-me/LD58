namespace _Scripts.Game
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        void AddScore(int amount);
        void ResetScore();
    }
}