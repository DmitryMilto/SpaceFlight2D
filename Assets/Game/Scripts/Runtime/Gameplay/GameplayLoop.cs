using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceFlight2D.Game.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class GameplayLoop : MonoBehaviour
    {
        private GameBootstrapper _bootstrapper;

        [Inject]
        public void Construct(GameBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public void StartLaunch()
        {
            _bootstrapper?.StartGameAsync().Forget();
        }

        public void DestroyRocket()
        {
            _bootstrapper?.FinishGameAsync().Forget();
        }

        public void RestartGame()
        {
            _bootstrapper?.RestartGame();
        }
    }
}
