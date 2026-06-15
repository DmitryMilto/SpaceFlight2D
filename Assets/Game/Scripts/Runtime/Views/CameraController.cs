using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;
using Zenject;

namespace SpaceFlight2D.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour, IInitializable
    {
        [SerializeField] private Camera _camera;
        private PrototypeGameConfig _config;
        private IGameStateService _gameStateService;
        private RocketController _rocketController;
        private bool _isFollowing;
        private bool _initialized;

        [Inject]
        public void Construct(
            PrototypeGameConfig config,
            IGameStateService gameStateService,
            RocketController rocketController)
        {
            _config = config;
            _gameStateService = gameStateService;
            _rocketController = rocketController;
        }

        public void Bind(Camera camera)
        {
            _camera = camera;
        }

        private void Awake()
        {
            _camera ??= GetComponent<Camera>();
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (_config == null || _gameStateService == null || _rocketController == null)
            {
                Debug.LogError("CameraController was initialized before dependencies were injected.", this);
                return;
            }

            if (_gameStateService != null)
            {
                _gameStateService.StateChanged += HandleStateChanged;
            }
            ApplyConfig();
            ResetCamera();
            _initialized = true;
        }

        private void LateUpdate()
        {
            if (!_isFollowing || _rocketController == null || _camera == null || _config == null)
            {
                return;
            }

            var targetPosition = _rocketController.transform.position + _config.Camera.Offset;
            targetPosition.z = _camera.transform.position.z;
            var smoothness = Mathf.Max(0.01f, _config.Camera.FollowSmoothness);
            var lerpFactor = 1f - Mathf.Exp(-smoothness * Time.deltaTime);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPosition, lerpFactor);
        }

        private void OnDestroy()
        {
            if (_gameStateService != null)
            {
                _gameStateService.StateChanged -= HandleStateChanged;
            }

            _initialized = false;
        }

        public void ApplyConfig()
        {
            if (_camera == null || _config == null)
            {
                return;
            }

            _camera.orthographic = true;
            _camera.orthographicSize = _config.Camera.OrthographicSize;
        }

        public void ResetCamera()
        {
            if (_camera == null)
            {
                return;
            }

            ApplyConfig();
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _isFollowing = false;
        }

        public void FollowRocket()
        {
            _isFollowing = true;
        }

        public void StopFollowing()
        {
            _isFollowing = false;
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                case GameState.Launching:
                    FollowRocket();
                    break;
                case GameState.Result:
                case GameState.RocketDestroyed:
                case GameState.Idle:
                    StopFollowing();
                    break;
            }
        }
    }
}
