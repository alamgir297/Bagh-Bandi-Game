using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private int _preyCount = 16;

    private GameLogic _gameLogic;
    private Marker _marker;
    private InputHandler inputHandler;
    private SpriteRenderer _renderer;
    private SelectedData _selectedData= new SelectedData(null, null);

    public event Action<int> OnPreyCountChanged;

    [SerializeField] private GameObject _preyPrefab;
    [SerializeField] private GameObject _predatorPrefab;
    [SerializeField] private GameObject _markerList;
    [SerializeField] private GameObject _predatorInitialPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _gameLogic = GetComponent<GameLogic>();
        PlacePreyOnBoard();
        GameManager.Instance.CurrentPlayerTurn(PlayerTurn.Predator);
        GameManager.Instance.IsGameOver(false);
        _preyCount = 16;
        PlacePredatorOnBoard(_predatorInitialPosition);
        inputHandler = GameObject.Find("InputHandler").GetComponent<InputHandler>();
        inputHandler.OnSelected += GetSelectedData;

    }

    void Update() {
        if (inputHandler.IsSelectedAPiece()) {
            if (_selectedData.IsPieceSelected()) {
                _gameLogic.ShowValidMovesForThisPiece(_selectedData.Selected);
                if (_selectedData.IsTargetSelected()) {
                    HandlePlayerTurn();
                }
            }
            _selectedData = new SelectedData(null, null);
        }
    }
    private void OnEnable() {
        if (inputHandler != null) {
            inputHandler.OnSelected += GetSelectedData;
        }
    }
    private void OnDisable() {
        if (inputHandler != null) {
            inputHandler.OnSelected -= GetSelectedData;
        }
    }
    private void OnDestroy() {
        if (inputHandler != null) {
            inputHandler.OnSelected -= GetSelectedData;
        }
    }
    private void HandlePlayerTurn() {
        if (GameManager.Instance.CurrentPlayerTurn() == PlayerTurn.Predator) {
            _gameLogic.PredatorMove(_selectedData.Selected, _selectedData.Target);
            if (_gameLogic.CaptureMovePerformed()) CaptureMoveEffect();
        }
        else {
            _gameLogic.PreyMove(_selectedData.Selected, _selectedData.Target);
        }
    }
    private void PlacePredatorOnBoard(GameObject initialPosition) {
        Marker initMarker = initialPosition.GetComponent<Marker>();
        _predatorPrefab.transform.position = initialPosition.transform.position;
        initMarker.HasAPiece(true, _predatorPrefab);
        
    }
    private void PlacePreyOnBoard() {
        int count = 0;
        foreach (Transform child in _markerList.transform) {
            _marker = child.GetComponent<Marker>();
            if (count < 16) {
                Vector3 worldPosition = child.position + GameManager.Instance._globalOffset;
                GameObject prey = Instantiate(_preyPrefab, worldPosition, child.transform.rotation);
                if (_marker.gameObject.TryGetComponent(out _renderer)) {
                    _renderer.color = Color.cyan;
                }
                _marker.HasAPiece(true, prey);
                prey.transform.SetParent(transform, true);
                prey.name = "prey (" + count + ")";
                count++;
            }
            if (_marker.gameObject.TryGetComponent(out _renderer)) {
                _renderer.color = Color.cyan;
            }
        }
    }

    private void GetSelectedData(SelectedData selectedData) {
        _selectedData = selectedData;
        //Debug.Log("selected: " + _selectedData.Selected + " , " + _selectedData.Target);
    }



    public void CaptureMoveEffect() {
        _preyCount--;
        if (_preyCount < 1) {
            if (GameManager.Instance != null) {
                GameManager.Instance.IsWinnerPlayer(PlayerTurn.Predator);
                GameManager.Instance.GameOver();
            }
        }
        OnPreyCountChanged?.Invoke(_preyCount);
    }


}
