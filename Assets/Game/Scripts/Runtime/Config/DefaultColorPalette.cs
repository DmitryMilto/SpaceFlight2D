using UnityEngine;

namespace SpaceFlight2D.Game.Config
{
    public static class DefaultColorPalette
    {
        public static Color White => Color.white;
        public static Color RocketBody => new(0.94f, 0.96f, 0.98f, 1f);
        public static Color RocketTrailStart => Color.yellow;
        public static Color RocketTrailEnd => Color.red;
        public static Color AsteroidSmall => new(0.76f, 0.77f, 0.81f, 1f);
        public static Color AsteroidMedium => new(0.66f, 0.68f, 0.74f, 1f);
        public static Color AsteroidLarge => new(0.54f, 0.58f, 0.65f, 1f);
        public static Color HitFlash => Color.white;
        public static Color CameraFlash => Color.white;
        public static Color PresentationBackground => new(0.06f, 0.09f, 0.16f, 1f);
        public static Color PresentationPanel => new(0.11f, 0.15f, 0.24f, 1f);
        public static Color PresentationPlatform => new(0.68f, 0.72f, 0.78f, 1f);
        public static Color CleanPresetPanel => new(0.1f, 0.14f, 0.24f, 1f);
        public static Color CleanPresetPlatform => new(0.28f, 0.62f, 0.24f, 1f);
        public static Color BackgroundTop => new(0.02f, 0.03f, 0.08f, 1f);
        public static Color BackgroundBottom => new(0.08f, 0.02f, 0.12f, 1f);
        public static Color StarTint => Color.white;
        public static Color SkyTop => new(0.42f, 0.72f, 0.98f, 1f);
        public static Color SkyBottom => new(0.82f, 0.92f, 1f, 1f);
        public static Color FogTop => new(0.76f, 0.84f, 0.9f, 1f);
        public static Color FogBottom => new(0.9f, 0.93f, 0.96f, 1f);
        public static Color SpaceTop => new(0.02f, 0.03f, 0.08f, 1f);
        public static Color SpaceBottom => new(0.08f, 0.02f, 0.12f, 1f);
        public static Color Cloud => new(1f, 1f, 1f, 1f);
        public static Color Fog => new(0.92f, 0.94f, 0.96f, 1f);
        public static Color SpaceGlow => new(0.16f, 0.22f, 0.34f, 0.8f);
        public static Color ActionBackground => new(0.12f, 0.05f, 0.08f, 1f);
        public static Color ActionPanel => new(0.27f, 0.08f, 0.13f, 1f);
        public static Color ActionTop => new(0.05f, 0.02f, 0.07f, 1f);
        public static Color ActionBottom => new(0.12f, 0.04f, 0.09f, 1f);
        public static Color ActionRocket => new(0.98f, 0.92f, 0.88f, 1f);

        public static CosmosPalette PurpleCosmos => new(
            new Color(0.08f, 0.02f, 0.12f, 1f),
            new Color(0.02f, 0.03f, 0.08f, 1f),
            new Color(0.06f, 0.05f, 0.14f, 1f),
            new Color(0.12f, 0.08f, 0.2f, 1f),
            new Color(0.75f, 0.72f, 0.9f, 1f));

        public static CosmosPalette BlueCosmos => new(
            new Color(0.02f, 0.08f, 0.18f, 1f),
            new Color(0.02f, 0.05f, 0.12f, 1f),
            new Color(0.05f, 0.1f, 0.22f, 1f),
            new Color(0.1f, 0.16f, 0.3f, 1f),
            new Color(0.74f, 0.9f, 1f, 1f));

        public static CosmosPalette RedCosmos => new(
            new Color(0.22f, 0.03f, 0.08f, 1f),
            new Color(0.08f, 0.01f, 0.04f, 1f),
            new Color(0.24f, 0.08f, 0.12f, 1f),
            new Color(0.34f, 0.12f, 0.14f, 1f),
            new Color(1f, 0.8f, 0.72f, 1f));

        public static CosmosPalette GreenCosmos => new(
            new Color(0.04f, 0.12f, 0.08f, 1f),
            new Color(0.02f, 0.06f, 0.05f, 1f),
            new Color(0.08f, 0.18f, 0.12f, 1f),
            new Color(0.14f, 0.28f, 0.16f, 1f),
            new Color(0.78f, 1f, 0.82f, 1f));

        public static CosmosPalette RainbowCosmos => new(
            new Color(0.18f, 0.08f, 0.2f, 1f),
            new Color(0.04f, 0.06f, 0.14f, 1f),
            new Color(0.2f, 0.14f, 0.28f, 1f),
            new Color(0.28f, 0.22f, 0.36f, 1f),
            new Color(0.95f, 0.95f, 1f, 1f));

        public readonly struct CosmosPalette
        {
            public readonly Color Top;
            public readonly Color Bottom;
            public readonly Color Background;
            public readonly Color Panel;
            public readonly Color Platform;

            public CosmosPalette(Color top, Color bottom, Color background, Color panel, Color platform)
            {
                Top = top;
                Bottom = bottom;
                Background = background;
                Panel = panel;
                Platform = platform;
            }
        }
    }
}
