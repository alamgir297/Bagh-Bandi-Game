using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUiManager : MonoBehaviour {

    //[SerializeField] PredatorController _playerController;
    [SerializeField] PlayerController _playerController;
    [SerializeField] TextMeshProUGUI _turnMessagePrey;
    [SerializeField] TextMeshProUGUI _turnMessagePredator;
    [SerializeField] TextMeshProUGUI _preyCountText;
    [SerializeField] TextMeshProUGUI _captureCountText;
    [SerializeField] TextMeshProUGUI _winnierMessage;
    [SerializeField] Button _restartButton;
    [SerializeField] GameObject _gameOverPanel;
    [SerializeField] GameObject _settingsPanel;
    void Start() {
        GameManager.Instance.OnTurnChanged += ShowPlayerTurnMessage;
        _playerController.OnPreyCountChanged += UpdatePreyCountUI;
        GameManager.Instance.OnGameOver += ShowGameOverUI;
        ShowPlayerTurnMessage(GameManager.Instance.CurrentPlayerTurn());
    }

    void Update() {

    }

    private void OnEnable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTurnChanged += ShowPlayerTurnMessage;
            _playerController.OnPreyCountChanged += UpdatePreyCountUI;
            GameManager.Instance.OnGameOver += ShowGameOverUI;
        }
    }
    private void OnDisable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTurnChanged -= ShowPlayerTurnMessage;
            _playerController.OnPreyCountChanged -= UpdatePreyCountUI;
            GameManager.Instance.OnGameOver -= ShowGameOverUI;
        }
    }
    private void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTurnChanged -= ShowPlayerTurnMessage;
            _playerController.OnPreyCountChanged -= UpdatePreyCountUI;
            GameManager.Instance.OnGameOver -= ShowGameOverUI;
        }
    }

    public void StartGame() {
        if (GameManager.Instance != null) {
            GameManager.Instance.StartGame();
        }
    }
    private void ShowPlayerTurnMessage(PlayerTurn playerTurn) {
        _turnMessagePredator.gameObject.SetActive(playerTurn == PlayerTurn.Predator);
        _turnMessagePrey.gameObject.SetActive(playerTurn == PlayerTurn.Prey);
    }

    private void UpdatePreyCountUI(int preyCount) {
        _preyCountText.text = "" + preyCount + "/16";
        _captureCountText.text = "" + (16 - preyCount) + "/16";
    }

    private void ShowGameOverUI(bool isOver) {
        _gameOverPanel.SetActive(true);
        _winnierMessage.gameObject.SetActive(isOver);
        _winnierMessage.text ="Game Over\n"+ GetWinnerPlayer()+" won!";
    }

    public void ToggleSettingsPanel() {
        _settingsPanel.SetActive(!_settingsPanel.activeSelf);
    }
    private PlayerTurn GetCurrentPlayer() {
        return GameManager.Instance.CurrentPlayerTurn();
    }
    private PlayerTurn GetWinnerPlayer() {
        return GameManager.Instance.IsWinnerPlayer();
    }
}
