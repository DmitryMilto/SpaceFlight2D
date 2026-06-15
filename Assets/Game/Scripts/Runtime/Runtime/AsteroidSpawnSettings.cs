using UnityEngine;

namespace SpaceFlight2D.Game.Runtime
{
    public enum AsteroidType
    {
        Small,
        Medium,
        Large
    }

    public struct AsteroidSpawnSettings
    {
        public AsteroidType Type;
        public Sprite VisualSprite;
        public Vector2 Size;
        public float Speed;
        public int ScoreReward;
        public Color Color;
        public float RotationSpeed;
        public float Lifetime;
    }
}
