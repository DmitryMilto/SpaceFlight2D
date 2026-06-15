using SpaceFlight2D.Game.Config;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using SpaceFlight2D.Game;

namespace SpaceFlight2D.Game.Bootstrap
{
    public sealed class PrototypeSceneReferences : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform _gameRoot;
        [SerializeField] private Transform _asteroidRoot;
        [SerializeField] private Transform _vfxRoot;
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private RocketController _rocket;
        [SerializeField] private AsteroidSpawner _spawner;
        [SerializeField] private VfxService _vfxService;
        [SerializeField] private CameraShakeService _cameraShakeService;
        [SerializeField] private UIController _uiController;
        [SerializeField] private BackgroundController _backgroundController;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private GameBootstrapper _gameBootstrapper;
        [SerializeField] private GameplayLoop _gameplayLoop;
        [SerializeField] private CreativeSceneCustomizer _customizer;
        [SerializeField] private GameInstaller _installer;
        [SerializeField] private SpriteRenderer _platformRenderer;
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private Volume _globalVolume;
        [SerializeField] private PrototypeGameConfig _config;

        public Camera MainCamera => _mainCamera;
        public Transform GameRoot => _gameRoot;
        public Transform AsteroidRoot => _asteroidRoot;
        public Transform VfxRoot => _vfxRoot;
        public Transform UiRoot => _uiRoot;
        public RocketController Rocket => _rocket;
        public AsteroidSpawner Spawner => _spawner;
        public VfxService VfxService => _vfxService;
        public CameraShakeService CameraShakeService => _cameraShakeService;
        public UIController UiController => _uiController;
        public BackgroundController BackgroundController => _backgroundController;
        public CameraController CameraController => _cameraController;
        public GameBootstrapper GameBootstrapper => _gameBootstrapper;
        public GameplayLoop GameplayLoop => _gameplayLoop;
        public CreativeSceneCustomizer Customizer => _customizer;
        public GameInstaller Installer => _installer;
        public SpriteRenderer PlatformRenderer => _platformRenderer;
        public SpriteRenderer BackgroundRenderer => _backgroundRenderer;
        public Volume GlobalVolume => _globalVolume;
        public PrototypeGameConfig Config => _config;

        public void SetRocket(RocketController rocket)
        {
            _rocket = rocket;
        }

        public void SetConfig(PrototypeGameConfig config)
        {
            _config = config;
        }

        public void SetCustomizer(CreativeSceneCustomizer customizer)
        {
            _customizer = customizer;
        }

        public void Bind(
            Camera mainCamera,
            Transform gameRoot,
            Transform asteroidRoot,
            Transform vfxRoot,
            Transform uiRoot,
            RocketController rocket,
            AsteroidSpawner spawner,
            VfxService vfxService,
            CameraShakeService cameraShakeService,
            UIController uiController,
            BackgroundController backgroundController,
            CameraController cameraController,
            GameBootstrapper gameBootstrapper,
            GameplayLoop gameplayLoop,
            CreativeSceneCustomizer customizer,
            GameInstaller installer,
            SpriteRenderer platformRenderer,
            SpriteRenderer backgroundRenderer,
            Volume globalVolume,
            PrototypeGameConfig config)
        {
            _mainCamera = mainCamera;
            _gameRoot = gameRoot;
            _asteroidRoot = asteroidRoot;
            _vfxRoot = vfxRoot;
            _uiRoot = uiRoot;
            _rocket = rocket;
            _spawner = spawner;
            _vfxService = vfxService;
            _cameraShakeService = cameraShakeService;
            _uiController = uiController;
            _backgroundController = backgroundController;
            _cameraController = cameraController;
            _gameBootstrapper = gameBootstrapper;
            _gameplayLoop = gameplayLoop;
            _customizer = customizer;
            _installer = installer;
            _platformRenderer = platformRenderer;
            _backgroundRenderer = backgroundRenderer;
            _globalVolume = globalVolume;
            _config = config;
        }

        public void ValidateOrThrow()
        {
            var missingFields = new List<string>();

            Require(_mainCamera, nameof(_mainCamera), missingFields);
            Require(_gameRoot, nameof(_gameRoot), missingFields);
            Require(_asteroidRoot, nameof(_asteroidRoot), missingFields);
            Require(_vfxRoot, nameof(_vfxRoot), missingFields);
            Require(_uiRoot, nameof(_uiRoot), missingFields);
            Require(_rocket, nameof(_rocket), missingFields);
            Require(_spawner, nameof(_spawner), missingFields);
            Require(_vfxService, nameof(_vfxService), missingFields);
            Require(_cameraShakeService, nameof(_cameraShakeService), missingFields);
            Require(_uiController, nameof(_uiController), missingFields);
            Require(_backgroundController, nameof(_backgroundController), missingFields);
            Require(_cameraController, nameof(_cameraController), missingFields);
            Require(_gameBootstrapper, nameof(_gameBootstrapper), missingFields);
            Require(_gameplayLoop, nameof(_gameplayLoop), missingFields);
            Require(_customizer, nameof(_customizer), missingFields);
            Require(_installer, nameof(_installer), missingFields);
            Require(_platformRenderer, nameof(_platformRenderer), missingFields);
            Require(_backgroundRenderer, nameof(_backgroundRenderer), missingFields);
            Require(_config, nameof(_config), missingFields);

            if (missingFields.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                $"PrototypeSceneReferences on '{name}' is missing required references: {string.Join(", ", missingFields)}");
        }

        private static void Require(UnityEngine.Object value, string fieldName, ICollection<string> missingFields)
        {
            if (value == null)
            {
                missingFields.Add(fieldName);
            }
        }
    }
}
