using DG.Tweening;
using SpaceFlight2D.Game.Config;
using UnityEngine;
using Zenject;

namespace SpaceFlight2D.Game
{
    public interface ICameraShakeService
    {
        void ShakeSmall();
        void ShakeBig();
    }

    public sealed class CameraShakeService : MonoBehaviour, ICameraShakeService, IInitializable
    {
        [SerializeField] private Camera _targetCamera;

        private PrototypeGameConfig _config;
        private Bootstrap.CreativeSceneCustomizer _customizer;
        private Vector3 _origin;

        [Inject]
        public void Construct(PrototypeGameConfig config, Bootstrap.CreativeSceneCustomizer customizer)
        {
            _config = config;
            _customizer = customizer;
        }

        public void Bind(Camera targetCamera)
        {
            _targetCamera = targetCamera;
            if (_targetCamera != null)
            {
                _origin = _targetCamera.transform.localPosition;
            }
        }

        public void Initialize()
        {
            if (_targetCamera != null)
            {
                _origin = _targetCamera.transform.localPosition;
            }
        }

        private void Awake()
        {
            if (_targetCamera != null)
            {
                _origin = _targetCamera.transform.localPosition;
            }
        }

        public void ShakeSmall()
        {
            if (_customizer == null || !_customizer.EnableScreenShake || _targetCamera == null || _config == null)
            {
                return;
            }

            Shake(_config.Vfx.SmallShakeDuration, _config.Vfx.SmallShakeStrength, _config.Vfx.SmallShakeVibrato);
        }

        public void ShakeBig()
        {
            if (_customizer == null || !_customizer.EnableScreenShake || _targetCamera == null || _config == null)
            {
                return;
            }

            Shake(_config.Vfx.BigShakeDuration, _config.Vfx.BigShakeStrength, _config.Vfx.BigShakeVibrato);
        }

        private void Shake(float duration, float strength, int vibrato)
        {
            _origin = _targetCamera.transform.localPosition;
            _targetCamera.transform.DOKill();
            _targetCamera.transform.localPosition = _origin;
            _targetCamera.transform.DOShakePosition(duration, strength, vibrato, 90f, false, true)
                .SetUpdate(true)
                .OnComplete(() => _targetCamera.transform.localPosition = _origin);
        }
    }
}
