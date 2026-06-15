using SpaceFlight2D.Game;
using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;
using Zenject;

namespace SpaceFlight2D.Game.Bootstrap
{
    public sealed class GameInstaller : MonoInstaller
    {
        [SerializeField] private PrototypeSceneReferences _sceneReferences;

        public override void InstallBindings()
        {
            InstallInto(Container);
        }

        public void Bind(PrototypeSceneReferences sceneReferences)
        {
            _sceneReferences = sceneReferences;
        }

        public void InstallInto(DiContainer container)
        {
            _sceneReferences.ValidateOrThrow();

            container.BindInstance(_sceneReferences);
            container.Bind<PrototypeGameConfig>().FromInstance(_sceneReferences.Config).AsSingle();
            container.BindInterfacesAndSelfTo<GameStateService>().AsSingle();
            container.BindInterfacesAndSelfTo<ScoreService>().AsSingle();
            container.BindInterfacesAndSelfTo<VfxService>().FromInstance(_sceneReferences.VfxService);
            container.BindInterfacesAndSelfTo<CameraShakeService>().FromInstance(_sceneReferences.CameraShakeService);
            container.Bind<IAsteroidSpawnSettingsProvider>().To<AsteroidSpawnSettingsProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<UIController>().FromInstance(_sceneReferences.UiController);
            container.BindInterfacesAndSelfTo<AsteroidSpawner>().FromInstance(_sceneReferences.Spawner);
            container.BindInstance(_sceneReferences.GlobalVolume);
            if (_sceneReferences.BackgroundController != null)
            {
                container.BindInterfacesAndSelfTo<BackgroundController>().FromInstance(_sceneReferences.BackgroundController);
            }
            if (_sceneReferences.CameraController != null)
            {
                container.BindInterfacesAndSelfTo<CameraController>().FromInstance(_sceneReferences.CameraController);
            }
            if (_sceneReferences.GameBootstrapper != null)
            {
                container.BindInterfacesAndSelfTo<GameBootstrapper>().FromInstance(_sceneReferences.GameBootstrapper);
            }
            container.BindInstance(_sceneReferences.GameplayLoop);
            container.BindInstance(_sceneReferences.Rocket);
            container.BindInstance(_sceneReferences.Customizer);

            container.BindExecutionOrder<BackgroundController>(-20);
            container.BindExecutionOrder<CameraShakeService>(-15);
            container.BindExecutionOrder<VfxService>(-10);
            container.BindExecutionOrder<UIController>(-5);
            container.BindExecutionOrder<AsteroidSpawner>(0);
            container.BindExecutionOrder<CameraController>(5);
            container.BindExecutionOrder<GameBootstrapper>(10);
        }
    }
}
