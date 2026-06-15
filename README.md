# SpaceFlight2D Prototype

Small Unity 2D portrait prototype built around a real gameplay loop: launch a rocket from a platform, steer with hold buttons, destroy asteroids for score, self-destruct to end the run, and reopen the mission with a restart flow. The same scene also supports marketing capture through creative presets and editor tooling.

## Gameplay Controls

- `Launch` starts the run.
- `Left` hold rotates and moves the rocket left.
- `Right` hold rotates and moves the rocket right.
- `Destroy Rocket` ends the run and triggers the result panel.
- `Restart` reloads the scene from the result panel.

## Architecture

- `GameConfig` stores gameplay, asteroid, VFX, UI, and creative recording parameters.
- `SceneContext` owns the scene lifecycle and runs `GameInstaller` through normal Zenject flow.
- `GameInstaller` binds `GameConfig`, `IGameStateService`, `IScoreService`, `IVfxService`, `ICameraShakeService`, `IAsteroidSpawnSettingsProvider`, and scene MonoBehaviours.
- `LoadingScene` shows a splash screen, loads `MainScene`, and then hands off to gameplay.
- `GameplayLoop` coordinates state transitions: `Idle`, `Launching`, `Playing`, `RocketDestroyed`, `Result`.
- `RocketController` handles upward flight, rotation, collider state, trail, and explosion timing.
- `AsteroidSpawner` runs an async UniTask spawn loop and requests `AsteroidSpawnSettings` from `IAsteroidSpawnSettingsProvider`.
- `Asteroid` receives `Initialize(AsteroidSpawnSettings settings)` and applies size, color, speed, reward, rotation, and lifetime.
- `UIController` manages portrait HUD, hold buttons, DOTween UI transitions, score punch, and result count-up.

## Configs

Preset config assets live in [Assets/Game/Configs](/Users/severex_unt_milto/UnityProjects/SpaceFlight2D/Assets/Game/Configs):

- `CleanGameplayConfig.asset`
- `ActionAdConfig.asset`
- `NoUiRecordingConfig.asset`

The playable scene builder uses `CleanGameplayConfig.asset` by default.

## Marketing Sandbox

Open `Tools/Rocket Creative Sandbox`.

Available actions:

- Apply `Clean Gameplay` preset
- Apply `Action Ad` preset
- Apply `No UI Recording` preset
- Toggle UI
- Toggle VFX
- Toggle Screen Shake
- Apply selected config values for spawn rate, camera zoom, background color, rocket color, and asteroid colors

## Scene Setup

Fastest setup path inside Unity:

1. Run `Tools/Rocket/Create Config Assets`.
2. Run `Tools/Rocket/Rebuild Prototype Scene`.
3. Open [Assets/Scenes/LoadingScene.unity](/Users/severex_unt_milto/UnityProjects/SpaceFlight2D/Assets/Scenes/LoadingScene.unity).
4. Confirm the scene contains `Main Camera`, `SceneContext`, `GameRoot`, `Rocket`, `Platform`, `AsteroidSpawner`, `Canvas`, and `VfxRoot` in [Assets/Scenes/MainScene.unity](/Users/severex_unt_milto/UnityProjects/SpaceFlight2D/Assets/Scenes/MainScene.unity).

Object sizing in the generated scene:

- Rocket: `0.8 x 1.8`
- Rocket collider: `0.55 x 1.5`
- Platform: `2.4 x 0.25`
- Camera orthographic size: `5`
- Canvas reference resolution: `1080 x 1920`

## Recorder

To record a vertical creative:

1. Open `Window > General > Recorder > Recorder Window`.
2. Add `Movie Recorder`.
3. Set `Game View` output to `1080 x 1920`.
4. Apply a creative preset in `Tools/Rocket Creative Sandbox`.
5. Enter play mode and record one gameplay pass per preset.

## AI Usage

AI tools were used to speed up architecture planning, generate initial C# scripts and prepare documentation. All gameplay decisions, scene setup, configuration values, dependency wiring and final code review were done manually.

## Improvements

- Add object pooling for asteroids and VFX.
- Add more asteroid movement patterns and scripted waves.
- Add cinematic camera paths for ad capture.
- Add more rocket skins and color packs.
- Add Timeline-based creative sequences.
- Add Recorder preset assets.
- Add Addressables for creative packs.
- Polish mobile safe area handling.
- Improve VFX variations.
- Add AI-generated sprite experiments as optional visual packs.
