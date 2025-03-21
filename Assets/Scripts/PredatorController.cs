using UnityEngine;
using System;

public class PredatorController : MonoBehaviour {
    private int _preyCount = 16;
    private InputHandler inputHandler;
    private Marker _marker;

    public event Action<int> OnPreyCountChanged;

    [SerializeField] private GameObject _predatorInitialPositioMarker;
    void Start() {
        inputHandler = GameObject.Find("InputHandler").GetComponent<InputHandler>();
        transform.position = _predatorInitialPositioMarker.transform.position;
    }

    void Update() {
        if (inputHandler.IsSelectedAPiece()) {
            if (GameManager.Instance.CurrentPlayerTurn() == PlayerTurn.Predator) {
                //inputHandler.PredatorMove();
            }
            else {
                //inputHandler.PreyMove();
            }

        }
    }


    public void CaptureMove() {
        _preyCount--;
        if (_preyCount < 1) {
            if (GameManager.Instance != null) {
                GameManager.Instance.IsWinnerPlayer(PlayerTurn.Predator);
                GameManager.Instance.IsGameOver(true);
            }
        }
        OnPreyCountChanged?.Invoke(_preyCount);
    }


}
