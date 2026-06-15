using System;
using System.Linq;
using UnityEngine;

namespace SpaceFlight2D.Game
{
    public sealed class PooledTimedEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] _particleSystems = Array.Empty<ParticleSystem>();
        private Action<PooledTimedEffect> _releaseAction;
        private float _releaseAt;
        private bool _isActive;

        public void Configure(ParticleSystem[] particleSystems)
        {
            _particleSystems = particleSystems == null
                ? Array.Empty<ParticleSystem>()
                : particleSystems.Where(system => system != null).ToArray();

            for (var i = 0; i < _particleSystems.Length; i++)
            {
                var particleSystem = _particleSystems[i];
                var main = particleSystem.main;
                main.playOnAwake = false;
                main.loop = false;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && renderer.sortingOrder < 20)
                {
                    renderer.sortingOrder = 20;
                }
            }
        }

        public void SetReleaseAction(Action<PooledTimedEffect> releaseAction)
        {
            _releaseAction = releaseAction;
        }

        public void Play(Transform parent, Vector3 position, float scale, float lifetime)
        {
            transform.SetParent(parent, false);
            position.z = 0f;
            transform.position = position;
            transform.localScale = Vector3.one * scale;
            gameObject.SetActive(true);

            for (var i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _particleSystems[i].Play(true);
            }

            _releaseAt = Time.time + lifetime;
            _isActive = true;
        }

        private void Update()
        {
            if (_isActive && Time.time >= _releaseAt)
            {
                Release();
            }
        }

        private void OnDisable()
        {
            _isActive = false;

            for (var i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void Release()
        {
            if (!_isActive)
            {
                return;
            }

            _isActive = false;
            _releaseAction?.Invoke(this);
        }
    }
}
