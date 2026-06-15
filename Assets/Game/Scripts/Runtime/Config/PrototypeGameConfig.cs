using System;
using UnityEngine;

namespace SpaceFlight2D.Game.Config
{
    [CreateAssetMenu(
        fileName = "PrototypeGameConfig",
        menuName = "SpaceFlight2D/Prototype Game Config")]
    public sealed class PrototypeGameConfig : ScriptableObject
    {
        [field: SerializeField] public RocketSettings Rocket { get; private set; } = new();
        [field: SerializeField] public AsteroidSettings Asteroids { get; private set; } = new();
        [field: SerializeField] public VfxSettings Vfx { get; private set; } = new();
        [field: SerializeField] public UiSettings Ui { get; private set; } = new();
        [field: SerializeField] public PresentationSettings Presentation { get; private set; } = new();
        [field: SerializeField] public RecordingSettings Recording { get; private set; } = new();
        [field: SerializeField] public StartFlowSettings StartFlow { get; private set; } = new();
        [field: SerializeField] public CameraSettings Camera { get; private set; } = new();
        [field: SerializeField] public BackgroundSettings Background { get; private set; } = new();
        [field: SerializeField] public LaunchFlowSettings LaunchFlow { get; private set; } = new();

        private void OnEnable()
        {
            EnsureSections();
        }

        private void OnValidate()
        {
            EnsureSections();
        }

        [Serializable]
        public sealed class RocketSettings
        {
            public GameObject Prefab;
            public Sprite VisualSprite;
            [Min(0.1f)] public float Speed = 4.5f;
            [Min(0.1f)] public float HorizontalSpeed = 2.2f;
            [Min(0.1f)] public float RotationSpeed = 120f;
            [Min(0f)] public float MaxRotationAngle = 45f;
            [Min(0.1f)] public float AutoStabilizeSpeed = 80f;
            public Vector2 Size = new(0.8f, 1.8f);
            public Color Color = default;
            public TrailSettings Trail { get; private set; } = new();

            public void EnsureInitialized()
            {
                if (Color == default)
                {
                    Color = DefaultColorPalette.RocketBody;
                }

                Trail ??= new TrailSettings();
            }
        }

        [Serializable]
        public sealed class TrailSettings
        {
            public bool Enabled = true;
            public bool RocketEnabled = true;
            [Min(0f)] public float Time = 0.35f;
            [Min(0f)] public float StartWidth = 0.28f;
            [Min(0f)] public float EndWidth = 0.05f;
            public Color StartColor = default;
            public Color EndColor = default;
        }

        [Serializable]
        public sealed class AsteroidSettings
        {
            public GameObject Prefab;
            public Sprite VisualSprite;
            [Min(1)] public int InitialPoolSize = 18;
            [Min(0.1f)] public float SpawnIntervalMin = 0.7f;
            [Min(0.1f)] public float SpawnIntervalMax = 1.4f;
            public Vector2 SpawnXRange = new(-3.3f, 3.3f);
            public float SpawnOffsetY = 6f;
            public Vector2 SpeedRange = new(2.6f, 4.6f);
            public Vector2 RotationSpeedRange = new(-110f, 110f);
            [Min(0.1f)] public float Lifetime = 8f;
            public AsteroidSizeSettings Small = new(new Vector2(0.45f, 0.65f), 10, default);
            public AsteroidSizeSettings Medium = new(new Vector2(0.75f, 1f), 20, default);
            public AsteroidSizeSettings Large = new(new Vector2(1.1f, 1.4f), 35, default);
        }

        [Serializable]
        public struct AsteroidSizeSettings
        {
            public Vector2 SizeRange;
            public int ScoreReward;
            public Color Color;

            public AsteroidSizeSettings(Vector2 sizeRange, int scoreReward, Color color)
            {
                SizeRange = sizeRange;
                ScoreReward = scoreReward;
                Color = color;
            }
        }

