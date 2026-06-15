using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceFlight2D.Game.Config
{
    [CreateAssetMenu(menuName = "Rocket Game/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Rocket")]
        public float rocketSpeed = 4.5f;
        public float horizontalSpeed = 2.2f;
        public float rotationSpeed = 120f;
        public float maxRotationAngle = 45f;
        public float autoStabilizeSpeed = 80f;
        public Vector2 rocketSize = new(0.8f, 1.8f);
        public Color rocketColor = default;
        public bool trailEnabled = true;
        public bool rocketTrailEnabled = true;

        [Header("Asteroids")]
        public GameObject asteroidPrefab;
        public float spawnIntervalMin = 0.7f;
        public float spawnIntervalMax = 1.4f;
        public Vector2 spawnRangeX = new(-2.3f, 2.3f);
        public float spawnOffsetY = 6f;
        public AsteroidSizeSettings smallAsteroid = new(new Vector2(0.45f, 0.65f), 10, default);
        public AsteroidSizeSettings mediumAsteroid = new(new Vector2(0.75f, 1f), 20, default);
        public AsteroidSizeSettings largeAsteroid = new(new Vector2(1.1f, 1.4f), 35, default);

        [Header("Asteroid Motion")]
        public Vector2 asteroidSpeedRange = new(2.6f, 4.6f);
        public Vector2 asteroidRotationSpeedRange = new(-110f, 110f);
        public float asteroidLifetime = 8f;

        [Header("VFX")]
        public bool vfxEnabled = true;
        public bool screenShakeEnabled = true;
        public bool asteroidHitFlashEnabled = true;
        public bool cameraColorFlashEnabled = false;
        [FormerlySerializedAs("hitVfxPrefab")]
        public GameObject asteroidHitVfxPrefab;
        public float asteroidHitVfxLifetime = 1.2f;
        public float asteroidHitVfxScale = 1f;
        public GameObject rocketExplosionVfxPrefab;
        public float rocketExplosionVfxLifetime = 2f;
        public float rocketExplosionVfxScale = 1.4f;
        public GameObject launchBurstVfxPrefab;
        public float launchBurstVfxLifetime = 0.8f;
        public float launchBurstVfxScale = 1f;
        public float smallShakeDuration = 0.15f;
        public float smallShakeStrength = 0.15f;
        public int smallShakeVibrato = 8;
        public float bigShakeDuration = 0.35f;
        public float bigShakeStrength = 0.35f;
        public int bigShakeVibrato = 16;
        public float trailTime = 0.35f;
        public float trailStartWidth = 0.28f;
        public float trailEndWidth = 0.05f;
        public Color trailStartColor = default;
        public Color trailEndColor = default;
        public Color asteroidHitFlashColor = default;
        public float asteroidHitFlashDuration = 0.08f;
        public Color cameraFlashColor = default;
        public float cameraFlashAlpha = 0.18f;
        public float cameraFlashDuration = 0.12f;
        [Header("UI")]
        public bool uiEnabled = true;
        public bool showScore = true;
        public bool showControls = true;

        [Header("Creative Recording")]
        public bool recordingMode;
        public bool hideUiForRecording;
        public float gameSpeedMultiplier = 1f;
        public Color backgroundColor = default;
        public float cameraZoom = 5f;
        public Color platformColor = default;
        public Color backgroundPanelColor = default;

        [Header("Start Flow")]
        public float launchDelay = 0.15f;
        public float launchAccelerationDuration = 0.45f;
        public float launchPunchScale = 0.12f;
        public float launchPunchDuration = 0.2f;

        [Header("Camera")]
        public float cameraOrthographicSize = 5f;
        public float cameraFollowSmoothness = 4f;
        public Vector3 cameraOffset = new(0f, 1.4f, -10f);

        [Header("Background")]
        public bool backgroundScrollingEnabled = true;
        public Color backgroundTopColor = default;
        public Color backgroundBottomColor = default;
        public Vector2 backgroundSize = new(7f, 14f);
        public float farStarsScrollSpeed = 0.4f;
        public float middleStarsScrollSpeed = 0.8f;
        public float nearStarsScrollSpeed = 1.3f;
        public int farStarsCount = 35;
        public int middleStarsCount = 25;
        public int nearStarsCount = 15;

        [Serializable]
        public struct AsteroidSizeSettings
        {
            public Vector2 sizeRange;
            public int scoreReward;
            public Color color;

            public AsteroidSizeSettings(Vector2 sizeRange, int scoreReward, Color color)
            {
                this.sizeRange = sizeRange;
                this.scoreReward = scoreReward;
                this.color = color;
            }
        }

        private void OnEnable()
        {
            ApplyDefaultColors();
        }

        private void OnValidate()
        {
            ApplyDefaultColors();
        }

        private void ApplyDefaultColors()
        {
            if (rocketColor == default)
            {
                rocketColor = DefaultColorPalette.RocketBody;
            }

            if (smallAsteroid.color == default)
            {
                smallAsteroid = new AsteroidSizeSettings(smallAsteroid.sizeRange, smallAsteroid.scoreReward, DefaultColorPalette.AsteroidSmall);
            }

            if (mediumAsteroid.color == default)
            {
                mediumAsteroid = new AsteroidSizeSettings(mediumAsteroid.sizeRange, mediumAsteroid.scoreReward, DefaultColorPalette.AsteroidMedium);
            }

            if (largeAsteroid.color == default)
            {
                largeAsteroid = new AsteroidSizeSettings(largeAsteroid.sizeRange, largeAsteroid.scoreReward, DefaultColorPalette.AsteroidLarge);
            }

            if (trailStartColor == default)
            {
                trailStartColor = DefaultColorPalette.RocketTrailStart;
            }

            if (trailEndColor == default)
            {
                trailEndColor = DefaultColorPalette.RocketTrailEnd;
            }

            if (asteroidHitFlashColor == default)
            {
                asteroidHitFlashColor = DefaultColorPalette.HitFlash;
            }

            if (cameraFlashColor == default)
            {
                cameraFlashColor = DefaultColorPalette.CameraFlash;
            }

            if (backgroundColor == default)
            {
                backgroundColor = DefaultColorPalette.PresentationBackground;
            }

            if (platformColor == default)
            {
                platformColor = DefaultColorPalette.PresentationPlatform;
            }

            if (backgroundPanelColor == default)
            {
                backgroundPanelColor = DefaultColorPalette.PresentationPanel;
            }

            if (backgroundTopColor == default)
            {
                backgroundTopColor = DefaultColorPalette.BackgroundTop;
            }

            if (backgroundBottomColor == default)
            {
                backgroundBottomColor = DefaultColorPalette.BackgroundBottom;
            }
        }
    }
}
