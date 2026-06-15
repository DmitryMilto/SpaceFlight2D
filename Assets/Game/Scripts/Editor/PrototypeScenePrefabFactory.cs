using System;
using SpaceFlight2D.Game;
using SpaceFlight2D.Game.Bootstrap;
using SpaceFlight2D.Game.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace SpaceFlight2D.Editor
{
    public static class PrototypeScenePrefabFactory
    {
        public static void EnsureScenePrefabs()
        {
            PrototypePrefabFactory.EnsureGameplayPrefabs();
            EnsureFolder(PrototypeScenePaths.PrefabFolder);

            var config = AssetDatabase.LoadAssetAtPath<PrototypeGameConfig>(PrototypeScenePaths.DefaultConfigPath);
            if (config == null)
            {
                throw new InvalidOperationException($"Missing prototype config at '{PrototypeScenePaths.DefaultConfigPath}'.");
            }

            CreatePrefabIfMissing(PrototypeScenePaths.SystemsPrefabPath, () => CreateSystemsPrefab(config));
            CreatePrefabIfMissing(PrototypeScenePaths.WorldPrefabPath, () => CreateWorldPrefab(config));
            EnsurePrefabExists(PrototypeScenePaths.UiPrefabPath);
        }

        public static void BakeSceneRoot(PrototypeSceneAuthoring authoring, PrototypeGameConfig config)
        {
            if (authoring == null)
            {
                throw new ArgumentNullException(nameof(authoring));
            }

            var asteroidRoot = authoring.transform.Find("AsteroidRoot") ?? CreateChildTransform(authoring.transform, "AsteroidRoot");
            var mainCamera = authoring.transform.Find("Main Camera")?.GetComponent<Camera>() ?? CreateMainCamera(authoring.transform, config);
            var cameraController = mainCamera.GetComponent<CameraController>() ?? mainCamera.gameObject.AddComponent<CameraController>();
            cameraController.Bind(mainCamera);
            var eventSystem = authoring.transform.Find("EventSystem")?.GetComponent<EventSystem>() ?? CreateEventSystem(authoring.transform);

            authoring.Bind(mainCamera, cameraController, eventSystem, asteroidRoot);
            authoring.EnsureBuilt();
        }

        private static GameObject CreateSystemsPrefab(PrototypeGameConfig config)
        {
            var gameRootGo = new GameObject("GameRoot");
            var particleCatalog = LoadParticleCatalog();
            var authoring = gameRootGo.AddComponent<PrototypeSystemsAuthoring>();
            var sceneReferences = gameRootGo.AddComponent<PrototypeSceneReferences>();
            var installer = gameRootGo.AddComponent<GameInstaller>();
            var customizer = gameRootGo.AddComponent<CreativeSceneCustomizer>();
            var gameplayLoop = gameRootGo.AddComponent<GameplayLoop>();
            var gameBootstrapper = gameRootGo.AddComponent<GameBootstrapper>();
            var vfxService = gameRootGo.AddComponent<VfxService>();
            var cameraShakeService = gameRootGo.AddComponent<CameraShakeService>();
            var spawner = gameRootGo.AddComponent<AsteroidSpawner>();
            var vfxRoot = CreateChildTransform(gameRootGo.transform, "VfxRoot");

            authoring.Bind(
                sceneReferences,
                installer,
                customizer,
                gameplayLoop,
                gameBootstrapper,
                vfxService,
                cameraShakeService,
                spawner,
                vfxRoot);
            authoring.EnsureBuilt(config);

            installer.Bind(sceneReferences);
            customizer.Bind(config, sceneReferences, true, true, true);
            vfxService.ConfigurePools(
                CreateTimedEffectPool(vfxRoot, particleCatalog.AsteroidHitPrefab, "AsteroidHitPool", config.Vfx.PrefabEffectPoolSize),
                CreateTimedEffectPool(vfxRoot, particleCatalog.RocketExplosionPrefab, "RocketExplosionPool", config.Vfx.PrefabEffectPoolSize),
                CreateTimedEffectPool(vfxRoot, particleCatalog.LaunchBurstPrefab, "LaunchBurstPool", config.Vfx.PrefabEffectPoolSize),
                CreateTimedEffectPool(vfxRoot, particleCatalog.FogTransitionPrefab, "FogTransitionPool", config.Vfx.PrefabEffectPoolSize),
                CreateTimedEffectPool(vfxRoot, particleCatalog.FogZonePrefab, "FogZonePool", config.Vfx.PrefabEffectPoolSize),
                CreateTimedEffectPool(vfxRoot, particleCatalog.SpaceEntryPrefab, "SpaceEntryPool", config.Vfx.PrefabEffectPoolSize));

            return gameRootGo;
        }

        private static GameObject CreateWorldPrefab(PrototypeGameConfig config)
        {
            var worldRoot = new GameObject("WorldRoot");
            var particleCatalog = LoadParticleCatalog();
            var authoring = worldRoot.AddComponent<PrototypeWorldAuthoring>();
            var backgroundRoot = CreateChildTransform(worldRoot.transform, "BackgroundRoot");
            var backgroundController = backgroundRoot.gameObject.AddComponent<BackgroundController>();
            var spaceGradient = CreateSpriteObject("SpaceGradient", PrimitiveSpriteLibrary.SquareSprite, Color.white, -20, backgroundRoot);
            var upperAtmosphereGlow = CreateSpriteObject("UpperAtmosphereGlow", PrimitiveSpriteLibrary.SquareSprite, Color.white, -19, backgroundRoot);
            var planetCurvatureGlow = CreateSpriteObject("PlanetCurvatureGlow", PrimitiveSpriteLibrary.SquareSprite, Color.white, -18, backgroundRoot);
            var farStarsLayer = CreateBackgroundLayer(backgroundRoot, "StarsLayerFar");
            var middleStarsLayer = CreateBackgroundLayer(backgroundRoot, "StarsLayerMiddle");
            var nearStarsLayer = CreateBackgroundLayer(backgroundRoot, "StarsLayerNear");
            var cloudFarLayer = CreateBackgroundLayer(backgroundRoot, "CloudLayerFar");
            var cloudNearLayer = CreateBackgroundLayer(backgroundRoot, "CloudLayerNear");
            var nebulaLayer = CreateBackgroundLayer(backgroundRoot, "NebulaLayer");
            var launchSmokeLayer = CreateBackgroundLayer(backgroundRoot, "LaunchSmokeLayer");
            var spaceStars = InstantiateSpaceStars(backgroundRoot, particleCatalog);

            backgroundController.Bind(
                spaceGradient,
                upperAtmosphereGlow,
                planetCurvatureGlow,
                farStarsLayer,
                middleStarsLayer,
                nearStarsLayer,
                cloudFarLayer,
                cloudNearLayer,
                nebulaLayer,
                launchSmokeLayer,
                spaceStars);
            backgroundController.ApplyConfig(config);

            var globalVolume = CreateGlobalVolume(worldRoot.transform);
            var platformRoot = CreateChildTransform(worldRoot.transform, "PlatformRoot");
            var backgroundRenderer = backgroundController.BackgroundRenderer;
            var platformRenderer = CreateLaunchPlatform(platformRoot, config);

            if (config.Rocket.Prefab == null)
            {
                throw new InvalidOperationException("Prototype rocket prefab must exist before baking the world prefab.");
            }

            var rocketGo = (GameObject)PrefabUtility.InstantiatePrefab(config.Rocket.Prefab, worldRoot.transform);
            rocketGo.name = "Rocket";
            rocketGo.transform.position = new Vector3(0f, -3.2f, 0f);
            rocketGo.transform.localScale = new Vector3(config.Rocket.Size.x, config.Rocket.Size.y, 1f);
            var rocket = rocketGo.GetComponent<RocketController>();
            var rocketAuthoring = rocketGo.GetComponent<RocketAuthoring>();
            rocket?.Bind(rocketGo.GetComponent<Rigidbody2D>(), rocketAuthoring);
            rocketAuthoring?.Apply(config);

            authoring.Bind(
                backgroundRoot,
                backgroundController,
                backgroundRenderer,
                platformRenderer,
                globalVolume,
                rocket,
                rocketAuthoring);
            authoring.EnsureBuilt();

            return worldRoot;
        }

        private static Camera CreateMainCamera(Transform parent, PrototypeGameConfig config)
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.transform.SetParent(parent, false);
            cameraGo.tag = "MainCamera";

            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = config != null ? config.Camera.OrthographicSize : 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = config != null ? config.Presentation.BackgroundColor : Color.black;

            var additionalData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            additionalData.renderPostProcessing = true;
            additionalData.renderShadows = false;
            additionalData.volumeLayerMask = ~0;
            return camera;
        }

        private static EventSystem CreateEventSystem(Transform parent)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(parent, false);
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            return eventSystemGo.GetComponent<EventSystem>();
        }

        private static Volume CreateGlobalVolume(Transform parent)
        {
            var volumeGo = new GameObject("GlobalVolume");
            volumeGo.transform.SetParent(parent, false);

            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 1f;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/DefaultVolumeProfile.asset");
            return volume;
        }

        private static SpriteRenderer CreateLaunchPlatform(Transform platformRoot, PrototypeGameConfig config)
        {
            var groundBase = CreateSpriteObject("LaunchGroundBase", PrimitiveSpriteLibrary.SquareSprite, new Color(0.16f, 0.42f, 0.16f, 1f), 3, platformRoot);
            groundBase.transform.localScale = new Vector3(5.8f, 0.85f, 1f);
            groundBase.transform.localPosition = new Vector3(0f, -4.32f, 0f);

            var groundHighlight = CreateSpriteObject("LaunchGroundHighlight", PrimitiveSpriteLibrary.SquareSprite, new Color(0.28f, 0.62f, 0.24f, 0.85f), 4, platformRoot);
            groundHighlight.transform.localScale = new Vector3(3.2f, 0.18f, 1f);
            groundHighlight.transform.localPosition = new Vector3(0f, -4.03f, 0f);

            var platformVisual = CreateSpriteObject("LaunchPad", PrimitiveSpriteLibrary.SquareSprite, config.Presentation.PlatformColor, 5, platformRoot);
            platformVisual.transform.localScale = new Vector3(2.4f, 0.25f, 1f);
            platformVisual.transform.localPosition = new Vector3(0f, -4.1f, 0f);
            return platformVisual;
        }

        private static SpriteRenderer CreateSpriteObject(string name, Sprite sprite, Color color, int sortingOrder, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static BackgroundLayer CreateBackgroundLayer(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<BackgroundLayer>();
        }

        private static ParticleSystem InstantiateSpaceStars(Transform parent, ParticlePrefabCatalog particleCatalog)
        {
            var prefab = particleCatalog != null ? particleCatalog.SpaceStarsPrefab : null;
            if (prefab == null)
            {
                throw new InvalidOperationException("Particle catalog is missing the space stars prefab.");
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = "SpaceStars";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance.GetComponent<ParticleSystem>();
        }

        private static ParticlePrefabCatalog LoadParticleCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ParticlePrefabCatalog>(PrototypeScenePaths.ParticleCatalogPath);
            if (catalog == null)
            {
                throw new InvalidOperationException($"Missing particle catalog at '{PrototypeScenePaths.ParticleCatalogPath}'.");
            }

            return catalog;
        }

        private static Transform CreateChildTransform(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static PooledTimedEffect[] CreateTimedEffectPool(
            Transform parent,
            GameObject sourcePrefab,
            string poolName,
            int poolSize)
        {
            var count = Mathf.Max(1, poolSize);
            var result = new PooledTimedEffect[count];
            var poolRoot = CreateChildTransform(parent, poolName);

            for (var i = 0; i < count; i++)
            {
                result[i] = CreateTimedEffect(sourcePrefab, poolRoot, $"{poolName}_{i:D2}");
            }

            return result;
        }

        private static PooledTimedEffect CreateTimedEffect(GameObject sourcePrefab, Transform parent, string name)
        {
            GameObject instance;

            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Timed effect instance '{name}' requires a source prefab.");
            }

            instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, parent);
            instance.name = name;

            var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            var effect = instance.GetComponent<PooledTimedEffect>() ?? instance.AddComponent<PooledTimedEffect>();
            effect.Configure(particleSystems);
            instance.SetActive(false);
            return effect;
        }

        private static void EnsurePrefabExists(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Missing UI prefab at '{path}'. Rebuild the UI prefab asset first.");
            }
        }

        private static void CreatePrefabIfMissing(string path, Func<GameObject> createRoot)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                return;
            }

            var root = createRoot();
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static T CreateButton<T>(
            string name,
            Transform parent,
            string label,
            Vector2 size,
            int fontSize,
            AnchorPreset anchorPreset,
            Vector2 anchoredPosition)
            where T : Button
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetAnchor(rect, anchorPreset);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);
            var canvasGroup = go.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            var button = go.AddComponent<T>();
            var text = CreateText("Label", go.transform, size, fontSize, TextAnchor.MiddleCenter, label, AnchorPreset.MiddleCenter, Vector2.zero);
            text.color = Color.black;
            return button;
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

        private static Text CreateText(
            string name,
            Transform parent,
            Vector2 size,
            int fontSize,
            TextAnchor alignment,
            string value,
            AnchorPreset anchorPreset,
            Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetAnchor(rect, anchorPreset);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = value;
            text.color = Color.white;
            return text;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetAnchor(RectTransform rect, AnchorPreset preset)
        {
            var anchor = preset switch
            {
                AnchorPreset.BottomCenter => new Vector2(0.5f, 0f),
                AnchorPreset.BottomLeft => new Vector2(0f, 0f),
                AnchorPreset.BottomRight => new Vector2(1f, 0f),
                AnchorPreset.TopLeft => new Vector2(0f, 1f),
                AnchorPreset.TopRight => new Vector2(1f, 1f),
                AnchorPreset.TopCenter => new Vector2(0.5f, 1f),
                _ => new Vector2(0.5f, 0.5f)
            };

            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
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