        [Serializable]
        public sealed class VfxSettings
        {
            public bool Enabled = true;
            public bool ScreenShakeEnabled = true;
            public bool AsteroidHitFlashEnabled = true;
            public bool CameraColorFlashEnabled;
            [Min(0f)] public float AsteroidHitLifetime = 1.2f;
            [Min(0f)] public float AsteroidHitScale = 1f;
            [Min(0f)] public float RocketExplosionLifetime = 2f;
            [Min(0f)] public float RocketExplosionScale = 1.4f;
            [Min(0f)] public float LaunchBurstLifetime = 0.8f;
            [Min(0f)] public float LaunchBurstScale = 1f;
            [Min(0f)] public float SmallShakeDuration = 0.15f;
            [Min(0f)] public float SmallShakeStrength = 0.15f;
            [Min(1)] public int SmallShakeVibrato = 8;
            [Min(0f)] public float BigShakeDuration = 0.35f;
            [Min(0f)] public float BigShakeStrength = 0.35f;
            [Min(1)] public int BigShakeVibrato = 16;
            public Color AsteroidHitFlashColor = default;
            [Min(0f)] public float AsteroidHitFlashDuration = 0.08f;
            public Color CameraFlashColor = default;
            [Min(0f)] public float CameraFlashAlpha = 0.18f;
            [Min(0f)] public float CameraFlashDuration = 0.12f;
            [Min(1)] public int PrefabEffectPoolSize = 4;
        }

        [Serializable]
        public sealed class UiSettings
        {
            public bool Enabled = true;
            public bool ShowScore = true;
            public bool ShowControls = true;
        }

        [Serializable]
        public sealed class PresentationSettings
        {
            public Color BackgroundColor = default;
            [Min(0.1f)] public float CameraZoom = 5f;
            public Color PlatformColor = default;
            public Color BackgroundPanelColor = default;
        }

        [Serializable]
        public sealed class StartFlowSettings
        {
            [Min(0f)] public float LaunchDelay = 0.15f;
            [Min(0f)] public float LaunchAccelerationDuration = 0.45f;
            [Min(0f)] public float LaunchPunchScale = 0.12f;
            [Min(0f)] public float LaunchPunchDuration = 0.2f;
        }

        [Serializable]
        public sealed class CameraSettings
        {
            [Min(0.1f)] public float OrthographicSize = 5f;
            [Min(0.1f)] public float FollowSmoothness = 4f;
            public Vector3 Offset = new(0f, 1.4f, -10f);
        }

        [Serializable]
        public sealed class BackgroundSettings
        {
            public bool ScrollingEnabled = true;
            public Color TopColor = default;
            public Color BottomColor = default;
            public Color StarTint = default;
            public Vector2 Size = new(7f, 14f);
            [Min(0f)] public float FarStarsScrollSpeed = 0.4f;
            [Min(0f)] public float MiddleStarsScrollSpeed = 0.8f;
            [Min(0f)] public float NearStarsScrollSpeed = 1.3f;
            [Min(0)] public int FarStarsCount = 35;
            [Min(0)] public int MiddleStarsCount = 25;
            [Min(0)] public int NearStarsCount = 15;
        }

        [Serializable]
        public sealed class LaunchFlowSettings
        {
            [Min(0f)] public float FogStartHeight = 1.8f;
            [Min(0f)] public float SpaceStartHeight = 6.4f;
            [Min(0.01f)] public float TransitionDuration = 0.8f;
            [Min(0.05f)] public float FogParticleInterval = 0.22f;
            public Color SkyTopColor = default;
            public Color SkyBottomColor = default;
            public Color FogTopColor = default;
            public Color FogBottomColor = default;
            public Color SpaceTopColor = default;
            public Color SpaceBottomColor = default;
            public Color CloudColor = default;
            public Color FogColor = default;
            public Color SpaceGlowColor = default;
        }

        [Serializable]
        public sealed class RecordingSettings
        {
            public bool RecordingMode;
            public bool HideUiForRecording;
            [Min(0.01f)] public float GameSpeedMultiplier = 1f;
        }

