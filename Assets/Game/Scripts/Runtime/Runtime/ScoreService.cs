using System;

namespace SpaceFlight2D.Game.Runtime
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        event Action<int> ScoreChanged;
        void Reset();
        void AddScore(int amount);
    }

    public sealed class ScoreService : IScoreService
    {
        public int CurrentScore { get; private set; }
        public event Action<int> ScoreChanged;

        public void Reset()
        {
            CurrentScore = 0;
            ScoreChanged?.Invoke(CurrentScore);
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            ScoreChanged?.Invoke(CurrentScore);
        }
    }
}
