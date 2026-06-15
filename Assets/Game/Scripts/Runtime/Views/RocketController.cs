using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SpaceFlight2D.Game.Bootstrap;
using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;
using Zenject;

namespace SpaceFlight2D.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class RocketController : MonoBehaviour
    {
        [SerializeField] private RocketAuthoring _authoring;
        [SerializeField] private Rigidbody2D _rigidbody;

        private PrototypeGameConfig _config;
        private IGameStateService _gameStateService;
        private UIController _uiController;
        private PrototypeSceneReferences _sceneReferences;
        private Vector3 _startPosition;
        private float _steerInput;
        private bool _isDestroyed;
        private bool _isLaunchSequenceComplete;

        public event Action<Asteroid> AsteroidHit;

        [Inject]
        public void Construct(
            PrototypeGameConfig config,
            IGameStateService gameStateService,
            UIController uiController,
            PrototypeSceneReferences sceneReferences)
        {
            _config = config;
            _gameStateService = gameStateService;
            _uiController = uiController;
            _sceneReferences = sceneReferences;
        }

        public void Bind(Rigidbody2D rigidbody, RocketAuthoring authoring)
        {
            _rigidbody = rigidbody;
            _authoring = authoring;
            _startPosition = transform.position;
        }

        private void Awake()
        {
            _rigidbody ??= GetComponent<Rigidbody2D>();
            _authoring ??= GetComponent<RocketAuthoring>();
            _startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (_gameStateService == null || _rigidbody == null || _config == null)
            {
                return;
            }

            var state = _gameStateService.CurrentState;
            if (state != GameState.Playing && state != GameState.Launching)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
                return;
            }

            if (state == GameState.Launching && !_isLaunchSequenceComplete)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
                return;
            }

            var multiplier = Mathf.Max(0.01f, _config.Recording.GameSpeedMultiplier);
            var steerInput = state == GameState.Playing
                ? (_uiController != null ? _uiController.GetHorizontalInput() : _steerInput)
                : 0f;
            _steerInput = steerInput;
            var targetAngle = -steerInput * _config.Rocket.MaxRotationAngle;
            var currentAngle = NormalizeAngle(transform.eulerAngles.z);
            var nextAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                (steerInput == 0f ? _config.Rocket.AutoStabilizeSpeed : _config.Rocket.RotationSpeed) * Time.fixedDeltaTime * multiplier);

            _rigidbody.MoveRotation(nextAngle);

            var verticalSpeed = _config.Rocket.Speed * multiplier;
            var horizontalSpeed = (state == GameState.Playing && steerInput != 0f)
                ? steerInput * _config.Rocket.HorizontalSpeed * multiplier
                : 0f;
            _rigidbody.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);

            ClampToViewport();
        }

        public void SetSteerInput(float value)
        {
            _steerInput = Mathf.Clamp(value, -1f, 1f);
        }

        public void ApplyVisuals(PrototypeGameConfig config)
        {
            _authoring?.Apply(config);
        }

        public void ResetRocket()
        {
            _isDestroyed = false;
            _isLaunchSequenceComplete = false;
            _steerInput = 0f;
            transform.DOKill();
            transform.position = _startPosition;
            transform.rotation = Quaternion.identity;
            var rocketSize = _config != null ? _config.Rocket.Size : new Vector2(0.8f, 1.8f);
            transform.localScale = new Vector3(rocketSize.x, rocketSize.y, 1f);

            if (_rigidbody != null)
            {
                _rigidbody.simulated = true;
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
            }

            _authoring?.SetVisualsVisible(true);

            if (_authoring?.HitCollider != null)
            {
                _authoring.HitCollider.enabled = true;
            }

            if (_config != null)
            {
                _authoring?.ResetToConfig(_config);
                ApplyVisuals(_config);
            }
        }

        public void ResetState()
        {
            ResetRocket();
        }

        public UniTask LaunchAsync(CancellationToken token)
        {
            return PlayLaunchSequenceAsync(token);
        }

        public async UniTask PlayLaunchSequenceAsync(CancellationToken token)
        {
            _authoring?.EnableTrail();
            _authoring?.PlayEngine();
            var punchScale = _config != null ? _config.StartFlow.LaunchPunchScale : 0.12f;
            var punchDuration = _config != null ? _config.StartFlow.LaunchPunchDuration : 0.2f;
            var launchDuration = _config != null ? _config.StartFlow.LaunchAccelerationDuration : 0.45f;
            transform.DOPunchScale(new Vector3(punchScale, punchScale, 0f), punchDuration, 4, 0.6f);
            var target = _startPosition + Vector3.up * 0.7f;
            transform.DOMove(target, launchDuration).SetEase(Ease.OutSine);
            await UniTask.Delay(TimeSpan.FromSeconds(launchDuration), cancellationToken: token);
            _isLaunchSequenceComplete = true;
        }

        public async UniTask PlayExplosionAsync(CancellationToken token)
        {
            if (_isDestroyed)
            {
                return;
            }

            _isDestroyed = true;
            transform.DOKill();
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.simulated = false;
            _authoring?.DisableTrail();
            _authoring?.StopEngine(false);
            transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.2f, 4, 0.5f);

            _authoring?.BodyRenderer?.DOColor(new Color(1f, 0.35f, 0.25f, 1f), 0.1f);

            if (_authoring?.HitCollider != null)
            {
                _authoring.HitCollider.enabled = false;
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(220), cancellationToken: token);

            _authoring?.SetVisualsVisible(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isDestroyed || _gameStateService == null || _gameStateService.CurrentState != GameState.Playing)
            {
                return;
            }

            if (other.TryGetComponent(out Asteroid asteroid))
            {
                AsteroidHit?.Invoke(asteroid);
            }
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }

            return angle;
        }

        private void ClampToViewport()
        {
            var camera = _sceneReferences.MainCamera;
            var halfWidth = camera.orthographicSize * camera.aspect;
            var maxX = halfWidth - (_config.Rocket.Size.x * 0.5f) - 0.15f;
            var position = _rigidbody.position;
            position.x = Mathf.Clamp(position.x, -maxX, maxX);
            _rigidbody.position = position;
        }
    }
}