        private void EnsureSections()
        {
            Rocket ??= new RocketSettings();
            Rocket.EnsureInitialized();
            Asteroids ??= new AsteroidSettings();
            Vfx ??= new VfxSettings();
            Ui ??= new UiSettings();
            Presentation ??= new PresentationSettings();
            Recording ??= new RecordingSettings();
            StartFlow ??= new StartFlowSettings();
            Camera ??= new CameraSettings();
            Background ??= new BackgroundSettings();
            LaunchFlow ??= new LaunchFlowSettings();

            if (Rocket.Trail.StartColor == default)
            {
                Rocket.Trail.StartColor = DefaultColorPalette.RocketTrailStart;
            }

            if (Rocket.Trail.EndColor == default)
            {
                Rocket.Trail.EndColor = DefaultColorPalette.RocketTrailEnd;
            }

            if (Asteroids.Small.Color == default)
            {
                Asteroids.Small = new AsteroidSizeSettings(Asteroids.Small.SizeRange, Asteroids.Small.ScoreReward, DefaultColorPalette.AsteroidSmall);
            }

            if (Asteroids.Medium.Color == default)
            {
                Asteroids.Medium = new AsteroidSizeSettings(Asteroids.Medium.SizeRange, Asteroids.Medium.ScoreReward, DefaultColorPalette.AsteroidMedium);
            }

            if (Asteroids.Large.Color == default)
            {
                Asteroids.Large = new AsteroidSizeSettings(Asteroids.Large.SizeRange, Asteroids.Large.ScoreReward, DefaultColorPalette.AsteroidLarge);
            }

            if (Vfx.AsteroidHitFlashColor == default)
            {
                Vfx.AsteroidHitFlashColor = DefaultColorPalette.HitFlash;
            }

            if (Vfx.CameraFlashColor == default)
            {
                Vfx.CameraFlashColor = DefaultColorPalette.CameraFlash;
            }

            if (Presentation.BackgroundColor == default)
            {
                Presentation.BackgroundColor = DefaultColorPalette.PresentationBackground;
            }

            if (Presentation.PlatformColor == default)
            {
                Presentation.PlatformColor = DefaultColorPalette.PresentationPlatform;
            }

            if (Presentation.BackgroundPanelColor == default)
            {
                Presentation.BackgroundPanelColor = DefaultColorPalette.PresentationPanel;
            }

            if (Background.TopColor == default)
            {
                Background.TopColor = DefaultColorPalette.BackgroundTop;
            }

            if (Background.BottomColor == default)
            {
                Background.BottomColor = DefaultColorPalette.BackgroundBottom;
            }

            if (Background.StarTint == default)
            {
                Background.StarTint = DefaultColorPalette.StarTint;
            }

            if (LaunchFlow.SkyTopColor == default)
            {
                LaunchFlow.SkyTopColor = DefaultColorPalette.SkyTop;
            }

            if (LaunchFlow.SkyBottomColor == default)
            {
                LaunchFlow.SkyBottomColor = DefaultColorPalette.SkyBottom;
            }

            if (LaunchFlow.FogTopColor == default)
            {
                LaunchFlow.FogTopColor = DefaultColorPalette.FogTop;
            }

            if (LaunchFlow.FogBottomColor == default)
            {
                LaunchFlow.FogBottomColor = DefaultColorPalette.FogBottom;
            }

            if (LaunchFlow.SpaceTopColor == default)
            {
                LaunchFlow.SpaceTopColor = DefaultColorPalette.SpaceTop;
            }

            if (LaunchFlow.SpaceBottomColor == default)
            {
                LaunchFlow.SpaceBottomColor = DefaultColorPalette.SpaceBottom;
            }

            if (LaunchFlow.CloudColor == default)
            {
                LaunchFlow.CloudColor = DefaultColorPalette.Cloud;
            }

            if (LaunchFlow.FogColor == default)
            {
                LaunchFlow.FogColor = DefaultColorPalette.Fog;
            }

            if (LaunchFlow.SpaceGlowColor == default)
            {
                LaunchFlow.SpaceGlowColor = DefaultColorPalette.SpaceGlow;
            }
        }
    }
}
