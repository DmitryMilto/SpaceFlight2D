using SpaceFlight2D.Game.Config;
using UnityEngine;

namespace SpaceFlight2D.Game
{
    [DisallowMultipleComponent]
    public sealed class RocketAuthoring : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _bodyRenderer;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private ParticleSystem _engineParticles;
        [SerializeField] private BoxCollider2D _hitCollider;
        [SerializeField] private Transform _visualRoot;

        public SpriteRenderer BodyRenderer => _bodyRenderer;
        public TrailRenderer TrailRenderer => _trailRenderer;
        public ParticleSystem EngineParticles => _engineParticles;
        public BoxCollider2D HitCollider => _hitCollider;
        public Transform VisualRoot => _visualRoot;

        public void Bind(
            SpriteRenderer bodyRenderer,
            TrailRenderer trailRenderer,
            ParticleSystem engineParticles,
            BoxCollider2D hitCollider,
            Transform visualRoot = null)
        {
            _bodyRenderer = bodyRenderer;
            _trailRenderer = trailRenderer;
            _engineParticles = engineParticles;
            _hitCollider = hitCollider;
            _visualRoot = visualRoot;
        }

        public void Apply(PrototypeGameConfig config)
        {
            if (config == null)
            {
                return;
            }

            EnsureReferences();

            if (_bodyRenderer != null)
            {
                if (config.Rocket.VisualSprite != null)
                {
                    _bodyRenderer.sprite = config.Rocket.VisualSprite;
                }
                else if (_bodyRenderer.sprite == null)
                {
                    _bodyRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
                }
            }

            ApplyBodyColor(config.Rocket.Color);

            if (_hitCollider != null)
            {
                _hitCollider.isTrigger = true;
                _hitCollider.size = new Vector2(0.55f, 1.5f);
            }

            if (_trailRenderer != null)
            {
                var trail = config.Rocket.Trail;
                _trailRenderer.time = trail.Time;
                _trailRenderer.widthMultiplier = 1f;
                _trailRenderer.startWidth = trail.StartWidth;
                _trailRenderer.endWidth = trail.EndWidth;
                _trailRenderer.startColor = trail.StartColor;
                _trailRenderer.endColor = trail.EndColor;
                _trailRenderer.sortingOrder = _bodyRenderer != null ? _bodyRenderer.sortingOrder - 5 : 5;
                _trailRenderer.emitting = false;
                _trailRenderer.enabled = trail.Enabled && trail.RocketEnabled;
                _trailRenderer.Clear();
            }
        }

        public void ResetToConfig(PrototypeGameConfig config)
        {
            EnsureReferences();
            Apply(config);

            if (_engineParticles != null)
            {
                _engineParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void EnableTrail()
        {
            EnsureReferences();
            if (_trailRenderer == null)
            {
                return;
            }

            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
            _trailRenderer.enabled = true;
        }

        public void DisableTrail()
        {
            EnsureReferences();
            if (_trailRenderer == null)
            {
                return;
            }

            _trailRenderer.emitting = false;
            _trailRenderer.Clear();
        }

        public void PlayEngine()
        {
            EnsureReferences();
            _engineParticles?.Play();
        }

        public void StopEngine(bool clear = true)
        {
            EnsureReferences();
            if (_engineParticles == null)
            {
                return;
            }

            _engineParticles.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }

        public void SetVisualsVisible(bool isVisible)
        {
            EnsureReferences();
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = isVisible;
            }
        }

        private void ApplyBodyColor(Color color)
        {
            EnsureReferences();
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].color = color;
            }
        }

        private void EnsureReferences()
        {
            _bodyRenderer ??= GetComponentInChildren<SpriteRenderer>(true);
            _trailRenderer ??= GetComponentInChildren<TrailRenderer>(true);
            _engineParticles ??= GetComponentInChildren<ParticleSystem>(true);
            _hitCollider ??= GetComponent<BoxCollider2D>();

            if (_visualRoot == null)
            {
                var visualRootTransform = transform.Find("VisualRoot");
                if (visualRootTransform != null)
                {
                    _visualRoot = visualRootTransform;
                }
                else if (_bodyRenderer != null && _bodyRenderer.transform.parent != null)
                {
                    _visualRoot = _bodyRenderer.transform.parent;
                }
            }
        }
    }
}
