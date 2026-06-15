using System;
using UnityEngine;

namespace SpaceFlight2D.Game
{
    public sealed class PooledParticleBurst : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;
        private Action<PooledParticleBurst> _releaseAction;
        private readonly ParticleSystem.Burst[] _singleBurst = new ParticleSystem.Burst[1];
        private float _releaseAt;
        private bool _isActive;

        public void Configure(ParticleSystem particles)
        {
            _particles = particles;
        }

        public void SetReleaseAction(Action<PooledParticleBurst> releaseAction)
        {
            _releaseAction = releaseAction;
        }

        public void Play(
            Transform parent,
            Vector3 position,
            Color color,
            short count,
            float radius,
            float duration,
            float scale,
            float startSpeedMin,
            float startSpeedMax,
            float startSizeMin,
            float startSizeMax)
        {
            transform.SetParent(parent, false);
            transform.position = position;
            transform.localScale = Vector3.one * scale;
            gameObject.SetActive(true);

            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _particles.main;
            main.duration = duration;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(duration * 0.3f, duration * 0.65f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeedMin, startSpeedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(startSizeMin, startSizeMax);
            main.startColor = color;
            main.maxParticles = count + 4;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = _particles.emission;
            emission.rateOverTime = 0f;
            _singleBurst[0] = new ParticleSystem.Burst(0f, count);
            emission.SetBursts(_singleBurst);

            var shape = _particles.shape;
            shape.shapeType = radius < 0.09f ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Circle;
            shape.radius = radius;
            if (shape.shapeType == ParticleSystemShapeType.Cone)
            {
                shape.angle = 24f;
                shape.rotation = new Vector3(90f, 0f, 0f);
            }

            _releaseAt = Time.time + duration + 0.7f;
            _isActive = true;
            _particles.Play();
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
            _particles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
