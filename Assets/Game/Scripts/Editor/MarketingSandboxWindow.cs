using SpaceFlight2D.Game.Bootstrap;
using SpaceFlight2D.Game.Config;
using UnityEditor;
using UnityEngine;

namespace SpaceFlight2D.Editor
{
    public sealed class MarketingSandboxWindow : EditorWindow
    {
        private PrototypeGameConfig _config;
        private CreativeSceneCustomizer _customizer;
        private float _spawnIntervalMin;
        private float _spawnIntervalMax;
        private float _cameraZoom;
        private Color _backgroundColor;
        private Color _rocketColor;
        private Color _smallAsteroidColor;
        private Color _mediumAsteroidColor;
        private Color _largeAsteroidColor;

        [MenuItem("Tools/Rocket Creative Sandbox")]
        public static void ShowWindow()
        {
            GetWindow<MarketingSandboxWindow>("Rocket Creative Sandbox");
        }

        private void OnFocus()
        {
            RefreshSceneBindings();
            LoadEditableValues();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Rocket Creative Sandbox", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use presets and overrides to prepare gameplay or marketing capture variants.",
                MessageType.Info);

            _config = (PrototypeGameConfig)EditorGUILayout.ObjectField(
                "Game Config",
                _config,
                typeof(PrototypeGameConfig),
                false);

            _customizer = (CreativeSceneCustomizer)EditorGUILayout.ObjectField(
                "Scene Customizer",
                _customizer,
                typeof(CreativeSceneCustomizer),
                true);

            if (GUILayout.Button("Load Values From Config"))
            {
                LoadEditableValues();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply Clean Gameplay preset"))
                {
                    ApplyPreset(PresetType.CleanGameplay);
                }

                if (GUILayout.Button("Apply Action Ad preset"))
                {
                    ApplyPreset(PresetType.ActionAd);
                }
            }

            if (GUILayout.Button("Apply No UI Recording preset"))
            {
                ApplyPreset(PresetType.NoUiRecording);
            }

            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Toggle UI"))
                {
                    _customizer?.ToggleUi();
                }

                if (GUILayout.Button("Toggle VFX"))
                {
                    _customizer?.ToggleVfx();
                }

                if (GUILayout.Button("Toggle Screen Shake"))
                {
                    _customizer?.ToggleScreenShake();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Selected Config Values", EditorStyles.boldLabel);

            _spawnIntervalMin = EditorGUILayout.FloatField("Spawn Interval Min", _spawnIntervalMin);
            _spawnIntervalMax = EditorGUILayout.FloatField("Spawn Interval Max", _spawnIntervalMax);
            _cameraZoom = EditorGUILayout.FloatField("Camera Zoom", _cameraZoom);
            _backgroundColor = EditorGUILayout.ColorField("Background Color", _backgroundColor);
            _rocketColor = EditorGUILayout.ColorField("Rocket Color", _rocketColor);
            _smallAsteroidColor = EditorGUILayout.ColorField("Small Asteroid", _smallAsteroidColor);
            _mediumAsteroidColor = EditorGUILayout.ColorField("Medium Asteroid", _mediumAsteroidColor);
            _largeAsteroidColor = EditorGUILayout.ColorField("Large Asteroid", _largeAsteroidColor);

            if (GUILayout.Button("Apply selected config values"))
            {
                ApplySelectedConfigValues();
            }
        }

        private void RefreshSceneBindings()
        {
            _customizer = UnityEngine.Object.FindFirstObjectByType<CreativeSceneCustomizer>();
            if (_customizer != null)
            {
                _config = _customizer.Config;
            }
        }

        private void LoadEditableValues()
        {
            if (_config == null)
            {
                return;
            }

            _spawnIntervalMin = _config.Asteroids.SpawnIntervalMin;
            _spawnIntervalMax = _config.Asteroids.SpawnIntervalMax;
            _cameraZoom = _config.Presentation.CameraZoom;
            _backgroundColor = _config.Presentation.BackgroundColor;
            _rocketColor = _config.Rocket.Color;
            _smallAsteroidColor = _config.Asteroids.Small.Color;
            _mediumAsteroidColor = _config.Asteroids.Medium.Color;
            _largeAsteroidColor = _config.Asteroids.Large.Color;
        }

