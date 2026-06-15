using UnityEngine;

namespace SpaceFlight2D.Game
{
    public static class PrimitiveSpriteLibrary
    {
        private static Sprite _squareSprite;
        private static Sprite _circleSprite;
        private static Sprite _softCircleSprite;

        public static Sprite SquareSprite => _squareSprite ??= CreateSquareSprite();
        public static Sprite CircleSprite => _circleSprite ??= CreateCircleSprite();
        public static Sprite SoftCircleSprite => _softCircleSprite ??= CreateSoftCircleSprite();

        private static Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 32f);
        }

        private static Sprite CreateCircleSprite()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.44f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(center, new Vector2(x, y));
                    var alpha = distance <= radius ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }

        private static Sprite CreateSoftCircleSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.48f;
            var edgeFalloff = size * 0.12f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(center, new Vector2(x, y));
                    var alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(radius - edgeFalloff, radius, distance));
                    alpha = alpha * alpha;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
