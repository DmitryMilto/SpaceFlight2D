using SpaceFlight2D.Game.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFlight2D.Game
{
    [DisallowMultipleComponent]
    public sealed class PrototypeUiAuthoring : MonoBehaviour
    {
        [SerializeField] private UIController _controller;
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

        public UIController Controller => _controller;

        public void Bind(
            UIController controller,
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
            _controller = controller;
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

        public void Apply(PrototypeGameConfig config)
        {
            if (_controller == null)
            {
                return;
            }

            _controller.Bind(
                _startUIGroup,
                _gameplayUIGroup,
                _resultUIGroup,
                _controlsGroup,
                _launchButtonGroup,
                _destroyButtonGroup,
                _restartButtonGroup,
                _launchButton,
                _leftButton,
                _rightButton,
                _destroyRocketButton,
                _restartButton,
                _gameTitleText,
                _hintText,
                _scoreText,
                _resultOverlay,
                _resultOverlayGroup,
                _resultPanel,
                _resultTitleText,
                _finalScoreText);

            _controller.ApplyAuthoringState();
            _controller.ApplyVisuals(config);
        }
    }
}
