using DG.Tweening;
using SpaceFlight2D.Game.Config;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceFlight2D.Game
{
    public interface IVfxService
    {
        void SpawnHitVfx(Vector3 position, Color color);
        void SpawnAsteroidHitVfx(Vector3 position, Color color);
        void SpawnRocketExplosion(Vector3 position);
        void SpawnLaunchBurst(Vector3 position);
        void SpawnFogTransitionVfx(Vector3 position);
        void SpawnFogZoneVfx(Vector3 position);
        void SpawnSpaceEntryVfx(Vector3 position);
        void PlayCameraFlash();
    }

    public sealed class VfxService : MonoBehaviour, IVfxService, IInitializable
    {
        [SerializeField] private Transform _vfxRoot;
        [SerializeField] private Image _cameraFlashOverlay;
        [SerializeField] private PooledTimedEffect[] _asteroidHitEffects = System.Array.Empty<PooledTimedEffect>();
        [SerializeField] private PooledTimedEffect[] _rocketExplosionEffects = System.Array.Empty<PooledTimedEffect>();
        [SerializeField] private PooledTimedEffect[] _launchBurstEffects = System.Array.Empty<PooledTimedEffect>();
        [SerializeField] private PooledTimedEffect[] _fogTransitionEffects = System.Array.Empty<PooledTimedEffect>();
        [SerializeField] private PooledTimedEffect[] _fogZoneEffects = System.Array.Empty<PooledTimedEffect>();
        [SerializeField] private PooledTimedEffect[] _spaceEntryEffects = System.Array.Empty<PooledTimedEffect>();

        private PrototypeGameConfig _config;
        private SpaceFlight2D.Game.Bootstrap.CreativeSceneCustomizer _customizer;
        private readonly Queue<PooledTimedEffect> _asteroidHitQueue = new();
        private readonly Queue<PooledTimedEffect> _rocketExplosionQueue = new();
        private readonly Queue<PooledTimedEffect> _launchBurstQueue = new();
        private readonly Queue<PooledTimedEffect> _fogTransitionQueue = new();
        private readonly Queue<PooledTimedEffect> _fogZoneQueue = new();
        private readonly Queue<PooledTimedEffect> _spaceEntryQueue = new();
        private bool _isInitialized;

        [Inject]
        public void Construct(PrototypeGameConfig config, SpaceFlight2D.Game.Bootstrap.CreativeSceneCustomizer customizer)
        {
            _config = config;
            _customizer = customizer;
        }

        public void Initialize()
        {
            EnsureInitialized();
        }

        public void Bind(
            Transform vfxRoot,
            Image cameraFlashOverlay,
            PooledTimedEffect[] asteroidHitEffects,
            PooledTimedEffect[] rocketExplosionEffects,
            PooledTimedEffect[] launchBurstEffects,
            PooledTimedEffect[] fogTransitionEffects,
            PooledTimedEffect[] fogZoneEffects,
            PooledTimedEffect[] spaceEntryEffects)
        {
            _vfxRoot = vfxRoot;
            _cameraFlashOverlay = cameraFlashOverlay;
            ConfigurePools(
                asteroidHitEffects,
                rocketExplosionEffects,
                launchBurstEffects,
                fogTransitionEffects,
                fogZoneEffects,
                spaceEntryEffects);
            EnsureInitialized();
        }

        public void Bind(Transform vfxRoot, Image cameraFlashOverlay)
        {
            _vfxRoot = vfxRoot;
            _cameraFlashOverlay = cameraFlashOverlay;
            EnsureInitialized();
        }

        public void ConfigurePools(
            PooledTimedEffect[] asteroidHitEffects,
            PooledTimedEffect[] rocketExplosionEffects,
            PooledTimedEffect[] launchBurstEffects,
            PooledTimedEffect[] fogTransitionEffects,
            PooledTimedEffect[] fogZoneEffects,
            PooledTimedEffect[] spaceEntryEffects)
        {
            _asteroidHitEffects = asteroidHitEffects ?? System.Array.Empty<PooledTimedEffect>();
            _rocketExplosionEffects = rocketExplosionEffects ?? System.Array.Empty<PooledTimedEffect>();
            _launchBurstEffects = launchBurstEffects ?? System.Array.Empty<PooledTimedEffect>();
            _fogTransitionEffects = fogTransitionEffects ?? System.Array.Empty<PooledTimedEffect>();
            _fogZoneEffects = fogZoneEffects ?? System.Array.Empty<PooledTimedEffect>();
            _spaceEntryEffects = spaceEntryEffects ?? System.Array.Empty<PooledTimedEffect>();
            RebuildPools();
        }

        public void SpawnHitVfx(Vector3 position, Color color)
        {
            SpawnAsteroidHitVfx(position, color);
        }

        public void SpawnAsteroidHitVfx(Vector3 position, Color color)
        {
            if (_customizer == null || !_customizer.EnableVfx || _config == null)
            {
                return;
            }

            SpawnTimedEffect(_asteroidHitQueue, position, _config.Vfx.AsteroidHitScale, _config.Vfx.AsteroidHitLifetime);
        }

        public void SpawnRocketExplosion(Vector3 position)
        {
            if (_customizer == null || !_customizer.EnableVfx || _config == null)
            {
                return;
            }

            SpawnTimedEffect(_rocketExplosionQueue, position, _config.Vfx.RocketExplosionScale, _config.Vfx.RocketExplosionLifetime);

            if (_config.Vfx.CameraColorFlashEnabled)
            {
                PlayCameraFlash();
            }
        }

        public void SpawnLaunchBurst(Vector3 position)
        {
            if (_customizer == null || !_customizer.EnableVfx || _config == null)
            {
                return;
            }

            SpawnTimedEffect(_launchBurstQueue, position, _config.Vfx.LaunchBurstScale, _config.Vfx.LaunchBurstLifetime);
        }

        public void SpawnFogTransitionVfx(Vector3 position)
        {
            if (_customizer == null || !_customizer.EnableVfx || _config == null)
            {
                return;
            }

            SpawnTimedEffect(_fogTransitionQueue, position, 1f, 1f);
        }

        public void SpawnFogZoneVfx(Vector3 position)
        {
            if (_customizer == null || !_customizer.EnableVfx || _config == null)
            {
                return;
            }

            SpawnTimedEffect(_fogZoneQueue, position, 1f, 1f);
        }

        public void SpawnSpaceEntryVfx(Vector3 position)
        {
            if (_customizer == null || !_customizer.EnableVfx || _config == null)
            {
                return;
            }

            SpawnTimedEffect(_spaceEntryQueue, position, 1f, 1f);
        }

        public void PlayCameraFlash()
        {
            if (_customizer == null || !_customizer.EnableVfx || _config == null || !_config.Vfx.CameraColorFlashEnabled || _cameraFlashOverlay == null)
            {
                return;
            }

            _cameraFlashOverlay.DOKill();
            _cameraFlashOverlay.gameObject.SetActive(true);
            _cameraFlashOverlay.color = new Color(
                _config.Vfx.CameraFlashColor.r,
                _config.Vfx.CameraFlashColor.g,
                _config.Vfx.CameraFlashColor.b,
                0f);

            var flashColor = new Color(
                _config.Vfx.CameraFlashColor.r,
                _config.Vfx.CameraFlashColor.g,
                _config.Vfx.CameraFlashColor.b,
                _config.Vfx.CameraFlashAlpha);

            _cameraFlashOverlay.DOColor(flashColor, _config.Vfx.CameraFlashDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _cameraFlashOverlay.DOColor(new Color(
                            _config.Vfx.CameraFlashColor.r,
                            _config.Vfx.CameraFlashColor.g,
                            _config.Vfx.CameraFlashColor.b,
                            0f),
                        _config.Vfx.CameraFlashDuration * 0.5f)
                        .SetEase(Ease.InQuad)
                        .SetUpdate(true)
                        .OnComplete(() => _cameraFlashOverlay.gameObject.SetActive(false));
                });
        }

        private void SpawnTimedEffect(Queue<PooledTimedEffect> pool, Vector3 position, float scale, float lifetime)
        {
            EnsureInitialized();
            if (pool.Count == 0)
            {
                return;
            }

            var effect = pool.Dequeue();
            effect.Play(_vfxRoot, position, scale, lifetime);
        }

        private void EnsureInitialized()
        {
            if (_isInitialized || _config == null || _vfxRoot == null)
            {
                return;
            }

            if (_asteroidHitQueue.Count == 0
                && (_asteroidHitEffects.Length > 0
                    || _rocketExplosionEffects.Length > 0
                    || _launchBurstEffects.Length > 0
                    || _fogTransitionEffects.Length > 0
                    || _fogZoneEffects.Length > 0
                    || _spaceEntryEffects.Length > 0))
            {
                RebuildPools();
            }

            _isInitialized = true;
        }

        private void RebuildPools()
        {
            _asteroidHitQueue.Clear();
            _rocketExplosionQueue.Clear();
            _launchBurstQueue.Clear();
            _fogTransitionQueue.Clear();
            _fogZoneQueue.Clear();
            _spaceEntryQueue.Clear();

            RebuildEffectPool(_asteroidHitEffects, _asteroidHitQueue);
            RebuildEffectPool(_rocketExplosionEffects, _rocketExplosionQueue);
            RebuildEffectPool(_launchBurstEffects, _launchBurstQueue);
            RebuildEffectPool(_fogTransitionEffects, _fogTransitionQueue);
            RebuildEffectPool(_fogZoneEffects, _fogZoneQueue);
            RebuildEffectPool(_spaceEntryEffects, _spaceEntryQueue);
        }

        private static void RebuildEffectPool(PooledTimedEffect[] effects, Queue<PooledTimedEffect> queue)
        {
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                effect.SetReleaseAction(releasedEffect =>
                {
                    releasedEffect.gameObject.SetActive(false);
                    queue.Enqueue(releasedEffect);
                });
                effect.gameObject.SetActive(false);
                queue.Enqueue(effect);
            }
        }
    }
}
