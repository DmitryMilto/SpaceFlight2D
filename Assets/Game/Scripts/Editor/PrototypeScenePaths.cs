namespace SpaceFlight2D.Editor
{
    public static class PrototypeScenePaths
    {
        public const string ScenePath = "Assets/Scenes/SampleScene.unity";
        public const string ConfigFolder = "Assets/Game/Configs";
        public const string PrefabFolder = "Assets/Game/Prefabs/Prototype";
        public const string ParticlePrefabFolder = PrefabFolder + "/Particles";
        public const string DefaultConfigPath = ConfigFolder + "/CleanGameplayPrototypeConfig.asset";
        public const string ActionConfigPath = ConfigFolder + "/ActionAdPrototypeConfig.asset";
        public const string NoUiConfigPath = ConfigFolder + "/NoUiRecordingPrototypeConfig.asset";
        public const string PurpleConfigPath = ConfigFolder + "/PurpleCosmosPrototypeConfig.asset";
        public const string BlueConfigPath = ConfigFolder + "/BlueCosmosPrototypeConfig.asset";
        public const string RedConfigPath = ConfigFolder + "/RedCosmosPrototypeConfig.asset";
        public const string GreenConfigPath = ConfigFolder + "/GreenCosmosPrototypeConfig.asset";
        public const string RainbowConfigPath = ConfigFolder + "/RainbowCosmosPrototypeConfig.asset";
        public const string ParticleCatalogPath = ConfigFolder + "/ParticlePrefabCatalog.asset";
        public const string TrailMaterialPath = ConfigFolder + "/RocketTrail.mat";
        public const string SpaceStarsMaterialPath = ConfigFolder + "/SpaceStars.mat";
        public const string RocketEnginePrefabPath = ParticlePrefabFolder + "/RocketEngine.prefab";
        public const string SpaceStarsPrefabPath = ParticlePrefabFolder + "/SpaceStars.prefab";
        public const string AsteroidHitPrefabPath = ParticlePrefabFolder + "/AsteroidHit.prefab";
        public const string RocketExplosionPrefabPath = ParticlePrefabFolder + "/RocketExplosion.prefab";
        public const string LaunchBurstPrefabPath = ParticlePrefabFolder + "/LaunchBurst.prefab";
        public const string FogTransitionPrefabPath = ParticlePrefabFolder + "/FogTransition.prefab";
        public const string FogZonePrefabPath = ParticlePrefabFolder + "/FogZone.prefab";
        public const string SpaceEntryPrefabPath = ParticlePrefabFolder + "/SpaceEntry.prefab";
        public const string SystemsPrefabPath = PrefabFolder + "/PrototypeSystems.prefab";
        public const string WorldPrefabPath = PrefabFolder + "/PrototypeWorld.prefab";
        public const string UiPrefabPath = PrefabFolder + "/PrototypeUi.prefab";
        public const string RocketPrefabPath = PrefabFolder + "/Rocket.prefab";
        public const string AsteroidPrefabPath = PrefabFolder + "/Asteroid.prefab";

        public static readonly string[] AllPrototypeConfigPaths =
        {
            DefaultConfigPath,
            ActionConfigPath,
            NoUiConfigPath,
            PurpleConfigPath,
            BlueConfigPath,
            RedConfigPath,
            GreenConfigPath,
            RainbowConfigPath
        };
    }
}
