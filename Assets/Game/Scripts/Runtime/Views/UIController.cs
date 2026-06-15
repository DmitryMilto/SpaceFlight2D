using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SpaceFlight2D.Game.Bootstrap;
using SpaceFlight2D.Game.Config;
using SpaceFlight2D.Game.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace SpaceFlight2D.Game
{
    public sealed class UIController : MonoBehaviour, IInitializable
    {
        [SerializeField] private CanvasGroup _startUIGroup;
        [SerializeField] private CanvasGroup _gameplayUIGroup;
        [SerializeField] private CanvasGroup _resultUIGroup;
        [SerializeField] private CanvasGroup _controlsGroup;
        [SerializeField] private CanvasGroup _launchButtonGroup;
        [SerializeField] private CanvasGroup _destroyButtonGroup;
        [SerializeField] private CanvasGroup _restartButtonGroup;
        [SerializeField] private Button _launchButton;
        [SerializeField] private HoldButton _leftButton;
        [SerializeField] private HoldButton _rightButton;
        [SerializeField] private Button _destroyRocketButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Text _gameTitleText;
        [SerializeField] private Text _hintText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Image _resultOverlay;
        [SerializeField] private CanvasGroup _resultOverlayGroup;
        [SerializeField] private RectTransform _resultPanel;
        [SerializeField] private Text _resultTitleText;
        [SerializeField] private Text _finalScoreText;

        private GameplayLoop _gameplayLoop;
        private GameBootstrapper _gameBootstrapper;
        private IGameStateService _gameStateService;
        private IScoreService _scoreService;
        private Canvas _canvas;
        private Tween _scoreTween;
        private Tween _finalScoreTween;
        private bool _uiVisible = true;
        private bool _controlsVisible = true;
        private bool _initialized;
        private int _currentScore;

        [Inject]
        public void Construct(
            GameplayLoop gameplayLoop,
            GameBootstrapper gameBootstrapper,
            IGameStateService gameStateService,
            IScoreService scoreService)
        {
            _gameplayLoop = gameplayLoop;
            _gameBootstrapper = gameBootstrapper;
            _gameStateService = gameStateService;
            _scoreService = scoreService;
        }

        public void Bind(
            CanvasGroup startUIGroup,
            CanvasGroup gameplayUIGroup,
            CanvasGroup resultUIGroup,
            CanvasGroup controlsGroup,
            CanvasGroup launchButtonGroup,
            CanvasGroup destroyButtonGroup,
            CanvasGroup restartButtonGroup,
            Button launchButton,
            HoldButton leftButton,
            HoldButton rightButton,
            Button destroyRocketButton,
            Button restartButton,
            Text gameTitleText,
            Text hintText,
            Text scoreText,
            Image resultOverlay,
            CanvasGroup resultOverlayGroup,
            RectTransform resultPanel,
            Text resultTitleText,
            Text finalScoreText)
        {
            _startUIGroup = startUIGroup;
            _gameplayUIGroup = gameplayUIGroup;
            _resultUIGroup = resultUIGroup;
            _controlsGroup = controlsGroup;
            _launchButtonGroup = launchButtonGroup;
            _destroyButtonGroup = destroyButtonGroup;
            _restartButtonGroup = restartButtonGroup;
            _launchButton = launchButton;
            _leftButton = leftButton;
            _rightButton = rightButton;
            _destroyRocketButton = destroyRocketButton;
            _restartButton = restartButton;
            _gameTitleText = gameTitleText;
            _hintText = hintText;
            _scoreText = scoreText;
            _resultOverlay = resultOverlay;
            _resultOverlayGroup = resultOverlayGroup;
            _resultPanel = resultPanel;
            _resultTitleText = resultTitleText;
            _finalScoreText = finalScoreText;
        }

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (_gameStateService == null || _scoreService == null)
            {
                Debug.LogError("UIController was initialized before dependencies were injected.", this);
                return;
            }

            _launchButton?.onClick.AddListener(HandleLaunchButtonClicked);
            _destroyRocketButton?.onClick.AddListener(HandleDestroyButtonClickedAndFinish);
            _restartButton?.onClick.AddListener(HandleRestartButtonClicked);

            _scoreService.ScoreChanged += HandleScoreChanged;
            _gameStateService.StateChanged += HandleStateChanged;
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (!_initialized)
            {
                return;
            }

            _launchButton?.onClick.RemoveListener(HandleLaunchButtonClicked);
            _destroyRocketButton?.onClick.RemoveListener(HandleDestroyButtonClickedAndFinish);
            _restartButton?.onClick.RemoveListener(HandleRestartButtonClicked);

            if (_scoreService != null)
            {
                _scoreService.ScoreChanged -= HandleScoreChanged;
            }

            if (_gameStateService != null)
            {
                _gameStateService.StateChanged -= HandleStateChanged;
            }

            _initialized = false;
        }

        public void ResetUiState()
        {
            _currentScore = 0;
            SetUIVisible(true);
            ShowStartUI();
            UpdateScore(0);
            HideResultElementsImmediate();
        }

        public void ApplyAuthoringState()
        {
            _currentScore = 0;

            if (_gameTitleText != null)
            {
                _gameTitleText.transform.localScale = Vector3.one * 0.85f;
            }

            if (_hintText != null)
            {
                _hintText.color = new Color(1f, 1f, 1f, 0.4f);
            }

            if (_scoreText != null)
            {
                _scoreText.text = "SCORE: 0";
                _scoreText.rectTransform.localScale = Vector3.one;
            }

            if (_resultOverlay != null)
            {
                _resultOverlay.color = new Color(0f, 0f, 0f, 1f);
                _resultOverlay.gameObject.SetActive(false);
            }

            if (_resultOverlayGroup != null)
            {
                _resultOverlayGroup.alpha = 0f;
                _resultOverlayGroup.interactable = false;
                _resultOverlayGroup.blocksRaycasts = false;
                _resultOverlayGroup.gameObject.SetActive(false);
            }

            if (_resultPanel != null)
            {
                _resultPanel.localScale = Vector3.zero;
            }

            if (_resultTitleText != null)
            {
                _resultTitleText.color = new Color(1f, 1f, 1f, 0f);
            }

            if (_finalScoreText != null)
            {
                _finalScoreText.color = new Color(1f, 1f, 1f, 0f);
            }

            if (_restartButtonGroup != null)
            {
                _restartButtonGroup.alpha = 0f;
                _restartButtonGroup.interactable = false;
                _restartButtonGroup.blocksRaycasts = false;
            }

            if (_launchButtonGroup != null)
            {
                _launchButtonGroup.alpha = 1f;
                _launchButtonGroup.interactable = true;
                _launchButtonGroup.blocksRaycasts = true;
            }

            if (_destroyButtonGroup != null)
            {
                _destroyButtonGroup.alpha = 1f;
                _destroyButtonGroup.interactable = true;
                _destroyButtonGroup.blocksRaycasts = true;
            }

            if (_restartButton != null)
            {
                _restartButton.transform.localScale = Vector3.one * 0.8f;
            }

            SetCanvasGroupVisible(_startUIGroup, true);
            SetCanvasGroupVisible(_gameplayUIGroup, false);
            SetCanvasGroupVisible(_resultUIGroup, false);
            SetControlsVisible(true);
            HideResultElementsImmediate();
        }

        public float GetHorizontalInput()
        {
            var input = 0f;

            if (_leftButton != null && _leftButton.IsHeld)
            {
                input -= 1f;
            }

            if (_rightButton != null && _rightButton.IsHeld)
            {
                input += 1f;
            }

            return Mathf.Clamp(input, -1f, 1f);
        }

        public void ShowStartUI()
        {
            if (!_uiVisible)
            {
                return;
            }

            SetCanvasGroupVisible(_startUIGroup, true);
            SetCanvasGroupVisible(_gameplayUIGroup, false);
            SetCanvasGroupVisible(_resultUIGroup, false);
            HideResultElementsImmediate();

            ResetStartVisuals();
            PlayStartVisuals();
        }

        public void ShowGameplayUI()
        {
            if (!_uiVisible)
            {
                return;
            }

            SetCanvasGroupVisible(_startUIGroup, false);
            SetCanvasGroupVisible(_resultUIGroup, false);
            HideResultElementsImmediate();
            SetControlsVisible(_controlsVisible);

            if (_gameplayUIGroup != null)
            {
                _gameplayUIGroup.gameObject.SetActive(true);
                _gameplayUIGroup.DOKill();
                _gameplayUIGroup.alpha = 0f;
                _gameplayUIGroup.interactable = true;
                _gameplayUIGroup.blocksRaycasts = true;
            }

            FadeCanvasGroup(_gameplayUIGroup, 1f, 0.25f);
            PlayGameplayVisuals();
        }

        public void HideGameplayUI()
        {
            FadeCanvasGroup(_gameplayUIGroup, 0f, 0.2f,
                onComplete: () => { SetCanvasGroupVisible(_gameplayUIGroup, false); });
        }

        public void ShowResultUI(int finalScore)
        {
            if (!_uiVisible)
            {
                return;
            }

            SetCanvasGroupVisible(_startUIGroup, false);
            SetCanvasGroupVisible(_gameplayUIGroup, false);
            SetControlsVisible(false);
            SetCanvasGroupVisible(_resultUIGroup, true);

            if (_resultOverlay != null)
            {
                _resultOverlay.gameObject.SetActive(true);
                _resultOverlay.color = new Color(0f, 0f, 0f, 1f);
            }

            if (_resultOverlayGroup != null)
            {
                _resultOverlayGroup.gameObject.SetActive(true);
                _resultOverlayGroup.DOKill();
                _resultOverlayGroup.alpha = 0f;
                _resultOverlayGroup.interactable = false;
                _resultOverlayGroup.blocksRaycasts = false;
                _resultOverlayGroup.DOFade(0.55f, 0.25f).SetEase(Ease.Linear).SetUpdate(true);
            }

            if (_resultPanel != null)
            {
                _resultPanel.DOKill();
                _resultPanel.localScale = Vector3.zero;
                _resultPanel.DOScale(1f, 0.38f).SetDelay(0.15f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (_resultTitleText != null)
            {
                _resultTitleText.DOKill();
                _resultTitleText.color = new Color(
                    _resultTitleText.color.r,
                    _resultTitleText.color.g,
                    _resultTitleText.color.b,
                    0f);
                _resultTitleText.DOFade(1f, 0.2f).SetDelay(0.25f).SetEase(Ease.Linear).SetUpdate(true);
            }

            if (_finalScoreText != null)
            {
                _finalScoreText.DOKill();
                _finalScoreText.color = new Color(
                    _finalScoreText.color.r,
                    _finalScoreText.color.g,
                    _finalScoreText.color.b,
                    0f);
                _finalScoreText.DOFade(1f, 0.2f).SetDelay(0.25f).SetEase(Ease.Linear).SetUpdate(true);
            }

            if (_restartButtonGroup != null)
            {
                _restartButtonGroup.DOKill();
                _restartButtonGroup.alpha = 0f;
                _restartButtonGroup.interactable = true;
                _restartButtonGroup.blocksRaycasts = true;
                _restartButtonGroup.DOFade(1f, 0.25f).SetDelay(0.55f).SetEase(Ease.Linear).SetUpdate(true);
            }

            if (_restartButton != null)
            {
                _restartButton.interactable = true;
                _restartButton.transform.DOKill();
                _restartButton.transform.localScale = Vector3.one * 0.8f;
                _restartButton.transform.DOScale(1f, 0.25f).SetDelay(0.55f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            SetFinalScoreAnimated(finalScore);
        }

        public void UpdateScore(int score)
        {
            _currentScore = score;

            if (_scoreText != null)
            {
                _scoreText.text = $"SCORE: {score}";
                _scoreText.rectTransform.DOKill();
                _scoreText.rectTransform.localScale = Vector3.one;
                _scoreTween?.Kill();
                _scoreTween = _scoreText.rectTransform.DOPunchScale(Vector3.one * 0.22f, 0.22f, 6, 0.7f)
                    .SetUpdate(true);

                var scoreColor = _scoreText.color;
                _scoreText.DOKill();
                _scoreText.color = new Color(scoreColor.r, scoreColor.g, scoreColor.b, 1f);
                _scoreText.DOColor(new Color(1f, 0.93f, 0.35f, 1f), 0.12f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true);
            }
        }

        public void SetUIVisible(bool visible)
        {
            _uiVisible = visible;

            if (_canvas != null)
            {
                _canvas.enabled = visible;
            }

            if (!visible)
            {
                SetCanvasGroupVisible(_startUIGroup, false);
                SetCanvasGroupVisible(_gameplayUIGroup, false);
                SetCanvasGroupVisible(_resultUIGroup, false);
                HideResultElementsImmediate();
            }
        }

        public void SetControlsVisible(bool visible)
        {
            _controlsVisible = visible;
            SetCanvasGroupVisible(_controlsGroup, visible);
        }

        public void ApplyVisuals(PrototypeGameConfig config)
        {
            if (config == null)
            {
                return;
            }

            SetUIVisible(config.Ui.Enabled && !config.Recording.HideUiForRecording);
            SetControlsVisible(config.Ui.ShowControls);

            if (_gameTitleText != null)
            {
                _gameTitleText.color = Color.white;
            }

            if (_hintText != null)
            {
                _hintText.color = new Color(1f, 1f, 1f, 0.4f);
            }

            if (_scoreText != null)
            {
                _scoreText.color = Color.white;
                _scoreText.gameObject.SetActive(config.Ui.ShowScore);
            }

            if (_resultTitleText != null)
            {
                _resultTitleText.color = Color.white;
            }

            if (_finalScoreText != null)
            {
                _finalScoreText.color = Color.white;
            }
        }

        public void ShowLaunchPrompt(bool isVisible)
        {
            if (isVisible)
            {
                ShowStartUI();
            }
            else
            {
                HideStartUI();
            }
        }

        public void SetFlightHint(bool isInSpace)
        {
            if (_hintText == null)
            {
                return;
            }

            _hintText.text = isInSpace ? "Tap and hold to steer" : "Tap launch to start";
        }

        public void ShowFlightOverlay(bool isVisible)
        {
            if (_gameplayUIGroup == null)
            {
                return;
            }

            if (isVisible)
            {
                ShowGameplayUI();
            }
            else
            {
                HideGameplayUI();
            }
        }

        public void SetScore(int score)
        {
            UpdateScore(score);
        }

        public void SetSelfDestructInteractable(bool interactable)
        {
            if (_destroyRocketButton != null)
            {
                _destroyRocketButton.interactable = interactable;
            }
        }

        public void ShowResults(bool isVisible, int score)
        {
            if (isVisible)
            {
                ShowResultUI(score);
            }
            else
            {
                SetCanvasGroupVisible(_resultUIGroup, false);
            }
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Idle:
                    SetFlightHint(false);
                    ShowStartUI();
                    break;
                case GameState.Launching:
                    if (_hintText != null)
                    {
                        _hintText.text = "Climbing through atmosphere";
                    }

                    HideStartUI();
                    break;
                case GameState.Playing:
                    SetFlightHint(true);
                    ShowGameplayUI();
                    break;
                case GameState.RocketDestroyed:
                    HideGameplayUI();
                    break;
                case GameState.Result:
                    ShowResultUI(_currentScore);
                    break;
            }
        }

        private void HandleScoreChanged(int score)
        {
            UpdateScore(score);
        }

        private void HandleLaunchButtonClicked()
        {
            PlayLaunchButtonClick();
            StartGame();
        }

        private void HandleDestroyButtonClicked()
        {
            AnimateButtonClick(_destroyRocketButton, 1.0f, 0.88f, 1.0f, 0.08f, 0.12f);
            HideGameplayUI();
        }

        private void HandleDestroyButtonClickedAndFinish()
        {
            HandleDestroyButtonClicked();
            FinishGame();
        }

        private void HandleRestartButtonClicked()
        {
            AnimateButtonClick(_restartButton, 1f, 0.92f, 1f, 0.08f, 0.07f);

            DOVirtual.DelayedCall(0.15f, RestartScene)
                .SetUpdate(true);
        }

        public void PlayLaunchButtonClick()
        {
            AnimateButtonClick(_launchButton, 1.0f, 0.92f, 1.1f, 0.08f, 0.12f);
            FadeCanvasGroup(_startUIGroup, 0f, 0.2f,
                onComplete: () => { SetCanvasGroupVisible(_startUIGroup, false); });
        }

        private void StartGame()
        {
            if (_gameBootstrapper != null)
            {
                _gameBootstrapper.StartGameAsync().Forget();
                return;
            }

            _gameplayLoop?.StartLaunch();
        }

        private void FinishGame()
        {
            if (_gameBootstrapper != null)
            {
                _gameBootstrapper.FinishGameAsync().Forget();
                return;
            }

            _gameplayLoop?.DestroyRocket();
        }

        private void ShowGameplayVisuals()
        {
            if (_destroyButtonGroup != null)
            {
                _destroyButtonGroup.DOKill();
                _destroyButtonGroup.alpha = 1f;
                _destroyButtonGroup.interactable = true;
                _destroyButtonGroup.blocksRaycasts = true;
            }

            if (_destroyRocketButton != null)
            {
                _destroyRocketButton.transform.DOKill();
                _destroyRocketButton.transform.localScale = Vector3.one;
                _destroyRocketButton.transform
                    .DOScale(1.04f, 0.9f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }
        }

        private void PlayStartVisuals()
        {
            if (_gameTitleText != null)
            {
                _gameTitleText.transform.DOKill();
                _gameTitleText.transform.localScale = Vector3.one * 0.85f;
                _gameTitleText.transform.DOScale(1f, 0.45f).SetEase(Ease.OutBack).SetUpdate(true);
                _gameTitleText.transform
                    .DOScale(1.04f, 0.6f)
                    .SetDelay(0.45f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }

            if (_hintText != null)
            {
                _hintText.DOKill();
                _hintText.color = new Color(_hintText.color.r, _hintText.color.g, _hintText.color.b, 0.4f);
                _hintText.DOFade(1f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
            }

            if (_launchButton != null)
            {
                _launchButton.transform.DOKill();
                _launchButton.transform.localScale = Vector3.one;
                _launchButton.transform
                    .DOScale(1.06f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }
        }

        private void PlayGameplayVisuals()
        {
            SetControlsVisible(_controlsVisible);

            if (_launchButtonGroup != null)
            {
                _launchButtonGroup.DOKill();
                _launchButtonGroup.alpha = 0f;
                _launchButtonGroup.interactable = false;
                _launchButtonGroup.blocksRaycasts = false;
            }

            if (_destroyButtonGroup != null)
            {
                _destroyButtonGroup.DOKill();
                _destroyButtonGroup.alpha = 1f;
                _destroyButtonGroup.interactable = true;
                _destroyButtonGroup.blocksRaycasts = true;
                _destroyButtonGroup.transform.localScale = Vector3.one;
            }

            if (_destroyRocketButton != null)
            {
                _destroyRocketButton.transform.DOKill();
                _destroyRocketButton.transform.localScale = Vector3.one;
                _destroyRocketButton.transform
                    .DOScale(1.04f, 0.9f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            }
        }

        private void ResetStartVisuals()
        {
            if (_gameTitleText != null)
            {
                _gameTitleText.transform.DOKill();
                _gameTitleText.transform.localScale = Vector3.one * 0.85f;
            }

            if (_hintText != null)
            {
                _hintText.DOKill();
                _hintText.color = new Color(_hintText.color.r, _hintText.color.g, _hintText.color.b, 0.4f);
            }

            if (_launchButton != null)
            {
                _launchButton.transform.DOKill();
                _launchButton.transform.localScale = Vector3.one;
            }

            if (_launchButtonGroup != null)
            {
                _launchButtonGroup.alpha = 1f;
                _launchButtonGroup.interactable = true;
                _launchButtonGroup.blocksRaycasts = true;
            }
        }

        public void HideStartUI()
        {
            FadeCanvasGroup(_startUIGroup, 0f, 0.2f,
                onComplete: () => { SetCanvasGroupVisible(_startUIGroup, false); });
        }

        private void HideResultElementsImmediate()
        {
            _finalScoreTween?.Kill();

            if (_resultOverlay != null)
            {
                _resultOverlay.gameObject.SetActive(false);
            }

            if (_resultOverlayGroup != null)
            {
                _resultOverlayGroup.gameObject.SetActive(false);
                _resultOverlayGroup.alpha = 0f;
                _resultOverlayGroup.interactable = false;
                _resultOverlayGroup.blocksRaycasts = false;
            }

            if (_resultPanel != null)
            {
                _resultPanel.localScale = Vector3.zero;
            }

            if (_resultTitleText != null)
            {
                _resultTitleText.color = new Color(
                    _resultTitleText.color.r,
                    _resultTitleText.color.g,
                    _resultTitleText.color.b,
                    0f);
            }

            if (_finalScoreText != null)
            {
                _finalScoreText.text = "SCORE: 0";
                _finalScoreText.color = new Color(
                    _finalScoreText.color.r,
                    _finalScoreText.color.g,
                    _finalScoreText.color.b,
                    0f);
            }

            if (_restartButtonGroup != null)
            {
                _restartButtonGroup.alpha = 0f;
                _restartButtonGroup.interactable = false;
                _restartButtonGroup.blocksRaycasts = false;
            }

            if (_restartButton != null)
            {
                _restartButton.transform.localScale = Vector3.one * 0.8f;
            }
        }

        private void SetFinalScoreAnimated(int finalScore)
        {
            if (_finalScoreText == null)
            {
                return;
            }

            _finalScoreTween?.Kill();

            var displayedScore = 0;
            _finalScoreText.text = "SCORE: 0";
            _finalScoreTween = DOTween.To(() => displayedScore, value =>
                {
                    displayedScore = value;
                    _finalScoreText.text = $"SCORE: {displayedScore}";
                }, finalScore, 0.8f)
                .SetDelay(0.25f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (_finalScoreText != null)
                    {
                        _finalScoreText.rectTransform.DOKill();
                        _finalScoreText.rectTransform.DOPunchScale(Vector3.one * 0.18f, 0.25f, 6, 0.7f)
                            .SetUpdate(true);
                    }
                });
        }

        private void AnimateButtonClick(Button button, float startScale, float pressedScale, float overshootScale,
            float pressDuration, float overshootDuration)
        {
            if (button == null)
            {
                return;
            }

            var transform = button.transform;
            transform.DOKill();
            transform.localScale = Vector3.one * startScale;

            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.Append(transform.DOScale(pressedScale, pressDuration).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(overshootScale, overshootDuration).SetEase(Ease.OutBack));
            sequence.Append(transform.DOScale(startScale, 0.08f).SetEase(Ease.OutQuad));
        }

        private static void SetCanvasGroupVisible(CanvasGroup canvasGroup, bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private static void FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration,
            Action onComplete = null)
        {
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            canvasGroup.DOKill();
            canvasGroup.interactable = targetAlpha > 0f;
            canvasGroup.blocksRaycasts = targetAlpha > 0f;
            canvasGroup.DOFade(targetAlpha, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() => { onComplete?.Invoke(); });
        }

        private static void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }
    }
}
