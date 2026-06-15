using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceFlight2D.Game.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class LoadingSceneController : MonoBehaviour
    {
        private const string MainSceneName = "MainScene";
        private const string MainScenePath = "Assets/Game/Scenes/MainScene.unity";

        [SerializeField] private CanvasGroup _splashGroup;
        [SerializeField] private RectTransform _spinnerRoot;
        [SerializeField] private Image[] _spinnerDots;
        [SerializeField] private Text _statusText;

        public void Bind(CanvasGroup splashGroup)
        {
            _splashGroup = splashGroup;
        }

        public void Bind(
            CanvasGroup splashGroup,
            RectTransform spinnerRoot,
            Image[] spinnerDots,
            Text statusText)
        {
            _splashGroup = splashGroup;
            _spinnerRoot = spinnerRoot;
            _spinnerDots = spinnerDots;
            _statusText = statusText;
        }

        private async void Start()
        {
            try
            {
                if (_splashGroup != null)
                {
                    _splashGroup.alpha = 1f;
                    _splashGroup.interactable = false;
                    _splashGroup.blocksRaycasts = true;
                }

                await UniTask.Yield();

                var loadOperation = LoadMainSceneAsync();
                if (loadOperation == null)
                {
                    Debug.LogError($"Failed to start loading '{MainSceneName}'. Add '{MainScenePath}' to the active build profile.", this);
                    return;
                }

                while (!loadOperation.isDone)
                {
                    AnimateSpinner();
                    await UniTask.Yield();
                }

                var mainScene = SceneManager.GetSceneByName(MainSceneName);
                if (mainScene.IsValid())
                {
                    SceneManager.SetActiveScene(mainScene);
                }

                if (_splashGroup != null)
                {
                    _splashGroup.alpha = 0f;
                    _splashGroup.blocksRaycasts = false;
                }

                var loadingScene = gameObject.scene;
                if (loadingScene.IsValid())
                {
                    SceneManager.UnloadSceneAsync(loadingScene);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private static AsyncOperation LoadMainSceneAsync()
        {
            var buildIndex = SceneUtility.GetBuildIndexByScenePath(MainScenePath);
            if (buildIndex >= 0)
            {
                return SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            }

            return SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Additive);
        }

        private void AnimateSpinner()
        {
            var time = Time.unscaledTime;

            if (_spinnerRoot != null)
            {
                _spinnerRoot.localRotation = Quaternion.Euler(0f, 0f, -time * 240f);
            }

            if (_spinnerDots == null || _spinnerDots.Length == 0)
            {
                return;
            }

            for (var i = 0; i < _spinnerDots.Length; i++)
            {
                var dot = _spinnerDots[i];
                if (dot == null)
                {
                    continue;
                }

                var phase = time * 4.2f - i * 0.75f;
                var alpha = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(phase));
                var scale = 0.9f + 0.18f * (0.5f + 0.5f * Mathf.Sin(phase + 0.5f));
                dot.color = new Color(dot.color.r, dot.color.g, dot.color.b, alpha);
                dot.transform.localScale = Vector3.one * scale;
            }

            if (_statusText != null)
            {
                var dots = ((int)(time * 2.2f) % 3) + 1;
                _statusText.text = dots == 1 ? "Preparing scene graph." :
                    dots == 2 ? "Preparing scene graph.." : "Preparing scene graph...";
            }
        }
    }
}