        private void ApplySelectedConfigValues()
        {
            if (_config == null)
            {
                return;
            }

            Undo.RecordObject(_config, "Apply creative config values");
            _config.Asteroids.SpawnIntervalMin = _spawnIntervalMin;
            _config.Asteroids.SpawnIntervalMax = _spawnIntervalMax;
            _config.Presentation.CameraZoom = _cameraZoom;
            _config.Presentation.BackgroundColor = _backgroundColor;
            _config.Rocket.Color = _rocketColor;
            var small = _config.Asteroids.Small;
            small.Color = _smallAsteroidColor;
            _config.Asteroids.Small = small;
            var medium = _config.Asteroids.Medium;
            medium.Color = _mediumAsteroidColor;
            _config.Asteroids.Medium = medium;
            var large = _config.Asteroids.Large;
            large.Color = _largeAsteroidColor;
            _config.Asteroids.Large = large;
            EditorUtility.SetDirty(_config);
            _customizer?.Apply();
        }

        private void ApplyPreset(PresetType presetType)
        {
            if (_config == null)
            {
                return;
            }

            Undo.RecordObject(_config, "Apply creative preset");

            switch (presetType)
            {
                case PresetType.CleanGameplay:
                    _config.Ui.Enabled = true;
                    _config.Recording.HideUiForRecording = false;
                    _config.Vfx.Enabled = true;
                    _config.Vfx.ScreenShakeEnabled = true;
                    _config.Vfx.CameraColorFlashEnabled = false;
                    _config.Rocket.Trail.RocketEnabled = true;
                    _config.Vfx.SmallShakeStrength = 0.08f;
                    _config.Vfx.BigShakeStrength = 0.2f;
                    _config.Asteroids.SpawnIntervalMin = 1.1f;
                    _config.Asteroids.SpawnIntervalMax = 1.4f;
                    _config.Presentation.CameraZoom = 5f;
                    _config.Recording.GameSpeedMultiplier = 1f;
                    break;
                case PresetType.ActionAd:
                    _config.Ui.Enabled = true;
                    _config.Recording.HideUiForRecording = false;
                    _config.Vfx.Enabled = true;
                    _config.Vfx.ScreenShakeEnabled = true;
                    _config.Vfx.CameraColorFlashEnabled = true;
                    _config.Rocket.Trail.RocketEnabled = true;
                    _config.Vfx.SmallShakeStrength = 0.18f;
                    _config.Vfx.BigShakeStrength = 0.45f;
                    _config.Asteroids.SpawnIntervalMin = 0.45f;
                    _config.Asteroids.SpawnIntervalMax = 0.6f;
                    _config.Presentation.CameraZoom = 4.5f;
                    _config.Recording.GameSpeedMultiplier = 1.1f;
                    break;
                case PresetType.NoUiRecording:
                    _config.Ui.Enabled = false;
                    _config.Recording.HideUiForRecording = true;
                    _config.Vfx.Enabled = false;
                    _config.Vfx.ScreenShakeEnabled = false;
                    _config.Vfx.CameraColorFlashEnabled = false;
                    _config.Rocket.Trail.RocketEnabled = false;
                    _config.Asteroids.SpawnIntervalMin = 0.65f;
                    _config.Asteroids.SpawnIntervalMax = 0.8f;
                    _config.Presentation.CameraZoom = 4.8f;
                    _config.Recording.GameSpeedMultiplier = 1f;
                    break;
            }

            EditorUtility.SetDirty(_config);
            LoadEditableValues();
            _customizer?.ApplyPreset(_config, _config.Ui.Enabled, _config.Vfx.Enabled, _config.Vfx.ScreenShakeEnabled);
        }

        private enum PresetType
        {
            CleanGameplay,
            ActionAd,
            NoUiRecording
        }
    }
}
