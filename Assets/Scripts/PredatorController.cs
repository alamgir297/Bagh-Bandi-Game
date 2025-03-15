using UnityEngine;

public class PredatorController : MonoBehaviour {
    private InputHandler inputHandler;

    [SerializeField] private GameObject _predatorInitialPositioMarker;
    void Start() {
        inputHandler = GameObject.Find("InputHandler").GetComponent<InputHandler>();
        transform.position = _predatorInitialPositioMarker.transform.position;
    }

    void Update() {
        if (inputHandler.IsSelectedAPiece()) {
            inputHandler.MoveAPiece();
        }
    }
}
