using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game;
using UnityEngine;

namespace SpaceFlight2D.Game.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSystemsAuthoring : MonoBehaviour
    {
        [SerializeField] private PrototypeSceneReferences _sceneReferences;
        [SerializeField] private GameInstaller _installer;
        [SerializeField] private CreativeSceneCustomizer _customizer;
        [SerializeField] private GameplayLoop _gameplayLoop;
        [SerializeField] private GameBootstrapper _gameBootstrapper;
        [SerializeField] private VfxService _vfxService;
        [SerializeField] private CameraShakeService _cameraShakeService;
        [SerializeField] private AsteroidSpawner _spawner;
        [SerializeField] private Transform _vfxRoot;

        public PrototypeSceneReferences SceneReferences => _sceneReferences;
        public GameInstaller Installer => _installer;
        public CreativeSceneCustomizer Customizer => _customizer;
        public GameplayLoop GameplayLoop => _gameplayLoop;
        public GameBootstrapper GameBootstrapper => _gameBootstrapper;
        public VfxService VfxService => _vfxService;
        public CameraShakeService CameraShakeService => _cameraShakeService;
        public AsteroidSpawner Spawner => _spawner;
        public Transform VfxRoot => _vfxRoot;

        public void Bind(
            PrototypeSceneReferences sceneReferences,
            GameInstaller installer,
            CreativeSceneCustomizer customizer,
            GameplayLoop gameplayLoop,
            GameBootstrapper gameBootstrapper,
            VfxService vfxService,
            CameraShakeService cameraShakeService,
            AsteroidSpawner spawner,
            Transform vfxRoot)
        {
            _sceneReferences = sceneReferences;
            _installer = installer;
            _customizer = customizer;
            _gameplayLoop = gameplayLoop;
            _gameBootstrapper = gameBootstrapper;
            _vfxService = vfxService;
            _cameraShakeService = cameraShakeService;
            _spawner = spawner;
            _vfxRoot = vfxRoot;
        }

        private void Awake()
        {
            if (Application.isPlaying && (_installer == null || _customizer == null || _gameplayLoop == null || _gameBootstrapper == null))
            {
                Debug.LogError("PrototypeSystemsAuthoring expects scene objects to be baked by the editor builder.", this);
            }
        }

        public void EnsureBuilt(PrototypeGameConfig config)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_sceneReferences == null
                || _installer == null
                || _customizer == null
                || _gameplayLoop == null
                || _gameBootstrapper == null
                || _vfxService == null
                || _cameraShakeService == null
                || _spawner == null
                || _vfxRoot == null)
            {
                throw new System.InvalidOperationException("PrototypeSystemsAuthoring requires the prefab factory to bind all references before baking.");
            }

            _installer.Bind(_sceneReferences);
            _customizer.Bind(config, _sceneReferences, true, true, true);
        }
    }
}
