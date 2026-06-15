using System.Collections.Generic;
using UnityEngine;

namespace SpaceFlight2D.Game
{
    [DisallowMultipleComponent]
    public sealed class BackgroundLayer : MonoBehaviour
    {
        [SerializeField] private Transform _tileA;
        [SerializeField] private Transform _tileB;
        [SerializeField] private float _scrollSpeed = 1f;
        [SerializeField] private int _starCount = 24;
        [SerializeField] private Vector2 _starScaleRange = new(0.05f, 0.1f);
        [SerializeField] private Vector2 _starAlphaRange = new(0.5f, 0.8f);
        [SerializeField] private Color _starColor = Color.white;
        [SerializeField] private int _sortingOrder = 0;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Vector2 _positionXRange;
        [SerializeField] private Vector2 _positionYRange;
        [SerializeField] private Vector2 _backgroundSize;
        [SerializeField] private bool _randomRotation;
        [SerializeField] private bool _built;

        [SerializeField] private List<SpriteRenderer> _spawnedRenderers = new();
        [SerializeField] private List<Color> _baseColors = new();
        private Color _tint = Color.white;
        private float _opacity = 1f;

        private void Awake()
        {
            if (Application.isPlaying && _tileA != null && _tileB != null)
            {
                _built = true;
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying && _tileA != null && _tileB != null)
            {
                _built = true;
            }
        }

        public void ResetTiles()
        {
            if (_tileA != null)
            {
                _tileA.localPosition = Vector3.zero;
            }

            if (_tileB != null)
            {
                _tileB.localPosition = Vector3.up * _backgroundSize.y;
            }
        }

        public void Scroll(float deltaTime, float cameraY, float backgroundHeight, bool enabled)
        {
            if (!_built && _tileA != null && _tileB != null)
            {
                _built = true;
            }

            if (!_built || !enabled)
            {
                return;
            }

            var delta = _scrollSpeed * deltaTime;
            MoveTile(_tileA, delta, cameraY, backgroundHeight);
            MoveTile(_tileB, delta, cameraY, backgroundHeight);
        }

        public void SetTint(Color tint)
        {
            _tint = tint;
            ApplyAppearance();
        }

        public void SetOpacity(float opacity)
        {
            _opacity = Mathf.Clamp01(opacity);
            ApplyAppearance();
        }

        private void ApplyAppearance()
        {
            for (var i = 0; i < _spawnedRenderers.Count; i++)
            {
                var renderer = _spawnedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                var baseColor = _baseColors[i];
                renderer.color = new Color(
                    baseColor.r * _tint.r,
                    baseColor.g * _tint.g,
                    baseColor.b * _tint.b,
                    baseColor.a * _tint.a * _opacity);
            }
        }

#if UNITY_EDITOR
        public void Build(
            Vector2 backgroundSize,
            float scrollSpeed,
            int starCount,
            Vector2 starScaleRange,
            Vector2 starAlphaRange,
            Color starColor,
            int sortingOrder)
        {
            Build(
                backgroundSize,
                scrollSpeed,
                starCount,
                starScaleRange,
                starAlphaRange,
                starColor,
                sortingOrder,
                PrimitiveSpriteLibrary.CircleSprite,
                new Vector2(-backgroundSize.x * 0.5f, backgroundSize.x * 0.5f),
                new Vector2(-backgroundSize.y * 0.5f, backgroundSize.y * 0.5f),
                false);
        }

        public void Build(
            Vector2 backgroundSize,
            float scrollSpeed,
            int objectCount,
            Vector2 objectScaleRange,
            Vector2 objectAlphaRange,
            Color objectColor,
            int sortingOrder,
            Sprite sprite,
            Vector2 positionXRange,
            Vector2 positionYRange,
            bool randomRotation)
        {
            _backgroundSize = backgroundSize;
            _scrollSpeed = scrollSpeed;
            _starCount = objectCount;
            _starScaleRange = objectScaleRange;
            _starAlphaRange = objectAlphaRange;
            _starColor = objectColor;
            _sortingOrder = sortingOrder;
            _sprite = sprite != null ? sprite : PrimitiveSpriteLibrary.CircleSprite;
            _positionXRange = positionXRange;
            _positionYRange = positionYRange;
            _randomRotation = randomRotation;

            EnsureTiles();
            RebuildObjects();
            ResetTiles();
            _built = true;
        }

        private void EnsureTiles()
        {
            _tileA ??= CreateTile("TileA");
            _tileB ??= CreateTile("TileB");
        }

        private Transform CreateTile(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void RebuildObjects()
        {
            ClearSpawnedObjects();
            CreateObjectsForTile(_tileA, 1);
            CreateObjectsForTile(_tileB, 2);
        }

        private void ClearSpawnedObjects()
        {
            for (var i = 0; i < _spawnedRenderers.Count; i++)
            {
                if (_spawnedRenderers[i] != null)
                {
                    DestroyImmediateSafe(_spawnedRenderers[i].gameObject);
                }
            }

            _spawnedRenderers.Clear();
            _baseColors.Clear();
        }

        private void CreateObjectsForTile(Transform tile, int seedOffset)
        {
            if (tile == null)
            {
                return;
            }

            var state = Random.state;
            Random.InitState(GetInstanceID() + seedOffset);

            for (var i = 0; i < _starCount; i++)
            {
                var item = new GameObject($"{tile.name}_{i:D2}");
                item.transform.SetParent(tile, false);
                item.transform.localPosition = new Vector3(
                    Random.Range(_positionXRange.x, _positionXRange.y),
                    Random.Range(_positionYRange.x, _positionYRange.y),
                    0f);

                var renderer = item.AddComponent<SpriteRenderer>();
                renderer.sprite = _sprite;
                renderer.sortingOrder = _sortingOrder;

                var scale = Random.Range(_starScaleRange.x, _starScaleRange.y);
                item.transform.localScale = Vector3.one * scale;

                var alpha = Random.Range(_starAlphaRange.x, _starAlphaRange.y);
                renderer.color = new Color(_starColor.r, _starColor.g, _starColor.b, alpha);
                _spawnedRenderers.Add(renderer);
                _baseColors.Add(renderer.color);

                if (_randomRotation)
                {
                    item.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                }
            }

            Random.state = state;
        }

        private static void DestroyImmediateSafe(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            Destroy(obj);
        }
#endif

        private static void MoveTile(Transform tile, float delta, float cameraY, float backgroundHeight)
        {
            if (tile == null)
            {
                return;
            }

            tile.position += Vector3.down * delta;

            if (tile.position.y < cameraY - backgroundHeight)
            {
                tile.position += Vector3.up * backgroundHeight * 2f;
            }
        }

    }
}
