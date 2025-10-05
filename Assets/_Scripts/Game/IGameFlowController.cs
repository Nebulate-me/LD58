using System.Collections.Generic;
using System;

namespace _Scripts.Game
{
    public interface IGameFlowController
    {
        float CurrentDifficulty { get; }   // 0..1
        float ElapsedTime { get; }
        float TotalDuration { get; }       
        bool IsLevelComplete { get; }      
        event Action OnLevelCompleted;    

        IReadOnlyList<LevelEvent> GetLevelMap();
    }
}