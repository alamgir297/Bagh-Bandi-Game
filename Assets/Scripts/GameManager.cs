using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    private bool _isGameOver = false;
    public  Vector3 _globalOffset = new Vector3(0, 0f, 0);
    public static GameManager Instance { get; private set; }
    public event Action<PlayerTurn> OnTurnChanged= delegate { };
    public event Action<bool> OnGameOver;

    PlayerTurn _currentPlayer;
    private PlayerTurn _winner;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame() {
        SceneManager.LoadScene(0);
    }

    public PlayerTurn CurrentPlayerTurn() {
        return _currentPlayer;
    }
    public void CurrentPlayerTurn(PlayerTurn player) {
        _currentPlayer = player;
    }
    public void ChangePlayerTurn() {
        if (!IsGameOver()) {
            _currentPlayer = (_currentPlayer == PlayerTurn.Predator) ? PlayerTurn.Prey : PlayerTurn.Predator;
            OnTurnChanged?.Invoke(_currentPlayer);
        }
    }
    public bool IsGameOver() => _isGameOver;

    public void IsGameOver(bool isGameOver) {
        _isGameOver = isGameOver;
    }
    public void GameOver() {
            _isGameOver = true;
            OnGameOver?.Invoke(_isGameOver);
    }
    public PlayerTurn IsWinnerPlayer() => _winner;
    public void IsWinnerPlayer(PlayerTurn playerTurn) {
        _winner = playerTurn;
    }
}

public enum PlayerTurn {
    Prey,
    Predator
}
