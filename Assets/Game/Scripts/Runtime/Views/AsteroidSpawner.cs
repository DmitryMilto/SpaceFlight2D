using System.Threading;
using Cysharp.Threading.Tasks;
using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;
using Zenject;

namespace SpaceFlight2D.Game
{
    public sealed class AsteroidSpawner : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform _asteroidRoot;
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private Asteroid[] _pooledAsteroids;

        private PrototypeGameConfig _config;
        private IGameStateService _gameStateService;
        private IAsteroidSpawnSettingsProvider _spawnSettingsProvider;
        private CancellationTokenSource _spawnCts;
        private AsteroidPool _asteroidPool;
        private bool _initialized;

        [Inject]
        public void Construct(
            PrototypeGameConfig config,
            IGameStateService gameStateService,
            IAsteroidSpawnSettingsProvider spawnSettingsProvider)
        {
            _config = config;
            _gameStateService = gameStateService;
            _spawnSettingsProvider = spawnSettingsProvider;
        }

        public void Bind(Transform asteroidRoot, Camera targetCamera, Asteroid[] pooledAsteroids)
        {
            _asteroidRoot = asteroidRoot;
            _targetCamera = targetCamera;
            _pooledAsteroids = pooledAsteroids;
        }

        private void Awake()
        {
            _asteroidRoot ??= transform;
            _targetCamera ??= Camera.main;
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (_config == null)
            {
                Debug.LogError("AsteroidSpawner was initialized before dependencies were injected.", this);
                return;
            }

            EnsurePool();
            if (_asteroidPool == null)
            {
                return;
            }

            _asteroidPool.Prewarm();

            if (_gameStateService != null)
            {
                _gameStateService.StateChanged += HandleStateChanged;
            }

            _initialized = true;
        }

        private void OnDestroy()
        {
            if (_gameStateService != null)
            {
                _gameStateService.StateChanged -= HandleStateChanged;
            }

            StopSpawnLoop();
            _initialized = false;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                StartSpawnLoop();
                return;
            }

            if (state == GameState.RocketDestroyed || state == GameState.Result || state == GameState.Idle)
            {
                StopSpawnLoop();
            }
        }

        public void StartSpawning()
        {
            StartSpawnLoop();
        }

        public void StopSpawning()
        {
            StopSpawnLoop();
        }

        private void StartSpawnLoop()
        {
            if (_spawnCts != null)
            {
                return;
            }

            EnsurePool();
            if (_asteroidPool == null)
            {
                return;
            }

            _spawnCts = new CancellationTokenSource();
            SpawnLoop(_spawnCts.Token).Forget();
        }

        private void StopSpawnLoop()
        {
            _spawnCts?.Cancel();
            _spawnCts?.Dispose();
            _spawnCts = null;
        }

        private async UniTaskVoid SpawnLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                SpawnAsteroid();

                var delay = Random.Range(_config.Asteroids.SpawnIntervalMin, _config.Asteroids.SpawnIntervalMax);
                delay /= Mathf.Max(0.01f, _config.Recording.GameSpeedMultiplier);
                await UniTask.Delay((int)(delay * 1000f), cancellationToken: token);
            }
        }

        private void SpawnAsteroid()
        {
            EnsurePool();
            if (_asteroidPool == null)
            {
                return;
            }

            var spawnY = (_targetCamera != null
                ? _targetCamera.transform.position.y + _targetCamera.orthographicSize + _config.Asteroids.SpawnOffsetY
                : _config.Asteroids.SpawnOffsetY);

            var position = new Vector3(
                Random.Range(_config.Asteroids.SpawnXRange.x, _config.Asteroids.SpawnXRange.y),
                spawnY,
                0f);

            var asteroid = _asteroidPool.Rent();
            if (asteroid == null)
            {
                return;
            }

            asteroid.Spawn(_spawnSettingsProvider.GetNextSpawnSettings(), _asteroidRoot, position, HandleAsteroidReleased);
        }

        private void HandleAsteroidReleased(Asteroid asteroid)
        {
            if (asteroid == null)
            {
                return;
            }

            _asteroidPool.Return(asteroid);
        }

        private void EnsurePool()
        {
            if (_asteroidPool != null)
            {
                return;
            }

            if (_asteroidRoot == null)
            {
                Debug.LogError("AsteroidSpawner is missing asteroid root.", this);
                return;
            }

            _asteroidPool = new AsteroidPool(_pooledAsteroids, _asteroidRoot);
        }
    }
}
