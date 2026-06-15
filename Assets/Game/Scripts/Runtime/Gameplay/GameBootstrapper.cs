using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SpaceFlight2D.Game;
using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace SpaceFlight2D.Game.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrapper : MonoBehaviour, IInitializable
    {
        private PrototypeGameConfig _config;
        private IGameStateService _gameStateService;
        private IScoreService _scoreService;
        private RocketController _rocketController;
        private AsteroidSpawner _asteroidSpawner;
        private BackgroundController _backgroundController;
        private CameraController _cameraController;
        private UIController _uiController;
        private IVfxService _vfxService;
        private ICameraShakeService _cameraShakeService;
        private CancellationTokenSource _destroyCts;
        private bool _initialized;
        private bool _isHandlingRocketHit;
        private bool _fogVfxPlayed;
        private bool _spaceEntryVfxPlayed;

        [Inject]
        public void Construct(
            PrototypeGameConfig config,
            IGameStateService gameStateService,
            IScoreService scoreService,
            RocketController rocketController,
            AsteroidSpawner asteroidSpawner,
            BackgroundController backgroundController,
            CameraController cameraController,
            UIController uiController,
            IVfxService vfxService,
            ICameraShakeService cameraShakeService)
        {
            _config = config;
            _gameStateService = gameStateService;
            _scoreService = scoreService;
            _rocketController = rocketController;
            _asteroidSpawner = asteroidSpawner;
            _backgroundController = backgroundController;
            _cameraController = cameraController;
            _uiController = uiController;
            _vfxService = vfxService;
            _cameraShakeService = cameraShakeService;
        }

        private void OnDestroy()
        {
            UnsubscribeFromRocket();
            _destroyCts?.Cancel();
            _destroyCts?.Dispose();
            _destroyCts = null;
        }

        public void Initialize()
        {
            _destroyCts ??= new CancellationTokenSource();
            InitializeGame();
        }

        public void InitializeGame()
        {
            if (_initialized)
            {
                return;
            }

            if (!IsReady())
            {
                return;
            }

            SubscribeToRocket();
            _scoreService.Reset();
            _gameStateService.SetState(GameState.Idle);
            _rocketController.ResetRocket();
            _asteroidSpawner.StopSpawning();
            _backgroundController?.SetIdleMode();
            _cameraController?.ResetCamera();
            _uiController.ResetUiState();
            _fogVfxPlayed = false;
            _spaceEntryVfxPlayed = false;
            _initialized = true;
        }

        public UniTask StartGameAsync()
        {
            return StartGameAsync(_destroyCts != null ? _destroyCts.Token : CancellationToken.None);
        }

        public async UniTask StartGameAsync(CancellationToken token)
        {
            try
            {
                if (!IsReady() || _gameStateService.CurrentState != GameState.Idle)
                {
                    return;
                }

                _gameStateService.SetState(GameState.Launching);
                _uiController.HideStartUI();
                _vfxService.SpawnLaunchBurst(_rocketController.transform.position + Vector3.down * 0.7f);
                _cameraShakeService.ShakeSmall();
                _rocketController.LaunchAsync(token).Forget();

                var launchDelay = _config.StartFlow.LaunchDelay;
                if (launchDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(launchDelay), cancellationToken: token);
                }

                if (token.IsCancellationRequested || _gameStateService.CurrentState != GameState.Launching)
                {
                    return;
                }

                _cameraController?.FollowRocket();
                _backgroundController?.StartScrolling();

                var spaceStartHeight = _config.LaunchFlow.SpaceStartHeight;
                while (!token.IsCancellationRequested
                       && _gameStateService.CurrentState == GameState.Launching
                       && _rocketController != null
                       && _rocketController.transform.position.y < spaceStartHeight)
                {
                    TriggerLaunchZoneVfx();
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                if (token.IsCancellationRequested || _gameStateService.CurrentState != GameState.Launching)
                {
                    return;
                }

                TriggerLaunchZoneVfx(forceSpaceEntry: true);
                _gameStateService.SetState(GameState.Playing);
                _asteroidSpawner.StartSpawning();
            }
            catch (OperationCanceledException)
            {
            }
        }

        public UniTask FinishGameAsync()
        {
            return FinishGameAsync(_destroyCts != null ? _destroyCts.Token : CancellationToken.None);
        }

        public async UniTask FinishGameAsync(CancellationToken token)
        {
            try
            {
                if (!IsReady())
                {
                    return;
                }

                var state = _gameStateService.CurrentState;
                if (state == GameState.Result || state == GameState.RocketDestroyed || state == GameState.Idle)
                {
                    return;
                }

                _gameStateService.SetState(GameState.RocketDestroyed);
                _asteroidSpawner.StopSpawning();
                _backgroundController?.StopScrolling();
                _cameraController?.StopFollowing();
                _uiController.HideGameplayUI();
                _vfxService.SpawnRocketExplosion(_rocketController.transform.position);
                _cameraShakeService.ShakeBig();
                await _rocketController.PlayExplosionAsync(token);

                var settleDelay = TimeSpan.FromMilliseconds(600);
                if (settleDelay > TimeSpan.Zero)
                {
                    await UniTask.Delay(settleDelay, cancellationToken: token);
                }

                if (token.IsCancellationRequested)
                {
                    return;
                }

                _gameStateService.SetState(GameState.Result);
                _uiController.ShowResultUI(_scoreService.CurrentScore);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void RestartGame()
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.path);
        }

        private void HandleRocketHit(Asteroid asteroid)
        {
            if (asteroid == null || asteroid.IsResolved)
            {
                return;
            }

            if (_gameStateService == null || _gameStateService.CurrentState != GameState.Playing)
            {
                return;
            }

            ResolveAsteroidHit(asteroid);
        }

        private void ResolveAsteroidHit(Asteroid asteroid)
        {
            if (asteroid == null || asteroid.IsResolved || _gameStateService == null || _gameStateService.CurrentState != GameState.Playing)
            {
                return;
            }

            _scoreService.AddScore(asteroid.ScoreReward);
            _vfxService.SpawnAsteroidHitVfx(asteroid.transform.position, asteroid.CurrentColor);
            _cameraShakeService.ShakeSmall();

            var flashEnabled = _config.Vfx.AsteroidHitFlashEnabled;
            var flashColor = _config.Vfx.AsteroidHitFlashColor;
            var flashDuration = _config.Vfx.AsteroidHitFlashDuration;
            asteroid.ResolveHit(flashColor, flashDuration, flashEnabled);
        }

        private void TriggerLaunchZoneVfx(bool forceSpaceEntry = false)
        {
            if (_rocketController == null || _vfxService == null || _config == null)
            {
                return;
            }

            var rocketPosition = _rocketController.transform.position;
            var fogStart = _config.LaunchFlow.FogStartHeight;
            var spaceStart = _config.LaunchFlow.SpaceStartHeight;

            if (!_fogVfxPlayed && rocketPosition.y >= fogStart)
            {
                _fogVfxPlayed = true;
                _vfxService.SpawnFogTransitionVfx(rocketPosition + Vector3.down * 0.35f);
            }

            if ((forceSpaceEntry || !_spaceEntryVfxPlayed) && rocketPosition.y >= spaceStart)
            {
                _spaceEntryVfxPlayed = true;
                _vfxService.SpawnSpaceEntryVfx(rocketPosition + Vector3.up * 0.15f);
            }
        }

        private void SubscribeToRocket()
        {
            if (_rocketController == null || _isHandlingRocketHit)
            {
                return;
            }

            _rocketController.AsteroidHit += HandleRocketHit;
            _isHandlingRocketHit = true;
        }

        private void UnsubscribeFromRocket()
        {
            if (_rocketController == null || !_isHandlingRocketHit)
            {
                return;
            }

            _rocketController.AsteroidHit -= HandleRocketHit;
            _isHandlingRocketHit = false;
        }

        private bool IsReady()
        {
            return _config != null
                   && _gameStateService != null
                   && _scoreService != null
                   && _rocketController != null
                   && _asteroidSpawner != null
                   && _uiController != null
                   && _vfxService != null
                   && _cameraShakeService != null;
        }
    }
}
