using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceFlight2D.Game
{
    [DisallowMultipleComponent]
    public sealed class PrototypeWorldAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform _backgroundRoot;
        [SerializeField] private BackgroundController _backgroundController;
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private SpriteRenderer _platformRenderer;
        [SerializeField] private Volume _globalVolume;
        [SerializeField] private RocketController _rocket;
        [SerializeField] private RocketAuthoring _rocketAuthoring;

        public Transform BackgroundRoot => _backgroundRoot;
        public BackgroundController BackgroundController => _backgroundController;
        public SpriteRenderer BackgroundRenderer => _backgroundRenderer;
        public SpriteRenderer PlatformRenderer => _platformRenderer;
        public Volume GlobalVolume => _globalVolume;
        public RocketController Rocket => _rocket;
        public RocketAuthoring RocketAuthoring => _rocketAuthoring;

        public void Bind(
            Transform backgroundRoot,
            BackgroundController backgroundController,
            SpriteRenderer backgroundRenderer,
            SpriteRenderer platformRenderer,
            Volume globalVolume,
            RocketController rocket,
            RocketAuthoring rocketAuthoring)
        {
            _backgroundRoot = backgroundRoot;
            _backgroundController = backgroundController;
            _backgroundRenderer = backgroundRenderer;
            _platformRenderer = platformRenderer;
            _globalVolume = globalVolume;
            _rocket = rocket;
            _rocketAuthoring = rocketAuthoring;
        }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Debug.LogWarning("PrototypeWorldAuthoring expects scene objects to be baked by the editor builder.", this);
        }

        public void EnsureBuilt()
        {
            if (!Application.isPlaying && (_backgroundRoot == null
                || _backgroundController == null
                || _backgroundRenderer == null
                || _platformRenderer == null
                || _globalVolume == null
                || _rocket == null
                || _rocketAuthoring == null))
            {
                Debug.LogWarning("PrototypeWorldAuthoring is missing some prefab-baked references. The scene will still be saved, but runtime behavior may be incomplete.", this);
            }
        }
    }
}
