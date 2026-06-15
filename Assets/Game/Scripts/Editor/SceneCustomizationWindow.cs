using System;
using SpaceFlight2D.Game;
using SpaceFlight2D.Game.Bootstrap;
using SpaceFlight2D.Game.Config;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpaceFlight2D.Editor
{
    public sealed class SceneCustomizationWindow : EditorWindow
    {
        private const string AsteroidHitPoolName = "AsteroidHitPool";
        private const string RocketExplosionPoolName = "RocketExplosionPool";
        private const string LaunchBurstPoolName = "LaunchBurstPool";
        private const string FogTransitionPoolName = "FogTransitionPool";
        private const string FogZonePoolName = "FogZonePool";
        private const string SpaceEntryPoolName = "SpaceEntryPool";

        private PrototypeGameConfig _config;
        private CreativeSceneCustomizer _customizer;
        private PrototypeSceneReferences _sceneReferences;
        private ParticlePrefabCatalog _particleCatalog;
        private bool _hasLoadedValues;

        private GameObject _rocketPrefab;
        private Vector2 _rocketSize;
        private Color _rocketColor;
        private Color _trailStartColor;
        private Color _trailEndColor;

        private GameObject _asteroidPrefab;
        private Color _smallAsteroidColor;
        private Color _mediumAsteroidColor;
        private Color _largeAsteroidColor;

        private Color _backgroundColor;
        private Color _spaceTopColor;
        private Color _spaceBottomColor;
        private Color _starTint;

        private GameObject _spaceStarsPrefab;
        private GameObject _asteroidHitPrefab;
        private GameObject _rocketExplosionPrefab;

        [MenuItem("Tools/Rocket/Scene Customization")]
        public static void ShowWindow()
        {
            GetWindow<SceneCustomizationWindow>("Scene Customization");
        }

        private void OnFocus()
        {
            RefreshBindings();
            EnsureDefaultBindings();
            if (!_hasLoadedValues)
            {
                LoadEditableValues();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scene Customization", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Настройка сохраняется в PrototypeGameConfig и ParticlePrefabCatalog. Превью применяет изменения к открытой сцене, а пересборка обновляет generated prefabs и VFX pools.",
                MessageType.Info);

            DrawBindings();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Load Current"))
                {
                    RefreshBindings();
                    EnsureDefaultBindings();
                    LoadEditableValues();
                }

                if (GUILayout.Button("Save To Assets"))
                {
                    ApplyToAssets();
                }
            }

            if (GUILayout.Button("Apply Default Palette"))
            {
                ApplyDefaultPalette();
            }

            DrawRocketSection();
            DrawAsteroidSection();
            DrawSpaceSection();
            DrawVfxSection();

            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Preview In Scene"))
            {
                ApplyToAssets();
                ApplyScenePreview();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild VFX Pools In Scene"))
                {
                    ApplyToAssets();
                    RebuildSceneVfxPools();
                    ReplaceSceneSpaceStars();
                    ApplyScenePreview();
                }

                if (GUILayout.Button("Rebuild Prototype Prefabs"))
                {
                    ApplyToAssets();
                    PrototypePrefabFactory.EnsureGameplayPrefabs();
                    PrototypeScenePrefabFactory.EnsureScenePrefabs();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        private void DrawBindings()
        {
            EditorGUI.BeginChangeCheck();
            var nextConfig = (PrototypeGameConfig)EditorGUILayout.ObjectField("Game Config", _config, typeof(PrototypeGameConfig), false);
            var nextCustomizer = (CreativeSceneCustomizer)EditorGUILayout.ObjectField("Scene Customizer", _customizer, typeof(CreativeSceneCustomizer), true);
            var nextSceneReferences = (PrototypeSceneReferences)EditorGUILayout.ObjectField("Scene References", _sceneReferences, typeof(PrototypeSceneReferences), true);
            var nextParticleCatalog = (ParticlePrefabCatalog)EditorGUILayout.ObjectField("Particle Catalog", _particleCatalog, typeof(ParticlePrefabCatalog), false);

            if (EditorGUI.EndChangeCheck())
            {
                var configChanged = nextConfig != _config || nextParticleCatalog != _particleCatalog;
                _config = nextConfig;
                _customizer = nextCustomizer;
                _sceneReferences = nextSceneReferences;
                _particleCatalog = nextParticleCatalog;

                if (configChanged)
                {
                    EnsureDefaultBindings();
                    LoadEditableValues();
                }
            }

            EditorGUILayout.Space(6f);
        }

        private void DrawRocketSection()
        {
            EditorGUILayout.LabelField("Rocket", EditorStyles.boldLabel);
            _rocketPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _rocketPrefab, typeof(GameObject), false);
            _rocketSize = EditorGUILayout.Vector2Field("Size", _rocketSize);
            _rocketColor = EditorGUILayout.ColorField("Color", _rocketColor);
            _trailStartColor = EditorGUILayout.ColorField("Trail Start", _trailStartColor);
            _trailEndColor = EditorGUILayout.ColorField("Trail End", _trailEndColor);

            if (GUILayout.Button("Reset Rocket Colors"))
            {
                ResetRocketColors();
            }

            EditorGUILayout.Space(6f);
        }

        private void DrawAsteroidSection()
        {
            EditorGUILayout.LabelField("Asteroids", EditorStyles.boldLabel);
            _asteroidPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _asteroidPrefab, typeof(GameObject), false);
            _smallAsteroidColor = EditorGUILayout.ColorField("Small Color", _smallAsteroidColor);
            _mediumAsteroidColor = EditorGUILayout.ColorField("Medium Color", _mediumAsteroidColor);
            _largeAsteroidColor = EditorGUILayout.ColorField("Large Color", _largeAsteroidColor);

            if (GUILayout.Button("Reset Asteroid Colors"))
            {
                ResetAsteroidColors();
            }

            EditorGUILayout.Space(6f);
        }

        private void DrawSpaceSection()
        {
            EditorGUILayout.LabelField("Space", EditorStyles.boldLabel);
            _backgroundColor = EditorGUILayout.ColorField("Camera Background", _backgroundColor);
            _spaceTopColor = EditorGUILayout.ColorField("Space Top", _spaceTopColor);
            _spaceBottomColor = EditorGUILayout.ColorField("Space Bottom", _spaceBottomColor);
            _starTint = EditorGUILayout.ColorField("Stars Tint", _starTint);

            if (GUILayout.Button("Reset Space Colors"))
            {
                ResetSpaceColors();
            }

            EditorGUILayout.Space(6f);
        }

        private void DrawVfxSection()
        {
            EditorGUILayout.LabelField("VFX Prefabs", EditorStyles.boldLabel);
            _spaceStarsPrefab = (GameObject)EditorGUILayout.ObjectField("Stars Prefab", _spaceStarsPrefab, typeof(GameObject), false);
            _asteroidHitPrefab = (GameObject)EditorGUILayout.ObjectField("Asteroid Destroy", _asteroidHitPrefab, typeof(GameObject), false);
            _rocketExplosionPrefab = (GameObject)EditorGUILayout.ObjectField("Rocket Destroy", _rocketExplosionPrefab, typeof(GameObject), false);
        }

        private void RefreshBindings()
        {
            _customizer = _customizer != null ? _customizer : UnityEngine.Object.FindFirstObjectByType<CreativeSceneCustomizer>();
            _sceneReferences = _sceneReferences != null
                ? _sceneReferences
                : UnityEngine.Object.FindFirstObjectByType<PrototypeSceneReferences>();

            if (_customizer != null)
            {
                _config = _customizer.Config;
            }
            else if (_sceneReferences != null)
            {
                _config = _sceneReferences.Config;
            }

            if (_particleCatalog == null)
            {
                _particleCatalog = AssetDatabase.LoadAssetAtPath<ParticlePrefabCatalog>(PrototypeScenePaths.ParticleCatalogPath);
            }
        }

        private void EnsureDefaultBindings()
        {
            if (_config == null && _customizer == null && _sceneReferences == null)
            {
                return;
            }

            var host = GetOrCreateBindingsHost();
            if (host == null)
            {
                return;
            }

            if (_sceneReferences == null)
            {
                _sceneReferences = Undo.AddComponent<PrototypeSceneReferences>(host);
            }

            if (_customizer == null)
            {
                _customizer = Undo.AddComponent<CreativeSceneCustomizer>(host);
            }

            if (_sceneReferences != null)
            {
                _sceneReferences.SetConfig(_config);
                _sceneReferences.SetCustomizer(_customizer);
                EditorUtility.SetDirty(_sceneReferences);
            }

            if (_customizer != null)
            {
                _customizer.Bind(_config, _sceneReferences, true, true, true);
                EditorUtility.SetDirty(_customizer);
            }

            EditorSceneManager.MarkSceneDirty(host.scene);
        }

        private GameObject GetOrCreateBindingsHost()
        {
            if (_sceneReferences != null)
            {
                return _sceneReferences.gameObject;
            }

            if (_customizer != null)
            {
                return _customizer.gameObject;
            }

            var systemsAuthoring = UnityEngine.Object.FindFirstObjectByType<PrototypeSystemsAuthoring>();
            if (systemsAuthoring != null)
            {
                return systemsAuthoring.gameObject;
            }

            var existingRoot = GameObject.Find("GameRoot");
            if (existingRoot != null)
            {
                return existingRoot;
            }

            var host = GameObject.Find("SceneCustomizationDefaults");
            if (host != null)
            {
                return host;
            }

            host = new GameObject("SceneCustomizationDefaults");
            Undo.RegisterCreatedObjectUndo(host, "Create scene customization defaults");
            return host;
        }

        private void LoadEditableValues()
        {
            if (_config == null)
            {
                return;
            }

            _hasLoadedValues = true;

            _rocketPrefab = _config.Rocket.Prefab;
            _rocketSize = _config.Rocket.Size;
            _rocketColor = _config.Rocket.Color;
            _trailStartColor = _config.Rocket.Trail.StartColor;
            _trailEndColor = _config.Rocket.Trail.EndColor;

            _asteroidPrefab = _config.Asteroids.Prefab;
            _smallAsteroidColor = _config.Asteroids.Small.Color;
            _mediumAsteroidColor = _config.Asteroids.Medium.Color;
            _largeAsteroidColor = _config.Asteroids.Large.Color;

            _backgroundColor = _config.Presentation.BackgroundColor;
            _spaceTopColor = _config.LaunchFlow.SpaceTopColor;
            _spaceBottomColor = _config.LaunchFlow.SpaceBottomColor;
            _starTint = _config.Background.StarTint;

            if (_particleCatalog != null)
            {
                _spaceStarsPrefab = _particleCatalog.SpaceStarsPrefab;
                _asteroidHitPrefab = _particleCatalog.AsteroidHitPrefab;
                _rocketExplosionPrefab = _particleCatalog.RocketExplosionPrefab;
            }
        }

        private void ApplyDefaultPalette()
        {
            ResetRocketColors();
            ResetAsteroidColors();
            ResetSpaceColors();
        }

        private void ResetRocketColors()
        {
            _rocketColor = DefaultColorPalette.RocketBody;
            _trailStartColor = DefaultColorPalette.RocketTrailStart;
            _trailEndColor = DefaultColorPalette.RocketTrailEnd;
        }

        private void ResetAsteroidColors()
        {
            _smallAsteroidColor = DefaultColorPalette.AsteroidSmall;
            _mediumAsteroidColor = DefaultColorPalette.AsteroidMedium;
            _largeAsteroidColor = DefaultColorPalette.AsteroidLarge;
        }

        private void ResetSpaceColors()
        {
            _backgroundColor = DefaultColorPalette.PresentationBackground;
            _spaceTopColor = DefaultColorPalette.SpaceTop;
            _spaceBottomColor = DefaultColorPalette.SpaceBottom;
            _starTint = DefaultColorPalette.StarTint;
        }

        private void ApplyToAssets()
        {
            if (_config == null)
            {
                return;
            }

            Undo.RecordObject(_config, "Apply scene customization");
            if (_particleCatalog != null)
            {
                Undo.RecordObject(_particleCatalog, "Apply particle customization");
            }

            _config.Rocket.Prefab = _rocketPrefab;
            _config.Rocket.Size = _rocketSize;
            _config.Rocket.Color = _rocketColor;
            _config.Rocket.Trail.StartColor = _trailStartColor;
            _config.Rocket.Trail.EndColor = _trailEndColor;

            _config.Asteroids.Prefab = _asteroidPrefab;
            var small = _config.Asteroids.Small;
            small.Color = _smallAsteroidColor;
            _config.Asteroids.Small = small;

            var medium = _config.Asteroids.Medium;
            medium.Color = _mediumAsteroidColor;
            _config.Asteroids.Medium = medium;

            var large = _config.Asteroids.Large;
            large.Color = _largeAsteroidColor;
            _config.Asteroids.Large = large;

            _config.Presentation.BackgroundColor = _backgroundColor;
            _config.LaunchFlow.SpaceTopColor = _spaceTopColor;
            _config.LaunchFlow.SpaceBottomColor = _spaceBottomColor;
            _config.Background.StarTint = _starTint;

            if (_particleCatalog != null)
            {
                _particleCatalog.SpaceStarsPrefab = _spaceStarsPrefab;
                _particleCatalog.AsteroidHitPrefab = _asteroidHitPrefab;
                _particleCatalog.RocketExplosionPrefab = _rocketExplosionPrefab;
                EditorUtility.SetDirty(_particleCatalog);
            }

            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }

        private void ApplyScenePreview()
        {
            if (_sceneReferences == null)
            {
                RefreshBindings();
            }

            ResolveSceneReferences();

            if (_config == null)
            {
                return;
            }

            ReplaceSceneRocket();
            _customizer?.Apply();

            RebuildSceneAsteroidPool();
            ReplaceSceneSpaceStars();
            _sceneReferences?.BackgroundController?.ApplyConfig(_config);

            if (_sceneReferences != null)
            {
                EditorSceneManager.MarkSceneDirty(_sceneReferences.gameObject.scene);
            }
        }

        private void ReplaceSceneRocket()
        {
            if (_sceneReferences == null || _rocketPrefab == null)
            {
                return;
            }

            var currentRocket = _sceneReferences.Rocket != null
                ? _sceneReferences.Rocket
                : UnityEngine.Object.FindFirstObjectByType<RocketController>();
            if (currentRocket != null && PrefabUtility.GetCorrespondingObjectFromSource(currentRocket.gameObject) == _rocketPrefab)
            {
                ApplyRocketVisuals(currentRocket);
                return;
            }

            var parent = currentRocket != null ? currentRocket.transform.parent : ResolveRocketParent();
            if (parent == null)
            {
                return;
            }

            var localPosition = currentRocket != null ? currentRocket.transform.localPosition : new Vector3(0f, -3.2f, 0f);
            var localRotation = currentRocket != null ? currentRocket.transform.localRotation : Quaternion.identity;

            if (currentRocket != null)
            {
                Undo.DestroyObjectImmediate(currentRocket.gameObject);
            }

            var rocketGo = (GameObject)PrefabUtility.InstantiatePrefab(_rocketPrefab, parent);
            rocketGo.name = "Rocket";
            rocketGo.transform.localPosition = localPosition;
            rocketGo.transform.localRotation = localRotation;
            rocketGo.transform.localScale = new Vector3(_config.Rocket.Size.x, _config.Rocket.Size.y, 1f);
            Undo.RegisterCreatedObjectUndo(rocketGo, "Replace rocket");

            var rocket = rocketGo.GetComponent<RocketController>();
            var rocketAuthoring = rocketGo.GetComponent<RocketAuthoring>();
            if (rocket != null)
            {
                rocket.Bind(rocketGo.GetComponent<Rigidbody2D>(), rocketAuthoring);
                _sceneReferences.SetRocket(rocket);
            }

            ApplyRocketVisuals(rocket);
            EditorUtility.SetDirty(_sceneReferences);
        }

        private void ApplyRocketVisuals(RocketController rocket)
        {
            if (rocket == null)
            {
                return;
            }

            rocket.transform.localScale = new Vector3(_config.Rocket.Size.x, _config.Rocket.Size.y, 1f);
            var rocketAuthoring = rocket.GetComponent<RocketAuthoring>();
            rocketAuthoring?.Apply(_config);
            EditorUtility.SetDirty(rocket.transform);
            if (rocketAuthoring != null)
            {
                EditorUtility.SetDirty(rocketAuthoring);
            }
        }

        private void RebuildSceneAsteroidPool()
        {
            if (_sceneReferences?.AsteroidRoot == null || _sceneReferences.Spawner == null || _config == null)
            {
                return;
            }

            var poolRoot = _sceneReferences.AsteroidRoot.Find("AsteroidPoolRoot");
            if (poolRoot != null)
            {
                Undo.DestroyObjectImmediate(poolRoot.gameObject);
            }

            var newPoolRoot = new GameObject("AsteroidPoolRoot").transform;
            newPoolRoot.SetParent(_sceneReferences.AsteroidRoot, false);
            Undo.RegisterCreatedObjectUndo(newPoolRoot.gameObject, "Rebuild asteroid pool");

            var asteroids = new Asteroid[Mathf.Max(1, _config.Asteroids.InitialPoolSize)];
            for (var i = 0; i < asteroids.Length; i++)
            {
                asteroids[i] = CreateSceneAsteroidInstance(newPoolRoot, i);
            }

            _sceneReferences.Spawner.Bind(_sceneReferences.AsteroidRoot, _sceneReferences.MainCamera, asteroids);
            EditorUtility.SetDirty(_sceneReferences.Spawner);
        }

        private void ReplaceSceneSpaceStars()
        {
            if (_sceneReferences?.BackgroundController == null || _spaceStarsPrefab == null)
            {
                return;
            }

            var parent = _sceneReferences.BackgroundController.transform;
            var existing = parent.Find("SpaceStars");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(_spaceStarsPrefab, parent);
            instance.name = "SpaceStars";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(instance, "Replace space stars");

            var particleSystem = instance.GetComponent<ParticleSystem>();
            _sceneReferences.BackgroundController.SetSpaceStarsParticles(particleSystem);
            _sceneReferences.BackgroundController.ApplyConfig(_config);
        }

        private void RebuildSceneVfxPools()
        {
            if (_sceneReferences?.VfxService == null || _particleCatalog == null || _config == null)
            {
                return;
            }

            var root = ResolveVfxRoot();
            if (root == null)
            {
                return;
            }

            DestroyPoolRoot(root, AsteroidHitPoolName);
            DestroyPoolRoot(root, RocketExplosionPoolName);
            DestroyPoolRoot(root, LaunchBurstPoolName);
            DestroyPoolRoot(root, FogTransitionPoolName);
            DestroyPoolRoot(root, FogZonePoolName);
            DestroyPoolRoot(root, SpaceEntryPoolName);

            var poolSize = Mathf.Max(1, _config.Vfx.PrefabEffectPoolSize);
            _sceneReferences.VfxService.ConfigurePools(
                CreateTimedEffectPool(root, _particleCatalog.AsteroidHitPrefab, AsteroidHitPoolName, poolSize),
                CreateTimedEffectPool(root, _particleCatalog.RocketExplosionPrefab, RocketExplosionPoolName, poolSize),
                CreateTimedEffectPool(root, _particleCatalog.LaunchBurstPrefab, LaunchBurstPoolName, poolSize),
                CreateTimedEffectPool(root, _particleCatalog.FogTransitionPrefab, FogTransitionPoolName, poolSize),
                CreateTimedEffectPool(root, _particleCatalog.FogZonePrefab, FogZonePoolName, poolSize),
                CreateTimedEffectPool(root, _particleCatalog.SpaceEntryPrefab, SpaceEntryPoolName, poolSize));
        }

        private static void DestroyPoolRoot(Transform parent, string poolName)
        {
            var poolRoot = parent.Find(poolName);
            if (poolRoot != null)
            {
                Undo.DestroyObjectImmediate(poolRoot.gameObject);
            }
        }

        private static PooledTimedEffect[] CreateTimedEffectPool(Transform parent, GameObject sourcePrefab, string poolName, int poolSize)
        {
            if (sourcePrefab == null)
            {
                return Array.Empty<PooledTimedEffect>();
            }

            var count = Mathf.Max(1, poolSize);
            var poolRoot = new GameObject(poolName).transform;
            poolRoot.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(poolRoot.gameObject, $"Create {poolName}");

            var result = new PooledTimedEffect[count];
            for (var i = 0; i < count; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, poolRoot);
                instance.name = $"{poolName}_{i:D2}";
                instance.SetActive(false);
                Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");

                var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
                var effect = instance.GetComponent<PooledTimedEffect>() ?? instance.AddComponent<PooledTimedEffect>();
                effect.Configure(particleSystems);
                result[i] = effect;
            }

            return result;
        }

        private Asteroid CreateSceneAsteroidInstance(Transform parent, int index)
        {
            if (_config.Asteroids.Prefab != null)
            {
                var asteroidGo = (GameObject)PrefabUtility.InstantiatePrefab(_config.Asteroids.Prefab, parent);
                asteroidGo.name = $"Asteroid_{index:D2}";
                asteroidGo.SetActive(false);
                Undo.RegisterCreatedObjectUndo(asteroidGo, $"Create {asteroidGo.name}");
                var asteroid = asteroidGo.GetComponent<Asteroid>();
                if (asteroid != null)
                {
                    return asteroid;
                }

                Undo.DestroyObjectImmediate(asteroidGo);
            }

            var fallbackGo = new GameObject($"Asteroid_{index:D2}");
            fallbackGo.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(fallbackGo, $"Create {fallbackGo.name}");

            var collider = fallbackGo.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(fallbackGo.transform, false);
            var bodyVisual = new GameObject("BodyVisual");
            bodyVisual.transform.SetParent(visualRoot.transform, false);
            var renderer = bodyVisual.AddComponent<SpriteRenderer>();
            renderer.sprite = _config.Asteroids.VisualSprite != null
                ? _config.Asteroids.VisualSprite
                : PrimitiveSpriteLibrary.CircleSprite;
            renderer.sortingOrder = 8;

            var authoring = fallbackGo.AddComponent<AsteroidAuthoring>();
            var asteroidFallback = fallbackGo.AddComponent<Asteroid>();
            authoring.Bind(renderer, collider, visualRoot.transform);
            asteroidFallback.Bind(authoring);
            fallbackGo.SetActive(false);
            authoring.Apply(new SpaceFlight2D.Game.Runtime.AsteroidSpawnSettings
            {
                VisualSprite = _config.Asteroids.VisualSprite,
                Size = _config.Asteroids.Large.SizeRange,
                Color = _config.Asteroids.Large.Color,
                ScoreReward = _config.Asteroids.Large.ScoreReward
            });
            return asteroidFallback;
        }

        private void ResolveSceneReferences()
        {
            if (_sceneReferences == null)
            {
                return;
            }

            if (_sceneReferences.Rocket == null)
            {
                var rocket = UnityEngine.Object.FindFirstObjectByType<RocketController>();
                if (rocket != null)
                {
                    _sceneReferences.SetRocket(rocket);
                    EditorUtility.SetDirty(_sceneReferences);
                }
            }
        }

        private Transform ResolveRocketParent()
        {
            var worldAuthoring = UnityEngine.Object.FindFirstObjectByType<PrototypeWorldAuthoring>();
            if (worldAuthoring != null)
            {
                return worldAuthoring.transform;
            }

            var worldRoot = GameObject.Find("WorldRoot");
            if (worldRoot != null)
            {
                return worldRoot.transform;
            }

            return _sceneReferences != null ? _sceneReferences.GameRoot : null;
        }

        private Transform ResolveVfxRoot()
        {
            if (_sceneReferences?.VfxRoot != null)
            {
                return _sceneReferences.VfxRoot;
            }

            var systemsAuthoring = UnityEngine.Object.FindFirstObjectByType<PrototypeSystemsAuthoring>();
            if (systemsAuthoring != null && systemsAuthoring.VfxRoot != null)
            {
                return systemsAuthoring.VfxRoot;
            }

            var vfxRoot = GameObject.Find("VfxRoot");
            return vfxRoot != null ? vfxRoot.transform : _sceneReferences?.VfxService?.transform;
        }
    }
}
