using SpaceFlight2D.Game.Config;
using UnityEngine;

namespace SpaceFlight2D.Game.Bootstrap
{
    [ExecuteAlways]
    public sealed class CreativeSceneCustomizer : MonoBehaviour
    {
        [SerializeField] private PrototypeGameConfig _config;
        [SerializeField] private PrototypeSceneReferences _sceneReferences;
        [SerializeField] private bool _showUiOverride = true;
        [SerializeField] private bool _enableVfxOverride = true;
        [SerializeField] private bool _enableScreenShakeOverride = true;
        [SerializeField] private bool _autoApplyInEditMode = true;

        public PrototypeGameConfig Config => _config;
        public bool ShowUi => _config != null && _showUiOverride && _config.Ui.Enabled && !_config.Recording.HideUiForRecording;
        public bool EnableVfx => _config != null && _enableVfxOverride && _config.Vfx.Enabled;
        public bool EnableScreenShake => _config != null && _enableScreenShakeOverride && _config.Vfx.ScreenShakeEnabled;

        public void Bind(
            PrototypeGameConfig config,
            PrototypeSceneReferences sceneReferences,
            bool showUiOverride,
            bool enableVfxOverride,
            bool enableScreenShakeOverride)
        {
            _config = config;
            _sceneReferences = sceneReferences;
            _showUiOverride = showUiOverride;
            _enableVfxOverride = enableVfxOverride;
            _enableScreenShakeOverride = enableScreenShakeOverride;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && _autoApplyInEditMode)
            {
                Apply();
            }
        }

        [ContextMenu("Apply Creative Settings")]
        public void Apply()
        {
            if (_config == null || _sceneReferences == null)
            {
                return;
            }

            if (_sceneReferences.MainCamera != null)
            {
                _sceneReferences.MainCamera.backgroundColor = _config.Presentation.BackgroundColor;
                _sceneReferences.MainCamera.orthographicSize = _config.Camera.OrthographicSize;
            }

            if (_sceneReferences.BackgroundController != null)
            {
                _sceneReferences.BackgroundController.ApplyConfig(_config);
            }

            if (_sceneReferences.BackgroundRenderer != null)
            {
                _sceneReferences.BackgroundRenderer.color = _config.Presentation.BackgroundPanelColor;
            }

            if (_sceneReferences.PlatformRenderer != null)
            {
                _sceneReferences.PlatformRenderer.color = _config.Presentation.PlatformColor;
            }

            _sceneReferences.Rocket?.ApplyVisuals(_config);
            _sceneReferences.UiController?.ApplyVisuals(_config);

            if (_sceneReferences.UiRoot != null)
            {
                _sceneReferences.UiRoot.gameObject.SetActive(ShowUi);
            }

            Time.timeScale = Mathf.Max(0.01f, _config.Recording.GameSpeedMultiplier);
        }

        public void ApplyPreset(PrototypeGameConfig config, bool showUi, bool enableVfx, bool enableScreenShake)
        {
            _config = config;
            _showUiOverride = showUi;
            _enableVfxOverride = enableVfx;
            _enableScreenShakeOverride = enableScreenShake;
            Apply();
        }

        public void ToggleUi()
        {
            _showUiOverride = !_showUiOverride;
            Apply();
        }

        public void ToggleVfx()
        {
            _enableVfxOverride = !_enableVfxOverride;
            Apply();
        }

        public void ToggleScreenShake()
        {
            _enableScreenShakeOverride = !_enableScreenShakeOverride;
            Apply();
        }
    }
}
