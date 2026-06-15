using System;
using System.Linq;
using SpaceFlight2D.Game;
using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEditor;
using UnityEngine;

namespace SpaceFlight2D.Editor
{
    public static class PrototypePrefabFactory
    {
        public static void EnsureGameplayPrefabs()
        {
            EnsureFolder(PrototypeScenePaths.PrefabFolder);
            EnsureFolder(PrototypeScenePaths.ParticlePrefabFolder);

            var particleCatalog = EnsureParticleCatalog();
            EnsureParticlePrefabs(particleCatalog);
            EnsureRocketPrefab(particleCatalog);
            EnsureAsteroidPrefab();
            EnsureDefaultPrototypePrefabReferences();

            EditorUtility.SetDirty(particleCatalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureRocketPrefab(ParticlePrefabCatalog particleCatalog)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeScenePaths.RocketPrefabPath) != null)
            {
                return;
            }

            var config = AssetDatabase.LoadAssetAtPath<PrototypeGameConfig>(PrototypeScenePaths.DefaultConfigPath);
            if (config == null)
            {
                throw new InvalidOperationException($"Missing prototype config at '{PrototypeScenePaths.DefaultConfigPath}'.");
            }

            var root = new GameObject("Rocket");
            try
            {
                var rigidbody = root.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 0f;
                rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rigidbody.freezeRotation = true;

                var collider = root.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(0.55f, 1.5f);

                var visualRoot = new GameObject("VisualRoot");
                visualRoot.transform.SetParent(root.transform, false);
                var bodyVisual = new GameObject("BodyVisual");
                bodyVisual.transform.SetParent(visualRoot.transform, false);
                var body = bodyVisual.AddComponent<SpriteRenderer>();
                body.sortingOrder = 10;

                var vfxRoot = new GameObject("VfxRoot");
                vfxRoot.transform.SetParent(root.transform, false);
                var trailGo = new GameObject("Trail");
                trailGo.transform.SetParent(vfxRoot.transform, false);
                var trail = trailGo.AddComponent<TrailRenderer>();
                trail.alignment = LineAlignment.View;
                trail.sharedMaterial = EnsureTrailMaterial();
                trail.widthMultiplier = 1f;
                trail.sortingOrder = 5;

                var rocketAuthoring = root.AddComponent<RocketAuthoring>();
                var rocketController = root.AddComponent<RocketController>();
                var engineParticles = InstantiateRocketEngineParticles(vfxRoot.transform, particleCatalog);

                body.sprite = config.Rocket.VisualSprite != null
                    ? config.Rocket.VisualSprite
                    : PrimitiveSpriteLibrary.SquareSprite;
                body.color = config.Rocket.Color;
                root.transform.localScale = new Vector3(config.Rocket.Size.x, config.Rocket.Size.y, 1f);
                rocketAuthoring.Bind(body, trail, engineParticles, collider, visualRoot.transform);
                rocketController.Bind(rigidbody, rocketAuthoring);
                rocketAuthoring.Apply(config);

                SavePrefab(root, PrototypeScenePaths.RocketPrefabPath);
                config.Rocket.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeScenePaths.RocketPrefabPath);
                EditorUtility.SetDirty(config);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void EnsureAsteroidPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeScenePaths.AsteroidPrefabPath) != null)
            {
                return;
            }

            var config = AssetDatabase.LoadAssetAtPath<PrototypeGameConfig>(PrototypeScenePaths.DefaultConfigPath);
            if (config == null)
            {
                throw new InvalidOperationException($"Missing prototype config at '{PrototypeScenePaths.DefaultConfigPath}'.");
            }

            var root = new GameObject("Asteroid");
            try
            {
                var collider = root.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = 0.5f;

                var visualRoot = new GameObject("VisualRoot");
                visualRoot.transform.SetParent(root.transform, false);
                var bodyVisual = new GameObject("BodyVisual");
                bodyVisual.transform.SetParent(visualRoot.transform, false);
                var renderer = bodyVisual.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 8;

                var asteroidAuthoring = root.AddComponent<AsteroidAuthoring>();
                var asteroid = root.AddComponent<Asteroid>();

                renderer.sprite = config.Asteroids.VisualSprite != null
                    ? config.Asteroids.VisualSprite
                    : PrimitiveSpriteLibrary.CircleSprite;
                renderer.color = config.Asteroids.Large.Color;
                asteroidAuthoring.Bind(renderer, collider, visualRoot.transform);
                asteroid.Bind(asteroidAuthoring);
                asteroidAuthoring.Apply(new AsteroidSpawnSettings
                {
                    VisualSprite = config.Asteroids.VisualSprite,
                    Size = config.Asteroids.Large.SizeRange,
                    Color = config.Asteroids.Large.Color,
                    ScoreReward = config.Asteroids.Large.ScoreReward
                });

                SavePrefab(root, PrototypeScenePaths.AsteroidPrefabPath);
                config.Asteroids.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeScenePaths.AsteroidPrefabPath);
                EditorUtility.SetDirty(config);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void EnsureDefaultPrototypePrefabReferences()
        {
            var rocketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeScenePaths.RocketPrefabPath);
            var asteroidPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeScenePaths.AsteroidPrefabPath);

            foreach (var configPath in PrototypeScenePaths.AllPrototypeConfigPaths)
            {
                var config = AssetDatabase.LoadAssetAtPath<PrototypeGameConfig>(configPath);
                if (config == null)
                {
                    continue;
                }

                var changed = false;
                if (rocketPrefab != null && config.Rocket.Prefab == null)
                {
                    config.Rocket.Prefab = rocketPrefab;
                    changed = true;
                }

                if (asteroidPrefab != null && config.Asteroids.Prefab == null)
                {
                    config.Asteroids.Prefab = asteroidPrefab;
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(config);
                }
            }
        }

        private static ParticlePrefabCatalog EnsureParticleCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ParticlePrefabCatalog>(PrototypeScenePaths.ParticleCatalogPath);
            if (catalog != null)
            {
                return catalog;
            }

            catalog = ScriptableObject.CreateInstance<ParticlePrefabCatalog>();
            AssetDatabase.CreateAsset(catalog, PrototypeScenePaths.ParticleCatalogPath);
            return catalog;
        }

        private static void EnsureParticlePrefabs(ParticlePrefabCatalog catalog)
        {
            catalog.RocketEnginePrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.RocketEnginePrefabPath, CreateRocketEnginePrefabRoot);
            catalog.SpaceStarsPrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.SpaceStarsPrefabPath, CreateSpaceStarsPrefabRoot);
            catalog.AsteroidHitPrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.AsteroidHitPrefabPath, CreateAsteroidHitPrefabRoot);
            catalog.RocketExplosionPrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.RocketExplosionPrefabPath, CreateRocketExplosionPrefabRoot);
            catalog.LaunchBurstPrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.LaunchBurstPrefabPath, CreateLaunchBurstPrefabRoot);
            catalog.FogTransitionPrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.FogTransitionPrefabPath, CreateFogTransitionPrefabRoot);
            catalog.FogZonePrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.FogZonePrefabPath, CreateFogZonePrefabRoot);
            catalog.SpaceEntryPrefab ??= CreatePrefabAssetIfMissing(PrototypeScenePaths.SpaceEntryPrefabPath, CreateSpaceEntryPrefabRoot);
        }

        private static GameObject CreateRocketEnginePrefabRoot()
        {
            var root = new GameObject("RocketEngine");
            var particles = root.AddComponent<ParticleSystem>();
            ConfigureRocketEngineParticleSystem(particles);
            return root;
        }

        private static GameObject CreateSpaceStarsPrefabRoot()
        {
            var root = new GameObject("SpaceStars");
            var particles = root.AddComponent<ParticleSystem>();
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 6;
            renderer.sharedMaterial = EnsureSpaceStarsMaterial();

            var starSprite = LoadSpaceStarSprite();
            if (starSprite != null)
            {
                ConfigureSpaceStarsSprite(particles, starSprite);
            }

            ConfigureSpaceStarsParticleSystem(particles);
            return root;
        }

        private static GameObject CreateAsteroidHitPrefabRoot()
        {
            var root = new GameObject("AsteroidHit");
            AddParticleSystem(root.transform, "Burst", Vector3.zero, particles =>
            {
                ConfigureBurstParticleSystem(particles, 14, 0.1f, 0.5f, Color.white, 1.5f, 4f, 0.08f, 0.25f);
            });
            return root;
        }

        private static GameObject CreateRocketExplosionPrefabRoot()
        {
            var root = new GameObject("RocketExplosion");
            AddParticleSystem(root.transform, "FireCore", Vector3.zero, particles =>
            {
                ConfigureBurstParticleSystem(particles, 36, 0.18f, 0.85f, new Color(1f, 0.45f, 0.2f, 1f), 2.5f, 6f, 0.2f, 0.6f);
            });
            AddParticleSystem(root.transform, "Flash", Vector3.zero, particles =>
            {
                ConfigureBurstParticleSystem(particles, 24, 0.14f, 0.65f, new Color(1f, 0.85f, 0.45f, 1f), 2.2f, 4.6f, 0.12f, 0.38f);
            });
            return root;
        }

        private static GameObject CreateLaunchBurstPrefabRoot()
        {
            var root = new GameObject("LaunchBurst");
            AddParticleSystem(root.transform, "Burst", Vector3.zero, particles =>
            {
                ConfigureBurstParticleSystem(particles, 26, 0.08f, 0.45f, new Color(1f, 0.74f, 0.2f, 1f), 2f, 4f, 0.15f, 0.35f);
            });
            return root;
        }

        private static GameObject CreateFogTransitionPrefabRoot()
        {
            var root = new GameObject("FogTransition");
            AddParticleSystem(root.transform, "Mist", Vector3.zero, particles =>
            {
                ConfigureBurstParticleSystem(particles, 42, 0.3f, 0.9f, new Color(0.95f, 0.98f, 1f, 1f), 1.5f, 3.8f, 0.16f, 0.5f);
            });
            AddParticleSystem(root.transform, "FogFill", new Vector3(0f, 0.12f, 0f), particles =>
            {
                ConfigureBurstParticleSystem(particles, 32, 0.42f, 1f, new Color(0.92f, 0.94f, 0.96f, 1f), 0.9f, 2.6f, 0.12f, 0.34f);
            });
            return root;
        }

        private static GameObject CreateFogZonePrefabRoot()
        {
            var root = new GameObject("FogZone");
            AddParticleSystem(root.transform, "Mist", Vector3.zero, particles =>
            {
                ConfigureBurstParticleSystem(particles, 24, 0.38f, 0.8f, new Color(0.95f, 0.98f, 1f, 0.9f), 0.55f, 2.1f, 0.14f, 0.38f);
            });
            return root;
        }

        private static GameObject CreateSpaceEntryPrefabRoot()
        {
            var root = new GameObject("SpaceEntry");
            AddParticleSystem(root.transform, "Glow", Vector3.zero, particles =>
            {
                ConfigureBurstParticleSystem(particles, 28, 0.18f, 0.85f, new Color(0.16f, 0.22f, 0.34f, 0.8f), 1.6f, 3.8f, 0.1f, 0.28f);
            });
            AddParticleSystem(root.transform, "Stars", new Vector3(0f, 0.06f, 0f), particles =>
            {
                ConfigureBurstParticleSystem(particles, 22, 0.12f, 0.65f, Color.white, 1.9f, 4.2f, 0.08f, 0.22f);
            });
            return root;
        }

        private static GameObject CreatePrefabAssetIfMissing(string assetPath, Func<GameObject> createRoot)
        {
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            var root = createRoot();
            try
            {
                SavePrefab(root, assetPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }

        private static void SavePrefab(GameObject root, string assetPath)
        {
            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
        }

        private static Material EnsureTrailMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(PrototypeScenePaths.TrailMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, PrototypeScenePaths.TrailMaterialPath);
            return material;
        }

        private static Material EnsureSpaceStarsMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(PrototypeScenePaths.SpaceStarsMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Particles/Standard Unlit"));
            AssetDatabase.CreateAsset(material, PrototypeScenePaths.SpaceStarsMaterialPath);
            return material;
        }

        private static ParticleSystem InstantiateRocketEngineParticles(Transform parent, ParticlePrefabCatalog particleCatalog)
        {
            if (particleCatalog == null || particleCatalog.RocketEnginePrefab == null)
            {
                throw new InvalidOperationException("Particle catalog is missing the rocket engine prefab.");
            }

            var engineGo = (GameObject)PrefabUtility.InstantiatePrefab(particleCatalog.RocketEnginePrefab, parent);
            engineGo.name = "EngineParticles";
            engineGo.transform.localPosition = new Vector3(0f, -0.95f, 0f);
            engineGo.transform.localRotation = Quaternion.identity;
            engineGo.transform.localScale = Vector3.one;
            return engineGo.GetComponent<ParticleSystem>();
        }

        private static void AddParticleSystem(Transform parent, string name, Vector3 localPosition, Action<ParticleSystem> configure)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var particles = go.AddComponent<ParticleSystem>();
            configure(particles);
        }

        private static void ConfigureRocketEngineParticleSystem(ParticleSystem particles)
        {
            var main = particles.main;
            main.duration = 0.6f;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = 0.16f;
            main.startSpeed = 1.5f;
            main.startSize = 0.16f;
            main.startColor = new Color(1f, 0.63f, 0.2f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = particles.emission;
            emission.rateOverTime = 28f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 14f;
            shape.radius = 0.05f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static void ConfigureBurstParticleSystem(
            ParticleSystem particles,
            short count,
            float radius,
            float duration,
            Color color,
            float startSpeedMin,
            float startSpeedMax,
            float startSizeMin,
            float startSizeMax)
        {
            var main = particles.main;
            main.duration = duration;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(duration * 0.3f, duration * 0.65f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeedMin, startSpeedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(startSizeMin, startSizeMax);
            main.startColor = color;
            main.maxParticles = count + 4;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = particles.shape;
            shape.shapeType = radius < 0.09f ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Circle;
            shape.radius = radius;
            if (shape.shapeType == ParticleSystemShapeType.Cone)
            {
                shape.angle = 24f;
                shape.rotation = new Vector3(90f, 0f, 0f);
            }

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static void ConfigureSpaceStarsParticleSystem(ParticleSystem particles)
        {
            var main = particles.main;
            main.duration = 5f;
            main.loop = true;
            main.playOnAwake = false;
            main.maxParticles = 96;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.4f, 5.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.9f, 1.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.22f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.85f), new Color(1f, 0.98f, 0.9f, 1f));

            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 26f;

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(8.4f, 1.6f, 0.2f);
            shape.position = new Vector3(0f, 6.4f, 0f);

            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.28f, -0.08f);

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.98f, 0.92f, 1f), 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.95f, 0.18f),
                    new GradientAlphaKey(0.95f, 0.82f),
                    new GradientAlphaKey(0f, 1f)
                }
            });

            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.08f;
            noise.frequency = 0.35f;
            noise.scrollSpeed = 0.12f;

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static void ConfigureSpaceStarsSprite(ParticleSystem particles, Sprite starSprite)
        {
            var textureSheetAnimation = particles.textureSheetAnimation;
            textureSheetAnimation.enabled = true;
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            textureSheetAnimation.numTilesX = 1;
            textureSheetAnimation.numTilesY = 1;
            textureSheetAnimation.AddSprite(starSprite);
        }

        private static Sprite LoadSpaceStarSprite()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Graphics/star.png");
            return assets
                .OfType<Sprite>()
                .Where(sprite => sprite.rect.width > 0f && sprite.rect.height > 0f)
                .OrderBy(sprite => sprite.rect.width * sprite.rect.height)
                .FirstOrDefault();
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
    }
}
