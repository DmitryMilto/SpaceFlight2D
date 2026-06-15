using System;

namespace SpaceFlight2D.Game.Runtime
{
    public interface IGameStateService
    {
        GameState CurrentState { get; }
        event Action<GameState> StateChanged;
        void SetState(GameState state);
    }

    public sealed class GameStateService : IGameStateService
    {
        public GameState CurrentState { get; private set; } = GameState.Idle;
        public event Action<GameState> StateChanged;

        public void SetState(GameState state)
        {
            if (CurrentState == state)
            {
                return;
            }

            CurrentState = state;
            StateChanged?.Invoke(state);
        }
    }
}
