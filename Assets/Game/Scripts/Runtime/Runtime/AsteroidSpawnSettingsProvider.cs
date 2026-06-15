using SpaceFlight2D.Game.Config;
using UnityEngine;

namespace SpaceFlight2D.Game.Runtime
{
    public interface IAsteroidSpawnSettingsProvider
    {
        AsteroidSpawnSettings GetNextSpawnSettings();
    }

    public sealed class AsteroidSpawnSettingsProvider : IAsteroidSpawnSettingsProvider
    {
        private readonly PrototypeGameConfig _config;

        public AsteroidSpawnSettingsProvider(PrototypeGameConfig config)
        {
            _config = config;
        }

        public AsteroidSpawnSettings GetNextSpawnSettings()
        {
            var type = (AsteroidType)Random.Range(0, 3);
            var sizeSettings = GetSizeSettings(type);
            var size = Random.Range(sizeSettings.SizeRange.x, sizeSettings.SizeRange.y);

            return new AsteroidSpawnSettings
            {
                Type = type,
                VisualSprite = _config.Asteroids.VisualSprite,
                Size = new Vector2(size, size),
                Speed = Random.Range(_config.Asteroids.SpeedRange.x, _config.Asteroids.SpeedRange.y),
                ScoreReward = sizeSettings.ScoreReward,
                Color = sizeSettings.Color,
                RotationSpeed = Random.Range(_config.Asteroids.RotationSpeedRange.x, _config.Asteroids.RotationSpeedRange.y),
                Lifetime = _config.Asteroids.Lifetime
            };
        }

        private PrototypeGameConfig.AsteroidSizeSettings GetSizeSettings(AsteroidType type)
        {
            return type switch
            {
                AsteroidType.Small => _config.Asteroids.Small,
                AsteroidType.Medium => _config.Asteroids.Medium,
                _ => _config.Asteroids.Large
            };
        }
    }
}
