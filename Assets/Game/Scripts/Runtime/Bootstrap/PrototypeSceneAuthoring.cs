using SpaceFlight2D.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceFlight2D.Game.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSceneAuthoring : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Transform _asteroidRoot;

        public Camera MainCamera => _mainCamera;
        public CameraController CameraController => _cameraController;
        public EventSystem EventSystem => _eventSystem;
        public Transform AsteroidRoot => _asteroidRoot;

        public void Bind(
            Camera mainCamera,
            CameraController cameraController,
            EventSystem eventSystem,
            Transform asteroidRoot)
        {
            _mainCamera = mainCamera;
            _cameraController = cameraController;
            _eventSystem = eventSystem;
            _asteroidRoot = asteroidRoot;
        }

        private void Awake()
        {
            if (Application.isPlaying && _mainCamera == null)
            {
                Debug.LogError("PrototypeSceneAuthoring expects scene objects to be baked by the editor builder.", this);
            }
        }

        public void EnsureBuilt()
        {
            if (!Application.isPlaying && (_mainCamera == null || _cameraController == null || _eventSystem == null || _asteroidRoot == null))
            {
                Debug.LogWarning("PrototypeSceneAuthoring is missing some prefab-baked references. The scene will still be saved, but runtime wiring may be incomplete.", this);
            }
        }
    }
}
