using UnityEngine;

namespace SpaceFlight2D.Game.Config
{
    [CreateAssetMenu(
        fileName = "ParticlePrefabCatalog",
        menuName = "SpaceFlight2D/Particle Prefab Catalog")]
    public sealed class ParticlePrefabCatalog : ScriptableObject
    {
        [field: SerializeField] public GameObject RocketEnginePrefab { get; set; }
        [field: SerializeField] public GameObject SpaceStarsPrefab { get; set; }
        [field: SerializeField] public GameObject AsteroidHitPrefab { get; set; }
        [field: SerializeField] public GameObject RocketExplosionPrefab { get; set; }
        [field: SerializeField] public GameObject LaunchBurstPrefab { get; set; }
        [field: SerializeField] public GameObject FogTransitionPrefab { get; set; }
        [field: SerializeField] public GameObject FogZonePrefab { get; set; }
        [field: SerializeField] public GameObject SpaceEntryPrefab { get; set; }
    }
}
