using System;
using System.IO;
using SpaceFlight2D.Game;
using SpaceFlight2D.Game.Bootstrap;
using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace SpaceFlight2D.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/MainScene.unity";
        private const string LoadingScenePath = "Assets/Game/Scenes/LoadingScene.unity";
        private const string ConfigFolder = "Assets/Game/Configs";
        private const string PrefabFolder = "Assets/Game/Prefabs/Prototype";
        private const string SystemsPrefabPath = PrefabFolder + "/PrototypeSystems.prefab";
        private const string WorldPrefabPath = PrefabFolder + "/PrototypeWorld.prefab";
        private const string UiPrefabPath = PrefabFolder + "/PrototypeUi.prefab";

        [MenuItem("Tools/Rocket/Create Config Assets")]
        public static void CreateConfigAssets()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder(ConfigFolder);

            CreateConfigIfMissing(PrototypeScenePaths.DefaultConfigPath, ConfigureCleanPreset);
            CreateConfigIfMissing(PrototypeScenePaths.ActionConfigPath, ConfigureActionPreset);
            CreateConfigIfMissing(PrototypeScenePaths.NoUiConfigPath, ConfigureNoUiPreset);
            CreateConfigIfMissing(PrototypeScenePaths.PurpleConfigPath, config => ConfigureCosmosPreset(config, DefaultColorPalette.PurpleCosmos));
            CreateConfigIfMissing(PrototypeScenePaths.BlueConfigPath, config => ConfigureCosmosPreset(config, DefaultColorPalette.BlueCosmos));
            CreateConfigIfMissing(PrototypeScenePaths.RedConfigPath, config => ConfigureCosmosPreset(config, DefaultColorPalette.RedCosmos));
            CreateConfigIfMissing(PrototypeScenePaths.GreenConfigPath, config => ConfigureCosmosPreset(config, DefaultColorPalette.GreenCosmos));
            CreateConfigIfMissing(PrototypeScenePaths.RainbowConfigPath, config => ConfigureCosmosPreset(config, DefaultColorPalette.RainbowCosmos));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Rocket/Rebuild Prototype Scene")]
        public static void RebuildPrototypeScene()
        {
            CreateConfigAssets();
            PrototypeScenePrefabFactory.EnsureScenePrefabs();

            var previousScene = SceneManager.GetActiveScene();
            var previousScenePath = previousScene.path;
            var config = AssetDatabase.LoadAssetAtPath<PrototypeGameConfig>(PrototypeScenePaths.DefaultConfigPath);

            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "MainScene";

                BuildScene(config);
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
                CreateLoadingScene();
                UpdateBuildSettings();
                EditorSceneManager.OpenScene(ScenePath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (!string.IsNullOrEmpty(previousScenePath) && File.Exists(previousScenePath))
                {
                    EditorSceneManager.OpenScene(previousScenePath);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildScene(PrototypeGameConfig config)
        {
            var sceneRootGo = new GameObject("PrototypeScene");
            var sceneAuthoring = sceneRootGo.AddComponent<PrototypeSceneAuthoring>();
            var sceneContext = sceneRootGo.AddComponent<SceneContext>();
            PrototypeScenePrefabFactory.BakeSceneRoot(sceneAuthoring, config);

            var systemsRootGo = InstantiatePrefab(SystemsPrefabPath);
            var worldRootGo = InstantiatePrefab(WorldPrefabPath);
            var uiRootGo = InstantiatePrefab(UiPrefabPath);

            var systemsAuthoring = systemsRootGo.GetComponent<PrototypeSystemsAuthoring>();
            var worldAuthoring = worldRootGo.GetComponent<PrototypeWorldAuthoring>();
            var uiAuthoring = uiRootGo.GetComponent<PrototypeUiAuthoring>();
            if (systemsAuthoring == null || worldAuthoring == null || uiAuthoring == null)
            {
                Debug.LogWarning("One or more prototype prefabs are missing authoring components. Scene build will continue, but runtime wiring may be incomplete.");
            }

            systemsAuthoring?.EnsureBuilt(config);
            worldAuthoring?.EnsureBuilt();
            uiAuthoring?.Apply(config);
            var uiController = uiAuthoring != null ? uiAuthoring.Controller : null;
            var gameRootGo = systemsRootGo;
            var sceneRefs = systemsAuthoring != null ? systemsAuthoring.SceneReferences : null;
            var installer = systemsAuthoring != null ? systemsAuthoring.Installer : null;
            var customizer = systemsAuthoring != null ? systemsAuthoring.Customizer : null;
            var gameplayLoop = systemsAuthoring != null ? systemsAuthoring.GameplayLoop : null;
            var gameBootstrapper = systemsAuthoring != null ? systemsAuthoring.GameBootstrapper : null;
            var vfxRoot = systemsAuthoring != null ? systemsAuthoring.VfxRoot : null;
            var vfxService = systemsAuthoring != null ? systemsAuthoring.VfxService : null;
            var cameraShakeService = systemsAuthoring != null ? systemsAuthoring.CameraShakeService : null;
            var spawner = systemsAuthoring != null ? systemsAuthoring.Spawner : null;

            var background = worldAuthoring != null ? worldAuthoring.BackgroundRenderer : null;
            var backgroundController = worldAuthoring != null ? worldAuthoring.BackgroundController : null;
            var platform = worldAuthoring != null ? worldAuthoring.PlatformRenderer : null;
            var globalVolume = worldAuthoring != null ? worldAuthoring.GlobalVolume : null;
            var asteroidRoot = sceneAuthoring.AsteroidRoot;
            var cameraController = sceneAuthoring.CameraController;
            var rocketAuthoring = worldAuthoring != null ? worldAuthoring.RocketAuthoring : null;
            var rocket = worldAuthoring != null ? worldAuthoring.Rocket : null;
            var rocketRenderer = rocketAuthoring?.BodyRenderer;
            var rocketCollider = rocketAuthoring?.HitCollider;
            var trail = rocketAuthoring?.TrailRenderer;
            var engineParticles = rocketAuthoring?.EngineParticles;
            if (rocket == null || rocketAuthoring == null)
            {
                Debug.LogWarning("Prototype world prefab is missing rocket controller or authoring references. Scene build will continue with whatever is available.");
            }
            var uiRoot = uiRootGo.transform;
            var camera = sceneAuthoring.MainCamera;
            var asteroidPoolRoot = asteroidRoot != null ? asteroidRoot : sceneRootGo.transform;
            var asteroidPool = CreateAsteroidPool(asteroidPoolRoot, config);

            var flashOverlayGo = uiRootGo.transform.Find("CameraFlashOverlay")?.gameObject;
            Image flashOverlay = null;
            if (flashOverlayGo != null)
            {
                var flashOverlayRect = flashOverlayGo.GetComponent<RectTransform>();
                flashOverlayRect.anchorMin = Vector2.zero;
                flashOverlayRect.anchorMax = Vector2.one;
                flashOverlayRect.offsetMin = Vector2.zero;
                flashOverlayRect.offsetMax = Vector2.zero;
                flashOverlay = flashOverlayGo.GetComponent<Image>();
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
                flashOverlay.raycastTarget = false;
                flashOverlayGo.SetActive(false);
            }

            sceneRefs?.Bind(
                camera,
                gameRootGo.transform,
                asteroidRoot,
                vfxRoot,
                uiRoot,
                rocket,
                spawner,
                vfxService,
                cameraShakeService,
                uiController,
                backgroundController,
                cameraController,
                gameBootstrapper,
                gameplayLoop,
                customizer,
                installer,
                platform,
                background,
                globalVolume,
                config);

            customizer?.Bind(config, sceneRefs, true, true, true);
            installer?.Bind(sceneRefs);
            spawner?.Bind(asteroidRoot, camera, asteroidPool);
            backgroundController?.Bind(camera);
            if (vfxService != null && flashOverlay != null)
            {
                vfxService.Bind(vfxRoot, flashOverlay);
            }
            cameraShakeService?.Bind(camera);
            rocket?.Bind(rocket.GetComponent<Rigidbody2D>(), rocketAuthoring);
            rocketAuthoring?.Bind(rocketRenderer, trail, engineParticles, rocketCollider);
            rocketAuthoring?.Apply(config);
            customizer?.Apply();

            sceneContext.Installers = installer != null ? new[] { installer } : Array.Empty<MonoInstaller>();
        }

        private static void CreateLoadingScene()
        {
            var loadingScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            loadingScene.name = "LoadingScene";

            var root = new GameObject("LoadingScene");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasScaler = root.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080f, 1920f);
            canvasScaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var controller = root.AddComponent<LoadingSceneController>();

            var splashGroupGo = new GameObject("SplashGroup", typeof(RectTransform), typeof(CanvasGroup));
            splashGroupGo.transform.SetParent(root.transform, false);
            Stretch(splashGroupGo.GetComponent<RectTransform>());
            var splashGroup = splashGroupGo.GetComponent<CanvasGroup>();
            splashGroup.alpha = 1f;
            splashGroup.interactable = false;
            splashGroup.blocksRaycasts = true;

            var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(splashGroupGo.transform, false);
            Stretch(backdrop.GetComponent<RectTransform>());
            backdrop.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 1f);

            var ambientTop = new GameObject("AmbientTop", typeof(RectTransform), typeof(Image));
            ambientTop.transform.SetParent(splashGroupGo.transform, false);
            Stretch(ambientTop.GetComponent<RectTransform>());
            var ambientTopImage = ambientTop.GetComponent<Image>();
            ambientTopImage.sprite = PrimitiveSpriteLibrary.SoftCircleSprite;
            ambientTopImage.color = new Color(0.26f, 0.44f, 0.88f, 0.16f);
            ambientTop.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            ambientTop.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            ambientTop.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            ambientTop.GetComponent<RectTransform>().sizeDelta = new Vector2(1480f, 760f);
            ambientTop.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 180f);

            var ambientBottom = new GameObject("AmbientBottom", typeof(RectTransform), typeof(Image));
            ambientBottom.transform.SetParent(splashGroupGo.transform, false);
            Stretch(ambientBottom.GetComponent<RectTransform>());
            var ambientBottomImage = ambientBottom.GetComponent<Image>();
            ambientBottomImage.sprite = PrimitiveSpriteLibrary.SoftCircleSprite;
            ambientBottomImage.color = new Color(0.95f, 0.56f, 0.25f, 0.12f);
            ambientBottom.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
            ambientBottom.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
            ambientBottom.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            ambientBottom.GetComponent<RectTransform>().sizeDelta = new Vector2(1320f, 640f);
            ambientBottom.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -120f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(splashGroupGo.transform, false);
            Stretch(content.GetComponent<RectTransform>());

            var card = CreatePanel("Card", content.transform, new Vector2(760f, 420f), AnchorPreset.MiddleCenter, Vector2.zero);
            card.transform.localScale = Vector3.one;
            var cardAccent = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
            cardAccent.transform.SetParent(card.transform, false);
            var accentRect = cardAccent.GetComponent<RectTransform>();
            SetAnchor(accentRect, AnchorPreset.TopCenter);
            accentRect.anchoredPosition = new Vector2(0f, -34f);
            accentRect.sizeDelta = new Vector2(240f, 8f);
            cardAccent.GetComponent<Image>().color = new Color(0.95f, 0.76f, 0.28f, 1f);

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(card.transform, false);
            Stretch(header.GetComponent<RectTransform>());
            var body = new GameObject("Body", typeof(RectTransform));
            body.transform.SetParent(card.transform, false);
            Stretch(body.GetComponent<RectTransform>());
            var footer = new GameObject("Footer", typeof(RectTransform));
            footer.transform.SetParent(card.transform, false);
            Stretch(footer.GetComponent<RectTransform>());
            var spinnerRoot = new GameObject("SpinnerRoot", typeof(RectTransform));
            spinnerRoot.transform.SetParent(footer.transform, false);
            var spinnerRect = spinnerRoot.GetComponent<RectTransform>();
            SetAnchor(spinnerRect, AnchorPreset.BottomCenter);
            spinnerRect.anchoredPosition = new Vector2(0f, 48f);
            spinnerRect.sizeDelta = new Vector2(140f, 140f);

            var spinnerTrack = new GameObject("SpinnerTrack", typeof(RectTransform), typeof(Image));
            spinnerTrack.transform.SetParent(spinnerRoot.transform, false);
            Stretch(spinnerTrack.GetComponent<RectTransform>());
            var spinnerTrackImage = spinnerTrack.GetComponent<Image>();
            spinnerTrackImage.sprite = PrimitiveSpriteLibrary.SoftCircleSprite;
            spinnerTrackImage.color = new Color(1f, 1f, 1f, 0.08f);

            var spinnerDotA = CreateSpinnerDot("SpinnerDotA", spinnerRoot.transform, 0f, new Color(0.95f, 0.76f, 0.28f, 1f)).GetComponent<Image>();
            var spinnerDotB = CreateSpinnerDot("SpinnerDotB", spinnerRoot.transform, 120f, new Color(0.78f, 0.9f, 1f, 1f)).GetComponent<Image>();
            var spinnerDotC = CreateSpinnerDot("SpinnerDotC", spinnerRoot.transform, 240f, new Color(1f, 1f, 1f, 1f)).GetComponent<Image>();

            var title = CreateText("Title", header.transform, new Vector2(620f, 110f), 62, TextAnchor.MiddleCenter, "ROCKET LAUNCH", AnchorPreset.TopCenter, new Vector2(0f, -100f));
            title.color = Color.white;
            var subtitle = CreateText("Subtitle", body.transform, new Vector2(520f, 80f), 34, TextAnchor.MiddleCenter, "Loading mission", AnchorPreset.MiddleCenter, new Vector2(0f, -10f));
            subtitle.color = new Color(1f, 1f, 1f, 0.75f);
            var hint = CreateText("Hint", footer.transform, new Vector2(480f, 60f), 24, TextAnchor.MiddleCenter, "Preparing scene graph", AnchorPreset.BottomCenter, new Vector2(0f, 46f));
            hint.color = new Color(1f, 1f, 1f, 0.5f);

            controller.Bind(splashGroup, spinnerRect, new[] { spinnerDotA, spinnerDotB, spinnerDotC }, hint);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), LoadingScenePath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void UpdateBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(LoadingScenePath, true),
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static Asteroid[] CreateAsteroidPool(Transform asteroidRoot, PrototypeGameConfig config)
        {
            var count = Mathf.Max(1, config.Asteroids.InitialPoolSize);
            var poolRoot = new GameObject("AsteroidPoolRoot");
            poolRoot.transform.SetParent(asteroidRoot, false);

            var asteroids = new Asteroid[count];
            for (var i = 0; i < count; i++)
            {
                asteroids[i] = CreateAsteroidInstance(poolRoot.transform, config, i);
            }

            return asteroids;
        }

        private static Asteroid CreateAsteroidInstance(Transform parent, PrototypeGameConfig config, int index)
        {
            if (config.Asteroids.Prefab == null)
            {
                Debug.LogWarning("Prototype asteroid prefab is missing. Creating a fallback asteroid instance for the scene build.");
                return CreateFallbackAsteroid(parent, config, index);
            }

            var asteroidGo = (GameObject)UnityEngine.Object.Instantiate(config.Asteroids.Prefab, parent);
            asteroidGo.name = $"Asteroid_{index:D2}";
            var asteroid = asteroidGo.GetComponent<Asteroid>();
            if (asteroid == null)
            {
                Debug.LogWarning("Prototype asteroid prefab is missing Asteroid component. Creating a fallback asteroid instance for the scene build.");
                return CreateFallbackAsteroid(parent, config, index);
            }

            asteroidGo.SetActive(false);
            return asteroid;
        }

        private static Asteroid CreateFallbackAsteroid(Transform parent, PrototypeGameConfig config, int index)
        {
            var asteroidGo = new GameObject($"Asteroid_{index:D2}");
            asteroidGo.transform.SetParent(parent, false);

            var collider = asteroidGo.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(asteroidGo.transform, false);
            var bodyVisual = new GameObject("BodyVisual");
            bodyVisual.transform.SetParent(visualRoot.transform, false);
            var renderer = bodyVisual.AddComponent<SpriteRenderer>();
            renderer.sprite = PrimitiveSpriteLibrary.CircleSprite;
            renderer.sortingOrder = 8;

            var authoring = asteroidGo.AddComponent<AsteroidAuthoring>();
            var asteroid = asteroidGo.AddComponent<Asteroid>();
            renderer.color = config.Asteroids.Large.Color;
            authoring.Bind(renderer, collider, visualRoot.transform);
            asteroid.Bind(authoring);
            asteroidGo.SetActive(false);
            authoring.Apply(new AsteroidSpawnSettings
            {
                VisualSprite = config.Asteroids.VisualSprite,
                Size = config.Asteroids.Large.SizeRange,
                Color = config.Asteroids.Large.Color,
                ScoreReward = config.Asteroids.Large.ScoreReward
            });
            return asteroid;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size, AnchorPreset anchorPreset, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetAnchor(rect, anchorPreset);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);
            return go;
        }

        private static GameObject CreateSpinnerDot(string name, Transform parent, float angleDegrees, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(18f, 18f);
            rect.anchoredPosition = new Vector2(Mathf.Cos(angleDegrees * Mathf.Deg2Rad) * 42f, Mathf.Sin(angleDegrees * Mathf.Deg2Rad) * 42f);
            var image = go.GetComponent<Image>();
            image.sprite = PrimitiveSpriteLibrary.SoftCircleSprite;
            image.color = color;
            return go;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Vector2 size,
            int fontSize,
            TextAnchor alignment,
            string text,
            AnchorPreset anchorPreset,
            Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetAnchor(rect, anchorPreset);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var uiText = go.GetComponent<Text>();
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = fontSize;
            uiText.alignment = alignment;
            uiText.text = text;
            uiText.color = Color.white;
            uiText.raycastTarget = false;
            return uiText;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetAnchor(RectTransform rectTransform, AnchorPreset anchorPreset)
        {
            switch (anchorPreset)
            {
                case AnchorPreset.TopCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    break;
                case AnchorPreset.BottomCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    break;
                case AnchorPreset.BottomLeft:
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);
                    break;
                case AnchorPreset.BottomRight:
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    break;
                case AnchorPreset.TopLeft:
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    break;
                case AnchorPreset.TopRight:
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.pivot = new Vector2(1f, 1f);
                    break;
                default:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }

        [MenuItem("Tools/Rocket/Create Prototype Prefabs")]
        public static void CreatePrototypePrefabs()
        {
            CreateConfigAssets();
            PrototypeScenePrefabFactory.EnsureScenePrefabs();
        }

        private static GameObject InstantiatePrefab(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Missing prototype prefab at '{assetPath}'. Creating an empty fallback object.");
                return new GameObject(System.IO.Path.GetFileNameWithoutExtension(assetPath));
            }

            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        private static void CreateConfigIfMissing(string assetPath, System.Action<PrototypeGameConfig> setup)
        {
            if (AssetDatabase.LoadAssetAtPath<PrototypeGameConfig>(assetPath) != null)
            {
                return;
            }

            var config = ScriptableObject.CreateInstance<PrototypeGameConfig>();
            setup(config);
            AssetDatabase.CreateAsset(config, assetPath);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var segments = path.Split('/');
            var current = segments[0];

            for (var i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }

        private static void ConfigureCleanPreset(PrototypeGameConfig config)
        {
            config.Asteroids.SpawnIntervalMin = 1.1f;
            config.Asteroids.SpawnIntervalMax = 1.4f;
            config.Presentation.CameraZoom = 5f;
            config.Camera.OrthographicSize = 5f;
            config.Camera.FollowSmoothness = 4f;
            config.Background.ScrollingEnabled = true;
            config.Background.TopColor = DefaultColorPalette.BackgroundTop;
            config.Background.BottomColor = DefaultColorPalette.BackgroundBottom;
            config.Background.StarTint = DefaultColorPalette.StarTint;
            config.Background.Size = new Vector2(7f, 14f);
            config.Background.FarStarsScrollSpeed = 0.4f;
            config.Background.MiddleStarsScrollSpeed = 0.8f;
            config.Background.NearStarsScrollSpeed = 1.3f;
            config.Background.FarStarsCount = 70;
            config.Background.MiddleStarsCount = 48;
            config.Background.NearStarsCount = 28;
            config.StartFlow.LaunchDelay = 0.15f;
            config.StartFlow.LaunchAccelerationDuration = 0.45f;
            config.StartFlow.LaunchPunchScale = 0.12f;
            config.StartFlow.LaunchPunchDuration = 0.2f;
            config.Presentation.BackgroundColor = DefaultColorPalette.PresentationBackground;
            config.Presentation.BackgroundPanelColor = DefaultColorPalette.CleanPresetPanel;
            config.Presentation.PlatformColor = DefaultColorPalette.CleanPresetPlatform;
            config.LaunchFlow.FogStartHeight = 1.8f;
            config.LaunchFlow.SpaceStartHeight = 6.4f;
            config.LaunchFlow.TransitionDuration = 0.8f;
            config.LaunchFlow.FogParticleInterval = 0.22f;
            config.LaunchFlow.SkyTopColor = DefaultColorPalette.SkyTop;
            config.LaunchFlow.SkyBottomColor = DefaultColorPalette.SkyBottom;
            config.LaunchFlow.FogTopColor = DefaultColorPalette.FogTop;
            config.LaunchFlow.FogBottomColor = DefaultColorPalette.FogBottom;
            config.LaunchFlow.SpaceTopColor = DefaultColorPalette.SpaceTop;
            config.LaunchFlow.SpaceBottomColor = DefaultColorPalette.SpaceBottom;
            config.LaunchFlow.CloudColor = DefaultColorPalette.Cloud;
            config.LaunchFlow.FogColor = DefaultColorPalette.Fog;
            config.LaunchFlow.SpaceGlowColor = DefaultColorPalette.SpaceGlow;
            config.Vfx.SmallShakeStrength = 0.08f;
            config.Vfx.BigShakeStrength = 0.2f;
            config.Vfx.CameraColorFlashEnabled = false;
            config.Rocket.Trail.RocketEnabled = true;
            config.Ui.Enabled = true;
            config.Recording.HideUiForRecording = false;
            config.Vfx.Enabled = true;
            config.Vfx.ScreenShakeEnabled = true;
        }

        private static void ConfigureActionPreset(PrototypeGameConfig config)
        {
            ConfigureCleanPreset(config);
            config.Asteroids.SpawnIntervalMin = 0.45f;
            config.Asteroids.SpawnIntervalMax = 0.6f;
            config.Presentation.CameraZoom = 4.5f;
            config.Camera.OrthographicSize = 4.5f;
            config.Recording.GameSpeedMultiplier = 1.1f;
            config.Presentation.BackgroundColor = DefaultColorPalette.ActionBackground;
            config.Presentation.BackgroundPanelColor = DefaultColorPalette.ActionPanel;
            config.Background.TopColor = DefaultColorPalette.ActionTop;
            config.Background.BottomColor = DefaultColorPalette.ActionBottom;
            config.LaunchFlow.TransitionDuration = 0.9f;
            config.LaunchFlow.FogParticleInterval = 0.18f;
            config.LaunchFlow.SpaceTopColor = DefaultColorPalette.ActionTop;
            config.LaunchFlow.SpaceBottomColor = DefaultColorPalette.ActionBottom;
            config.Rocket.Color = DefaultColorPalette.ActionRocket;
            config.Vfx.SmallShakeStrength = 0.18f;
            config.Vfx.BigShakeStrength = 0.45f;
            config.Vfx.CameraColorFlashEnabled = true;
            config.Rocket.Trail.RocketEnabled = true;
        }

        private static void ConfigureNoUiPreset(PrototypeGameConfig config)
        {
            ConfigureCleanPreset(config);
            config.Ui.Enabled = false;
            config.Recording.HideUiForRecording = true;
            config.Asteroids.SpawnIntervalMin = 0.65f;
            config.Asteroids.SpawnIntervalMax = 0.8f;
            config.Presentation.CameraZoom = 4.8f;
            config.Camera.OrthographicSize = 4.8f;
            config.Vfx.Enabled = false;
            config.Vfx.ScreenShakeEnabled = false;
            config.Vfx.CameraColorFlashEnabled = false;
            config.Rocket.Trail.RocketEnabled = false;
        }

        private static void ConfigureCosmosPreset(PrototypeGameConfig config, DefaultColorPalette.CosmosPalette palette)
        {
            ConfigureCleanPreset(config);
            config.Background.TopColor = palette.Top;
            config.Background.BottomColor = palette.Bottom;
            config.LaunchFlow.SpaceTopColor = palette.Top;
            config.LaunchFlow.SpaceBottomColor = palette.Bottom;
            config.Presentation.BackgroundColor = palette.Background;
            config.Presentation.BackgroundPanelColor = palette.Panel;
            config.Presentation.PlatformColor = palette.Platform;
        }

        private enum AnchorPreset
        {
            MiddleCenter,
            BottomCenter,
            BottomLeft,
            BottomRight,
            TopLeft,
            TopRight,
            TopCenter
        }

    }
}
