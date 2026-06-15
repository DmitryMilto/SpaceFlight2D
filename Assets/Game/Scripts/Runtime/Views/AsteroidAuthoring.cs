using SpaceFlight2D.Game.Runtime;
using UnityEngine;

namespace SpaceFlight2D.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class AsteroidAuthoring : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private Transform _visualRoot;

        public SpriteRenderer Renderer => _renderer;
        public CircleCollider2D Collider => _collider;
        public Transform VisualRoot => _visualRoot;

        public void Bind(SpriteRenderer renderer, CircleCollider2D collider, Transform visualRoot = null)
        {
            _renderer = renderer;
            _collider = collider;
            _visualRoot = visualRoot;
        }

        public void Apply(AsteroidSpawnSettings settings)
        {
            if (settings.Size == Vector2.zero)
            {
                return;
            }

            EnsureReferences();

            if (_renderer != null)
            {
                if (settings.VisualSprite != null)
                {
                    _renderer.sprite = settings.VisualSprite;
                }
                else if (_renderer.sprite == null)
                {
                    _renderer.sprite = PrimitiveSpriteLibrary.CircleSprite;
                }

                ApplyBodyColor(settings.Color);
                _renderer.enabled = true;
            }

            transform.localScale = Vector3.one;

            if (_visualRoot != null)
            {
                _visualRoot.localScale = new Vector3(settings.Size.x, settings.Size.y, 1f);
            }

            if (_collider != null)
            {
                _collider.enabled = true;
                _collider.isTrigger = true;
                _collider.offset = Vector2.zero;
                _collider.radius = Mathf.Max(settings.Size.x, settings.Size.y) * 0.5f;
            }
        }

        public void ResetToDefault()
        {
            EnsureReferences();

            if (_renderer != null)
            {
                _renderer.enabled = true;
            }

            transform.localScale = Vector3.one;

            if (_collider != null)
            {
                _collider.enabled = true;
                _collider.isTrigger = true;
                _collider.offset = Vector2.zero;
                _collider.radius = 0.5f;
            }
        }

        private void EnsureReferences()
        {
            _renderer ??= GetComponentInChildren<SpriteRenderer>();
            _collider ??= GetComponent<CircleCollider2D>();

            if (_visualRoot == null)
            {
                if (_renderer != null && _renderer.transform.parent != null)
                {
                    _visualRoot = _renderer.transform.parent;
                }
                else
                {
                    var visualRootTransform = transform.Find("VisualRoot");
                    if (visualRootTransform != null)
                    {
                        _visualRoot = visualRootTransform;
                    }
                }
            }
        }

        private void ApplyBodyColor(Color color)
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].color = color;
            }
        }
    }
}
