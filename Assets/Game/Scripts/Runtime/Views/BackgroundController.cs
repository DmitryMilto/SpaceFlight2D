using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace SpaceFlight2D.Game
{
    [DisallowMultipleComponent]
    public sealed class BackgroundController : MonoBehaviour, IInitializable
    {
        [SerializeField] private SpriteRenderer _spaceGradient;
        [SerializeField] private SpriteRenderer _upperAtmosphereGlow;
        [SerializeField] private SpriteRenderer _planetCurvatureGlow;
        [SerializeField] private BackgroundLayer _farStarsLayer;
        [SerializeField] private BackgroundLayer _middleStarsLayer;
        [SerializeField] private BackgroundLayer _nearStarsLayer;
        [SerializeField] private BackgroundLayer _cloudFarLayer;
        [SerializeField] private BackgroundLayer _cloudNearLayer;
        [SerializeField] private BackgroundLayer _nebulaLayer;
        [SerializeField] private BackgroundLayer _launchSmokeLayer;
        [SerializeField] private ParticleSystem _spaceStarsParticles;

        private PrototypeGameConfig _config;
        private IGameStateService _gameStateService;
        private IVfxService _vfxService;
        private RocketController _rocketController;
        [SerializeField] private Camera _targetCamera;
        private Volume _globalVolume;
        private VolumeProfile _runtimeVolumeProfile;
        private Bloom _bloom;
        private ColorAdjustments _colorAdjustments;
        private Vignette _vignette;
        private ChromaticAberration _chromaticAberration;
        private bool _scrollingEnabled;
        [SerializeField] private bool _built;
        private bool _initialized;
        private Sprite _gradientSprite;
        private Texture2D _gradientTexture;
        private Sprite _upperAtmosphereSprite;
        private Texture2D _upperAtmosphereTexture;
        private Sprite _planetCurvatureSprite;
        private Texture2D _planetCurvatureTexture;
        private LaunchStage _currentStage = LaunchStage.Sky;
        private LaunchStage _fromStage = LaunchStage.Sky;
        private LaunchStage _toStage = LaunchStage.Sky;
        private float _transitionStartTime;
        private float _fogVfxNextTime;

        public SpriteRenderer BackgroundRenderer => _spaceGradient;
        public ParticleSystem SpaceStarsParticles => _spaceStarsParticles;

        [Inject]
        public void Construct(
            PrototypeGameConfig config,
            IGameStateService gameStateService,
            IVfxService vfxService,
            RocketController rocketController,
            Volume globalVolume)
        {
            _config = config;
            _gameStateService = gameStateService;
            _vfxService = vfxService;
            _rocketController = rocketController;
            _globalVolume = globalVolume;
        }

        public void Bind(Camera targetCamera)
        {
            _targetCamera = targetCamera;
        }

        public void Bind(
            SpriteRenderer spaceGradient,
            SpriteRenderer upperAtmosphereGlow,
            SpriteRenderer planetCurvatureGlow,
            BackgroundLayer farStarsLayer,
            BackgroundLayer middleStarsLayer,
            BackgroundLayer nearStarsLayer,
            BackgroundLayer cloudFarLayer,
            BackgroundLayer cloudNearLayer,
            BackgroundLayer nebulaLayer,
            BackgroundLayer launchSmokeLayer,
            ParticleSystem spaceStarsParticles)
        {
            _spaceGradient = spaceGradient;
            _upperAtmosphereGlow = upperAtmosphereGlow;
            _planetCurvatureGlow = planetCurvatureGlow;
            _farStarsLayer = farStarsLayer;
            _middleStarsLayer = middleStarsLayer;
            _nearStarsLayer = nearStarsLayer;
            _cloudFarLayer = cloudFarLayer;
            _cloudNearLayer = cloudNearLayer;
            _nebulaLayer = nebulaLayer;
            _launchSmokeLayer = launchSmokeLayer;
            _spaceStarsParticles = spaceStarsParticles;
        }

        public void SetSpaceStarsParticles(ParticleSystem spaceStarsParticles)
        {
            _spaceStarsParticles = spaceStarsParticles;
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EnsureBuilt();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _built = HasBakedLayers();
            if (_built)
            {
                ResetLayerPositions();
            }
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (_config == null || _gameStateService == null)
            {
                Debug.LogError("BackgroundController was initialized before dependencies were injected.", this);
                return;
            }

            if (_gameStateService != null)
            {
                _gameStateService.StateChanged += HandleStateChanged;
            }

            _built = _built || HasBakedLayers();
            _scrollingEnabled = _gameStateService.CurrentState == GameState.Playing
                && _config.Background.ScrollingEnabled;
            _currentStage = LaunchStage.Sky;
            _fromStage = LaunchStage.Sky;
            _toStage = LaunchStage.Sky;
            _fogVfxNextTime = 0f;
            EnsureVolumeProfile();
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (_gameStateService != null)
            {
                _gameStateService.StateChanged -= HandleStateChanged;
            }

            DestroyGeneratedTexture(_gradientTexture);
            DestroyGeneratedTexture(_upperAtmosphereTexture);
            DestroyGeneratedTexture(_planetCurvatureTexture);
            if (_runtimeVolumeProfile != null && _runtimeVolumeProfile != _globalVolume?.sharedProfile)
            {
                Destroy(_runtimeVolumeProfile);
            }
            _initialized = false;
        }

        private void LateUpdate()
        {
            if (!_built || _config == null)
            {
                return;
            }

            if (_scrollingEnabled)
            {
                var camera = _targetCamera != null ? _targetCamera : Camera.main;
                if (camera == null)
                {
                    UpdateLaunchVisuals();
                    return;
                }

                var deltaTime = Time.deltaTime;
                var cameraY = camera.transform.position.y;
                var backgroundHeight = _config.Background.Size.y;

                _farStarsLayer?.Scroll(deltaTime, cameraY, backgroundHeight, true);
                _middleStarsLayer?.Scroll(deltaTime, cameraY, backgroundHeight, true);
                _nearStarsLayer?.Scroll(deltaTime, cameraY, backgroundHeight, true);
                _cloudFarLayer?.Scroll(deltaTime, cameraY, backgroundHeight, true);
                _cloudNearLayer?.Scroll(deltaTime, cameraY, backgroundHeight, true);
                _nebulaLayer?.Scroll(deltaTime, cameraY, backgroundHeight, true);
                _launchSmokeLayer?.Scroll(deltaTime, cameraY, backgroundHeight, true);
                UpdateSpaceStarsAnchor(camera.transform.position);
            }

            UpdateLaunchVisuals();
        }

        public void EnsureBuilt()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_spaceGradient == null
                || _upperAtmosphereGlow == null
                || _planetCurvatureGlow == null
                || _farStarsLayer == null
                || _middleStarsLayer == null
                || _nearStarsLayer == null
                || _cloudFarLayer == null
                || _cloudNearLayer == null
                || _nebulaLayer == null
                || _launchSmokeLayer == null
                || _spaceStarsParticles == null)
            {
                Debug.LogWarning(
                    "BackgroundController is missing some prefab-baked background renderers and layers. The scene will still be saved, but background visuals may be incomplete.",
                    this);
            }

            _built = true;
        }

        public void ApplyConfig(PrototypeGameConfig config)
        {
            if (config == null)
            {
                return;
            }

            _config = config;

            if (!Application.isPlaying)
            {
                EnsureBuilt();
#if UNITY_EDITOR
                ApplyGradient(config);
                ApplyAtmosphereBands(config);

                _farStarsLayer?.Build(
                    config.Background.Size,
                    config.Background.FarStarsScrollSpeed,
                    config.Background.FarStarsCount,
                    new Vector2(0.03f, 0.06f),
                    new Vector2(0.35f, 0.55f),
                    config.Background.StarTint,
                    0);

                _middleStarsLayer?.Build(
                    config.Background.Size,
                    config.Background.MiddleStarsScrollSpeed,
                    config.Background.MiddleStarsCount,
                    new Vector2(0.05f, 0.09f),
                    new Vector2(0.55f, 0.75f),
                    config.Background.StarTint,
                    1);

                _nearStarsLayer?.Build(
                    config.Background.Size,
                    config.Background.NearStarsScrollSpeed,
                    config.Background.NearStarsCount,
                    new Vector2(0.08f, 0.14f),
                    new Vector2(0.75f, 1f),
                    config.Background.StarTint,
                    2);

                _cloudFarLayer?.Build(
                    config.Background.Size,
                    config.Background.FarStarsScrollSpeed * 0.35f,
                    28,
                    new Vector2(0.6f, 1.55f),
                    new Vector2(0.1f, 0.22f),
                    config.LaunchFlow.CloudColor,
                    -3,
                    PrimitiveSpriteLibrary.SoftCircleSprite,
                    new Vector2(-config.Background.Size.x * 0.55f, config.Background.Size.x * 0.55f),
                    new Vector2(-config.Background.Size.y * 0.15f, config.Background.Size.y * 0.35f),
                    false);

                _cloudNearLayer?.Build(
                    config.Background.Size,
                    config.Background.MiddleStarsScrollSpeed * 0.3f,
                    42,
                    new Vector2(0.85f, 2.1f),
                    new Vector2(0.12f, 0.28f),
                    config.LaunchFlow.FogColor,
                    -2,
                    PrimitiveSpriteLibrary.SoftCircleSprite,
                    new Vector2(-config.Background.Size.x * 0.5f, config.Background.Size.x * 0.5f),
                    new Vector2(-config.Background.Size.y * 0.12f, config.Background.Size.y * 0.7f),
                    false);

                _nebulaLayer?.Build(
                    config.Background.Size,
                    config.Background.NearStarsScrollSpeed * 0.2f,
                    14,
                    new Vector2(1.6f, 3.8f),
                    new Vector2(0.06f, 0.16f),
                    config.LaunchFlow.SpaceGlowColor,
                    -4,
                    PrimitiveSpriteLibrary.SoftCircleSprite,
                    new Vector2(-config.Background.Size.x * 0.45f, config.Background.Size.x * 0.45f),
                    new Vector2(config.Background.Size.y * 0.3f, config.Background.Size.y * 0.95f),
                    false);

                _launchSmokeLayer?.Build(
                    config.Background.Size,
                    0.06f,
                    44,
                    new Vector2(1.05f, 3.1f),
                    new Vector2(0.06f, 0.18f),
                    config.LaunchFlow.FogColor,
                    4,
                    PrimitiveSpriteLibrary.SoftCircleSprite,
                    new Vector2(-config.Background.Size.x * 0.5f, config.Background.Size.x * 0.5f),
                    new Vector2(-config.Background.Size.y * 0.45f, config.Background.Size.y * 0.85f),
                    false);
#endif
                ApplySpaceStarsTint(config);
                SetIdleMode();
                return;
            }

            ApplyGradient(config);
            ApplyAtmosphereBands(config);
            ApplySpaceStarsTint(config);
            ApplyIdleLaunchVisuals();
            _built = HasBakedLayers();
            ResetLayerPositions();
        }

        public void SetIdleMode()
        {
            _scrollingEnabled = false;
            ResetLayerPositions();
            ApplyIdleLaunchVisuals();
        }

        public void StartScrolling()
        {
            if (_config == null)
            {
                return;
            }

            _scrollingEnabled = _config.Background.ScrollingEnabled;
        }

        public void StopScrolling()
        {
            _scrollingEnabled = false;
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                case GameState.Launching:
                    StartScrolling();
                    break;
                case GameState.RocketDestroyed:
                case GameState.Result:
                case GameState.Idle:
                    StopScrolling();
                    break;
            }
        }

        private void UpdateLaunchVisuals()
        {
            if (_config == null)
            {
                return;
            }

            var launchFlow = _config.LaunchFlow;
            var rocketY = _rocketController != null ? _rocketController.transform.position.y : launchFlow.FogStartHeight * 0.25f;
            var desiredStage = GetLaunchStage(rocketY);

            if (desiredStage != _toStage)
            {
                _fromStage = _currentStage;
                _toStage = desiredStage;
                _transitionStartTime = Time.time;
                _currentStage = desiredStage;
            }

            var transitionDuration = Mathf.Max(0.01f, launchFlow.TransitionDuration);
            var transitionT = Mathf.Clamp01((Time.time - _transitionStartTime) / transitionDuration);
            transitionT = Mathf.SmoothStep(0f, 1f, transitionT);
            ApplyLaunchBlend(_fromStage, _toStage, transitionT);
            UpdateFogParticles(rocketY, desiredStage);
            UpdateSpaceStars(desiredStage, transitionT);
        }

        private void ApplyIdleLaunchVisuals()
        {
            if (_config == null)
            {
                return;
            }

            _currentStage = LaunchStage.Sky;
            _fromStage = LaunchStage.Sky;
            _toStage = LaunchStage.Sky;
            _transitionStartTime = Time.time;
            _fogVfxNextTime = 0f;
            ApplyLaunchBlend(LaunchStage.Sky, LaunchStage.Sky, 1f);
            StopSpaceStars(true);
        }

        private void ApplyLaunchBlend(LaunchStage fromStage, LaunchStage toStage, float t)
        {
            var fromState = GetStageState(fromStage);
            var toState = GetStageState(toStage);
            var state = StageState.Lerp(fromState, toState, t);

            ApplyGradientColor(state.TopColor, state.BottomColor);
            SetLayerAppearance(_farStarsLayer, state.FarStarsOpacity, _config.Background.StarTint);
            SetLayerAppearance(_middleStarsLayer, state.MiddleStarsOpacity, _config.Background.StarTint);
            SetLayerAppearance(_nearStarsLayer, state.NearStarsOpacity, _config.Background.StarTint);
            SetLayerAppearance(_nebulaLayer, state.NebulaOpacity, state.NebulaTint);
            SetLayerAppearance(_cloudFarLayer, state.CloudFarOpacity, state.CloudTint);
            SetLayerAppearance(_cloudNearLayer, state.CloudNearOpacity, state.CloudTint);
            SetLayerAppearance(_launchSmokeLayer, state.LaunchSmokeOpacity, state.FogTint);
            SetRendererOpacity(_upperAtmosphereGlow, state.UpperAtmosphereOpacity);
            SetRendererOpacity(_planetCurvatureGlow, state.PlanetCurvatureOpacity);
            ApplyPostProcessing(state);
        }

        private StageState GetStageState(LaunchStage stage)
        {
            var launchFlow = _config.LaunchFlow;

            switch (stage)
            {
                case LaunchStage.Sky:
                    return new StageState(
                        launchFlow.SkyTopColor,
                        launchFlow.SkyBottomColor,
                        0f,
                        0f,
                        0f,
                        0f,
                        0.3f,
                        0.45f,
                        0.2f,
                        0.8f,
                        0.35f,
                        Color.white,
                        launchFlow.CloudColor,
                        launchFlow.FogColor,
                        0.14f,
                        0.05f,
                        0.02f,
                        0.08f,
                        0.92f,
                        0.35f,
                        0.08f,
                        0.3f,
                        0.01f);
                case LaunchStage.Fog:
                    return new StageState(
                        launchFlow.FogTopColor,
                        launchFlow.FogBottomColor,
                        0.08f,
                        0.06f,
                        0.03f,
                        0.15f,
                        0.95f,
                        1f,
                        1f,
                        0.95f,
                        0.22f,
                        Color.white,
                        launchFlow.FogColor,
                        launchFlow.FogColor,
                        0.02f,
                        -0.08f,
                        0.06f,
                        0.22f,
                        1f,
                        0.12f,
                        0.22f,
                        0.42f,
                        0.02f);
                case LaunchStage.Space:
                default:
                    return new StageState(
                        launchFlow.SpaceTopColor,
                        launchFlow.SpaceBottomColor,
                        1f,
                        0.9f,
                        0.8f,
                        1f,
                        0.05f,
                        0.03f,
                        0f,
                        0f,
                        0f,
                        launchFlow.SpaceGlowColor,
                        launchFlow.CloudColor,
                        launchFlow.FogColor,
                        -0.2f,
                        0.08f,
                        0.12f,
                        0.4f,
                        1.35f,
                        0.18f,
                        0.4f,
                        0.5f,
                        0.06f);
            }
        }

        private LaunchStage GetLaunchStage(float rocketY)
        {
            var launchFlow = _config.LaunchFlow;
            if (rocketY < launchFlow.FogStartHeight)
            {
                return LaunchStage.Sky;
            }

            if (rocketY < launchFlow.SpaceStartHeight)
            {
                return LaunchStage.Fog;
            }

            return LaunchStage.Space;
        }

        private void UpdateFogParticles(float rocketY, LaunchStage desiredStage)
        {
            if (_vfxService == null || _config == null)
            {
                return;
            }

            if (desiredStage != LaunchStage.Fog)
            {
                return;
            }

            var fogInterval = Mathf.Max(0.05f, _config.LaunchFlow.FogParticleInterval);
            if (Time.time < _fogVfxNextTime)
            {
                return;
            }

            _fogVfxNextTime = Time.time + fogInterval;
            var position = _rocketController != null
                ? _rocketController.transform.position + Vector3.down * 0.45f
                : new Vector3(0f, rocketY - 0.45f, 0f);
            _vfxService.SpawnFogZoneVfx(position);
        }

        private void ApplyGradientColor(Color topColor, Color bottomColor)
        {
            if (_spaceGradient == null)
            {
                return;
            }

            var size = _config.Background.Size;
            _gradientSprite = UpdateGradientSprite(_gradientSprite, ref _gradientTexture, topColor, bottomColor);
            _spaceGradient.sprite = _gradientSprite;
            _spaceGradient.transform.localPosition = new Vector3(0f, 0f, 2f);
            _spaceGradient.transform.localScale = new Vector3(size.x * 1.4f, size.y * 1.4f, 1f);
            _spaceGradient.sortingOrder = -20;
            _spaceGradient.color = Color.white;
        }

        private static void SetLayerAppearance(BackgroundLayer layer, float opacity, Color tint)
        {
            if (layer == null)
            {
                return;
            }

            layer.SetTint(tint);
            layer.SetOpacity(opacity);
        }

        private static void SetRendererOpacity(SpriteRenderer renderer, float opacity)
        {
            if (renderer == null)
            {
                return;
            }

            var color = renderer.color;
            color.a = Mathf.Clamp01(opacity);
            renderer.color = color;
        }

        private void UpdateSpaceStars(LaunchStage desiredStage, float transitionT)
        {
            if (_spaceStarsParticles == null)
            {
                return;
            }

            ApplySpaceStarsTint(_config);

            var emission = _spaceStarsParticles.emission;
            var targetMultiplier = desiredStage == LaunchStage.Space
                ? (_toStage == LaunchStage.Space ? transitionT : 1f)
                : 0f;

            emission.rateOverTimeMultiplier = Mathf.Lerp(0f, 26f, Mathf.Clamp01(targetMultiplier));

            if (targetMultiplier > 0.01f)
            {
                if (!_spaceStarsParticles.isPlaying)
                {
                    _spaceStarsParticles.gameObject.SetActive(true);
                    _spaceStarsParticles.Play(true);
                }

                return;
            }

            StopSpaceStars(true);
        }

        private void StopSpaceStars(bool clear)
        {
            if (_spaceStarsParticles == null)
            {
                return;
            }

            _spaceStarsParticles.Stop(true, clear
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting);
            _spaceStarsParticles.gameObject.SetActive(false);
        }

        private void UpdateSpaceStarsAnchor(Vector3 cameraPosition)
        {
            if (_spaceStarsParticles == null)
            {
                return;
            }

            var transformToMove = _spaceStarsParticles.transform;
            transformToMove.position = new Vector3(cameraPosition.x, cameraPosition.y, transformToMove.position.z);
        }

        private void ApplySpaceStarsTint(PrototypeGameConfig config)
        {
            if (config == null || _spaceStarsParticles == null)
            {
                return;
            }

            var main = _spaceStarsParticles.main;
            main.startColor = config.Background.StarTint;
        }

        private void ResetLayerPositions()
        {
            _farStarsLayer?.ResetTiles();
            _middleStarsLayer?.ResetTiles();
            _nearStarsLayer?.ResetTiles();
            _cloudFarLayer?.ResetTiles();
            _cloudNearLayer?.ResetTiles();
            _nebulaLayer?.ResetTiles();
            _launchSmokeLayer?.ResetTiles();
        }

        private void ApplyGradient(PrototypeGameConfig config)
        {
            ApplyGradientColor(config.LaunchFlow.SkyTopColor, config.LaunchFlow.SkyBottomColor);
        }

        private void ApplyAtmosphereBands(PrototypeGameConfig config)
        {
            var size = config.Background.Size;
            _upperAtmosphereSprite = UpdateGradientSprite(
                _upperAtmosphereSprite,
                ref _upperAtmosphereTexture,
                new Color(config.LaunchFlow.SkyTopColor.r, config.LaunchFlow.SkyTopColor.g, config.LaunchFlow.SkyTopColor.b, 0f),
                new Color(config.LaunchFlow.FogTopColor.r, config.LaunchFlow.FogTopColor.g, config.LaunchFlow.FogTopColor.b, 0.84f));
            _planetCurvatureSprite = UpdateGradientSprite(
                _planetCurvatureSprite,
                ref _planetCurvatureTexture,
                new Color(1f, 1f, 1f, 0f),
                new Color(config.LaunchFlow.SpaceGlowColor.r, config.LaunchFlow.SpaceGlowColor.g, config.LaunchFlow.SpaceGlowColor.b, 0.45f));

            ApplyBand(
                _upperAtmosphereGlow,
                _upperAtmosphereSprite,
                new Vector3(0f, size.y * 0.33f, 1.5f),
                new Vector3(size.x * 1.5f, size.y * 0.9f, 1f),
                -19);

            ApplyBand(
                _planetCurvatureGlow,
                _planetCurvatureSprite,
                new Vector3(0f, size.y * 0.52f, 1.2f),
                new Vector3(size.x * 1.95f, size.y * 0.55f, 1f),
                -18);
        }

        private void EnsureVolumeProfile()
        {
            if (_globalVolume == null)
            {
                return;
            }

            if (_globalVolume.profile == null && _globalVolume.sharedProfile != null)
            {
                _runtimeVolumeProfile = Instantiate(_globalVolume.sharedProfile);
                _globalVolume.profile = _runtimeVolumeProfile;
            }
            else
            {
                _runtimeVolumeProfile = _globalVolume.profile;
            }

            if (_runtimeVolumeProfile == null)
            {
                return;
            }

            _runtimeVolumeProfile.TryGet(out _bloom);
            _runtimeVolumeProfile.TryGet(out _colorAdjustments);
            _runtimeVolumeProfile.TryGet(out _vignette);
            _runtimeVolumeProfile.TryGet(out _chromaticAberration);
        }

        private void ApplyPostProcessing(StageState state)
        {
            if (_runtimeVolumeProfile == null)
            {
                return;
            }

            if (_bloom != null)
            {
                _bloom.active = true;
                _bloom.intensity.overrideState = true;
                _bloom.intensity.value = Mathf.Max(0f, state.BloomIntensity);
                _bloom.threshold.overrideState = true;
                _bloom.threshold.value = Mathf.Max(0f, state.BloomThreshold);
                _bloom.scatter.overrideState = true;
                _bloom.scatter.value = Mathf.Clamp01(state.BloomScatter);
            }

            if (_colorAdjustments != null)
            {
                _colorAdjustments.active = true;
                _colorAdjustments.postExposure.overrideState = true;
                _colorAdjustments.postExposure.value = state.Exposure;
                _colorAdjustments.saturation.overrideState = true;
                _colorAdjustments.saturation.value = state.Saturation;
                _colorAdjustments.contrast.overrideState = true;
                _colorAdjustments.contrast.value = state.Contrast;
            }

            if (_vignette != null)
            {
                _vignette.active = true;
                _vignette.intensity.overrideState = true;
                _vignette.intensity.value = Mathf.Clamp01(state.VignetteIntensity);
                _vignette.smoothness.overrideState = true;
                _vignette.smoothness.value = Mathf.Clamp01(state.VignetteSmoothness);
            }

            if (_chromaticAberration != null)
            {
                _chromaticAberration.active = true;
                _chromaticAberration.intensity.overrideState = true;
                _chromaticAberration.intensity.value = Mathf.Clamp01(state.ChromaticAberration);
            }
        }

        private static Sprite UpdateGradientSprite(Sprite existing, ref Texture2D texture, Color topColor, Color bottomColor)
        {
            if (texture == null)
            {
                texture = new Texture2D(1, 128, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }

            for (var y = 0; y < texture.height; y++)
            {
                var t = y / (texture.height - 1f);
                var color = Color.Lerp(bottomColor, topColor, t);
                texture.SetPixel(0, y, color);
            }

            texture.Apply();

            if (existing != null)
            {
                return existing;
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
        }

        private static void DestroyGeneratedTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
        }

        private static void DestroySprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            var texture = sprite.texture;
            if (Application.isPlaying)
            {
                Destroy(sprite);
                if (texture != null)
                {
                    Destroy(texture);
                }
            }
            else
            {
                DestroyImmediate(sprite);
                if (texture != null)
                {
                    DestroyImmediate(texture);
                }
            }
        }

        private static void ApplyBand(SpriteRenderer renderer, Sprite sprite, Vector3 localPosition, Vector3 localScale, int sortingOrder)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sprite = sprite;
            renderer.transform.localPosition = localPosition;
            renderer.transform.localScale = localScale;
            renderer.sortingOrder = sortingOrder;
            renderer.color = Color.white;
        }

        private bool HasBakedLayers()
        {
            return _spaceGradient != null
                   && _upperAtmosphereGlow != null
                   && _planetCurvatureGlow != null
                   && _farStarsLayer != null
                   && _middleStarsLayer != null
                   && _nearStarsLayer != null
                   && _cloudFarLayer != null
                   && _cloudNearLayer != null
                   && _nebulaLayer != null
                   && _launchSmokeLayer != null
                   && _spaceStarsParticles != null;
        }

        private readonly struct StageState
        {
            public readonly Color TopColor;
            public readonly Color BottomColor;
            public readonly float FarStarsOpacity;
            public readonly float MiddleStarsOpacity;
            public readonly float NearStarsOpacity;
            public readonly float NebulaOpacity;
            public readonly float CloudFarOpacity;
            public readonly float CloudNearOpacity;
            public readonly float LaunchSmokeOpacity;
            public readonly float UpperAtmosphereOpacity;
            public readonly float PlanetCurvatureOpacity;
            public readonly Color NebulaTint;
            public readonly Color CloudTint;
            public readonly Color FogTint;
            public readonly float Exposure;
            public readonly float Saturation;
            public readonly float Contrast;
            public readonly float BloomIntensity;
            public readonly float BloomThreshold;
            public readonly float BloomScatter;
            public readonly float VignetteIntensity;
            public readonly float VignetteSmoothness;
            public readonly float ChromaticAberration;

            public StageState(
                Color topColor,
                Color bottomColor,
                float farStarsOpacity,
                float middleStarsOpacity,
                float nearStarsOpacity,
                float nebulaOpacity,
                float cloudFarOpacity,
                float cloudNearOpacity,
                float launchSmokeOpacity,
                float upperAtmosphereOpacity,
                float planetCurvatureOpacity,
                Color nebulaTint,
                Color cloudTint,
                Color fogTint,
                float exposure,
                float saturation,
                float contrast,
                float bloomIntensity,
                float bloomThreshold,
                float bloomScatter,
                float vignetteIntensity,
                float vignetteSmoothness,
                float chromaticAberration)
            {
                TopColor = topColor;
                BottomColor = bottomColor;
                FarStarsOpacity = farStarsOpacity;
                MiddleStarsOpacity = middleStarsOpacity;
                NearStarsOpacity = nearStarsOpacity;
                NebulaOpacity = nebulaOpacity;
                CloudFarOpacity = cloudFarOpacity;
                CloudNearOpacity = cloudNearOpacity;
                LaunchSmokeOpacity = launchSmokeOpacity;
                UpperAtmosphereOpacity = upperAtmosphereOpacity;
                PlanetCurvatureOpacity = planetCurvatureOpacity;
                NebulaTint = nebulaTint;
                CloudTint = cloudTint;
                FogTint = fogTint;
                Exposure = exposure;
                Saturation = saturation;
                Contrast = contrast;
                BloomIntensity = bloomIntensity;
                BloomThreshold = bloomThreshold;
                BloomScatter = bloomScatter;
                VignetteIntensity = vignetteIntensity;
                VignetteSmoothness = vignetteSmoothness;
                ChromaticAberration = chromaticAberration;
            }

            public static StageState Lerp(StageState from, StageState to, float t)
            {
                return new StageState(
                    Color.Lerp(from.TopColor, to.TopColor, t),
                    Color.Lerp(from.BottomColor, to.BottomColor, t),
                    Mathf.Lerp(from.FarStarsOpacity, to.FarStarsOpacity, t),
                    Mathf.Lerp(from.MiddleStarsOpacity, to.MiddleStarsOpacity, t),
                    Mathf.Lerp(from.NearStarsOpacity, to.NearStarsOpacity, t),
                    Mathf.Lerp(from.NebulaOpacity, to.NebulaOpacity, t),
                    Mathf.Lerp(from.CloudFarOpacity, to.CloudFarOpacity, t),
                    Mathf.Lerp(from.CloudNearOpacity, to.CloudNearOpacity, t),
                    Mathf.Lerp(from.LaunchSmokeOpacity, to.LaunchSmokeOpacity, t),
                    Mathf.Lerp(from.UpperAtmosphereOpacity, to.UpperAtmosphereOpacity, t),
                    Mathf.Lerp(from.PlanetCurvatureOpacity, to.PlanetCurvatureOpacity, t),
                    Color.Lerp(from.NebulaTint, to.NebulaTint, t),
                    Color.Lerp(from.CloudTint, to.CloudTint, t),
                    Color.Lerp(from.FogTint, to.FogTint, t),
                    Mathf.Lerp(from.Exposure, to.Exposure, t),
                    Mathf.Lerp(from.Saturation, to.Saturation, t),
                    Mathf.Lerp(from.Contrast, to.Contrast, t),
                    Mathf.Lerp(from.BloomIntensity, to.BloomIntensity, t),
                    Mathf.Lerp(from.BloomThreshold, to.BloomThreshold, t),
                    Mathf.Lerp(from.BloomScatter, to.BloomScatter, t),
                    Mathf.Lerp(from.VignetteIntensity, to.VignetteIntensity, t),
                    Mathf.Lerp(from.VignetteSmoothness, to.VignetteSmoothness, t),
                    Mathf.Lerp(from.ChromaticAberration, to.ChromaticAberration, t));
            }
        }

        private enum LaunchStage
        {
            Sky,
            Fog,
            Space
        }
    }
}
