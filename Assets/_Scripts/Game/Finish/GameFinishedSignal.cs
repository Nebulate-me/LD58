namespace _Scripts.Game.Finish
{
    public struct GameFinishedSignal
    {
        public bool Success;
        public int FinalScore;
        
        public GameFinishedSignal(bool success, int score)
        {
            Success = success;
            FinalScore = score;
        }
    }
}