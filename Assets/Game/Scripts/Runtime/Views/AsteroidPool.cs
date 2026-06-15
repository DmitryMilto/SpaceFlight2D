using System.Collections.Generic;
using UnityEngine;

namespace SpaceFlight2D.Game
{
    public sealed class AsteroidPool
    {
        private readonly Queue<Asteroid> _available = new();
        private readonly Asteroid[] _allAsteroids;
        private readonly Transform _asteroidRoot;

        public AsteroidPool(Asteroid[] allAsteroids, Transform asteroidRoot)
        {
            _allAsteroids = allAsteroids ?? System.Array.Empty<Asteroid>();
            _asteroidRoot = asteroidRoot;
        }

        public void Prewarm()
        {
            for (var i = 0; i < _allAsteroids.Length; i++)
            {
                var asteroid = _allAsteroids[i];
                if (asteroid == null)
                {
                    continue;
                }

                asteroid.transform.SetParent(_asteroidRoot, false);
                asteroid.gameObject.SetActive(false);
                _available.Enqueue(asteroid);
            }
        }

        public Asteroid Rent()
        {
            return _available.Count > 0 ? _available.Dequeue() : null;
        }

        public void Return(Asteroid asteroid)
        {
            if (asteroid == null)
            {
                return;
            }

            asteroid.transform.SetParent(_asteroidRoot, false);
            asteroid.gameObject.SetActive(false);
            _available.Enqueue(asteroid);
        }
    }
}
