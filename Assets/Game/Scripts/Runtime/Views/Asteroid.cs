using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;

namespace SpaceFlight2D.Game
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class Asteroid : MonoBehaviour
    {
        [SerializeField] private AsteroidAuthoring _authoring;

        private Vector2 _velocity;
        private float _rotationSpeed;
        private float _remainingLifetime;
        private bool _resolved;
        private int _activationId;
        private Action<Asteroid> _releaseAction;

        public int ScoreReward { get; private set; }
        public bool IsResolved => _resolved;
        public Color CurrentColor => _authoring?.Renderer != null ? _authoring.Renderer.color : Color.white;

        public void Bind(AsteroidAuthoring authoring)
        {
            _authoring = authoring;
        }

        private void Awake()
        {
            _authoring ??= GetComponent<AsteroidAuthoring>();
        }

        public void Spawn(AsteroidSpawnSettings settings, Transform parent, Vector3 position, Action<Asteroid> releaseAction)
        {
            _activationId++;
            _releaseAction = releaseAction;
            transform.SetParent(parent, false);
            transform.position = position;
            gameObject.SetActive(true);
            _resolved = false;
            ScoreReward = settings.ScoreReward;
            _rotationSpeed = settings.RotationSpeed;
            _remainingLifetime = settings.Lifetime;
            _velocity = Vector2.down * settings.Speed;

            _authoring?.Apply(settings);
            transform.rotation = Quaternion.identity;
        }

        public void ResolveHit(Color flashColor, float flashDuration, bool useFlash = true)
        {
            ResolveHitAsync(flashColor, flashDuration, useFlash).Forget();
        }

        public async UniTask ResolveHitAsync(Color flashColor, float flashDuration, bool useFlash = true)
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;
            var activationId = _activationId;
            if (_authoring?.Collider != null)
            {
                _authoring.Collider.enabled = false;
            }

            var renderer = _authoring?.Renderer;
            var originalColor = renderer != null ? renderer.color : Color.white;
            renderer?.DOKill();
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.22f, 0.14f, 4, 0.5f);
            if (useFlash && renderer != null)
            {
                var halfDuration = Mathf.Max(0.01f, flashDuration * 0.5f);
                renderer.DOColor(flashColor, halfDuration);
                await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
                renderer.DOColor(originalColor, halfDuration);
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0.06f, 0.14f - flashDuration)));
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.14f));
            }

            if (_activationId != activationId)
            {
                return;
            }

            ReleaseToPool();
        }

        private void Update()
        {
            if (_resolved)
            {
                return;
            }

            transform.position += (Vector3)(_velocity * Time.deltaTime);
            transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
            _remainingLifetime -= Time.deltaTime;

            if (_remainingLifetime <= 0f)
            {
                ReleaseToPool();
            }
        }

        private void OnDisable()
        {
            _authoring?.Renderer?.DOKill();
            transform.DOKill();
        }

        private void ReleaseToPool()
        {
            if (_releaseAction == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _activationId++;
            _resolved = true;
            _authoring?.Renderer?.DOKill();
            transform.DOKill();
            _releaseAction.Invoke(this);
        }
    }
}
