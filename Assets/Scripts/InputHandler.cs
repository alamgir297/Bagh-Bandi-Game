using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputHandler : MonoBehaviour {
    private bool _isSelected;
    private bool _isCaptureMove;
    private GameObject _selectedPiece;
    private GameObject _selectedMarker;
    private GameObject _previousSelected;
    private GameObject _targetMarker;
    private SpriteRenderer _spriteRenderer;
    private Marker _selectedMarkerNode;
    private Marker _targetMarkerNode;
    private List<Marker> _validMoves = new();
    private Dictionary<Marker, Marker> _captureMoves = new Dictionary<Marker, Marker>();
    Camera _mainCamera;

    [SerializeField] LayerMask markerLayer;
    [SerializeField] LayerMask gamePieceLayer;
    [SerializeField] PredatorController _playerController;

    Color defaultColor = Color.cyan;
    Color defaultColorPrey = Color.white;
    Color selectedColor = Color.green;
    Color defaultColorPredator = Color.white;

    private void Awake() {
        _isSelected = false;
        _mainCamera = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context) {
        if (!context.started) return;

        Vector2 worldPosition = GetMouseWorldPosition();
        RaycastHit2D rayHit = CastRayInLayer(worldPosition, gamePieceLayer);
        if (_previousSelected != null) ChangeDefaultColor(_previousSelected);
        if (rayHit.collider != null) {
            RaycastHit2D hit = CastRayInLayer(worldPosition, markerLayer);
            if (hit.collider != null) {
                _selectedPiece = rayHit.collider.gameObject;
                if (CurrentPlayerTurn() == PlayerTurn.Predator) {
                    if (!_selectedPiece.CompareTag("Predator")) {
                        ResetPiece(_previousSelected);
                        return;
                    }
                }
                if (CurrentPlayerTurn() == PlayerTurn.Prey) {
                    if (!_selectedPiece.CompareTag("Prey")) {
                        ResetPiece(_previousSelected);
                        return;
                    }
                }
                _selectedMarker = hit.collider.gameObject;
                _selectedMarkerNode = _selectedMarker.GetComponent<Marker>();
                SelectAPiece(rayHit.collider.gameObject);
                GenerateValidMoves();
                if(CurrentPlayerTurn()== PlayerTurn.Predator) {
                    GetCaptureMove();
                    if (_validMoves.Count == 0) {
                        GameManager.Instance.IsWinnerPlayer(PlayerTurn.Prey);
                        GameManager.Instance.IsGameOver(true);
                    }
                }
                ToggleValidMoveIndicator(true);
            }
            return;
        }
        else {
            rayHit = CastRayInLayer(worldPosition, markerLayer);
            if (rayHit.collider != null) {
                if (_isSelected) {
                    _targetMarker = rayHit.collider.gameObject;
                    return;
                }

                Debug.Log("select a valid cell");
            }
            else {

                Debug.Log("Not a valid click");
            }
        }
    }
    private Vector2 GetMouseWorldPosition() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return _mainCamera.ScreenToWorldPoint(mousePosition);
    }
    public bool IsSelectedAPiece() => _isSelected;

    private void ResetPiece(GameObject gameObject) {
        ChangeDefaultColor(gameObject);
        ToggleValidMoveIndicator(false);
        _isSelected = false;
        _selectedPiece = null;
        _targetMarker = null;
    }

    private RaycastHit2D CastRayInLayer(Vector2 origin, LayerMask layer) {
        return Physics2D.Raycast(origin, Vector2.zero, 1f, layer);
    }

    private RaycastHit2D[] CastRayInDirectionAll(Vector2 origin, Vector2 direction, float distance, LayerMask layer) {
        Debug.DrawRay(origin, direction * distance, Color.red, 0.5f); // Visualize the ray
        return Physics2D.RaycastAll(origin, direction, distance, layer);
    }
    private void SelectAPiece(GameObject gamePiece) {
        if (_selectedPiece != null) {
            ChangeDefaultColor(_selectedPiece);
            _previousSelected = _selectedPiece;
            ChangeDefaultColor(_previousSelected);
            ToggleValidMoveIndicator(false);
            if (_validMoves.Count == 0) {
                Debug.Log("list is empty ");
            }
        }
        _selectedPiece = gamePiece;
        _isSelected = true;
        if (_selectedPiece != null) {
            if (_selectedPiece.TryGetComponent(out _spriteRenderer)) {
                ChangeColorOnSelected(selectedColor);
            }
        }
    }


    public void PreyMove() {
        if (_selectedPiece != null && _targetMarker != null) {
            _targetMarkerNode = _targetMarker.GetComponent<Marker>();
            if (_validMoves.Contains(_targetMarkerNode)) {
                MoveAPiece(_targetMarker);
                GameManager.Instance.ChangePlayerTurn();
                //Debug.Log("move by: " + PlayerTurn.Prey);
            }
            else {
                Debug.Log("plz select a valid marker");
                return;
            }

            // Reset color to default when the piece is moved or invalid move
            ResetPiece(_selectedMarker);
        }
    }
    public void PredatorMove() {
        if (_selectedPiece != null && _targetMarker != null) {
            _targetMarkerNode = _targetMarker.GetComponent<Marker>();
            if (_validMoves.Contains(_targetMarkerNode)) {
                MoveAPiece(_targetMarker);
                Debug.Log("move by: " + PlayerTurn.Predator);
                if (_captureMoves.ContainsKey(_targetMarkerNode)) {
                    IsCaptureMove(true);
                    _playerController.CaptureMove();
                    Marker pieceToBeRemovedMarker = _captureMoves[_targetMarkerNode];
                    GameObject pieceToBeRemoved = pieceToBeRemovedMarker.GetPieceRef();
                    if (pieceToBeRemoved != null) {
                        pieceToBeRemovedMarker.HasAPiece(false, null);
                        Destroy(pieceToBeRemoved);
                    }
                }
                if (!IsCaptureMove())
                    GameManager.Instance.ChangePlayerTurn();
                _targetMarkerNode.HasAPiece(false, null);
                _captureMoves.Clear();
            }
            else {
                Debug.Log("plz select a valid marker");
                return;
            }

            // Reset color to default when the piece is moved or invalid move
            ResetPiece(_selectedMarker);
            IsCaptureMove(false);
        }
    }

    public void GetCaptureMove() {
        foreach (Marker target in _selectedMarkerNode.GetCaptureList()) {
            Vector2 direction = CalculateDirection(_selectedMarker.transform.position, target.gameObject.transform.position);
            RaycastHit2D[] rayHit = CastRayInDirectionAll(target.gameObject.transform.position, direction, 2f, markerLayer);
            if (rayHit.Length > 1) {
                Marker preyMarker = rayHit[0].collider.GetComponent<Marker>();
                Marker landingMarker = rayHit[1].collider.GetComponent<Marker>();

                if (landingMarker.gameObject != null && !landingMarker.HasAPiece()) {
                    AddToMoveList(landingMarker);
                    _captureMoves[landingMarker] = preyMarker;
                }
            }
        }
    }
    private bool IsCaptureMove() => _isCaptureMove;
    private void IsCaptureMove(bool isCaptureMove) {
        _isCaptureMove = isCaptureMove;
    }

    private PlayerTurn CurrentPlayerTurn() {
        return GameManager.Instance.CurrentPlayerTurn();
    }

    private void GenerateValidMoves() {
        _validMoves.Clear();
        _captureMoves.Clear();
        _validMoves = _selectedMarkerNode.GetValidList();
        ToggleValidMoveIndicator(true);
    }
    private void AddToMoveList(Marker marker) {
        _validMoves.Add(marker);
    }


    private void ToggleValidMoveIndicator(bool isOn) {
        foreach (Marker target in _validMoves) {
            if (target.gameObject.TryGetComponent(out _spriteRenderer)) {
                if (isOn)
                    _spriteRenderer.color = Color.red;
                else _spriteRenderer.color = defaultColor;
            }
        }
    }

    private Vector2 CalculateDirection(Vector2 source, Vector2 target) {
        return (target - source).normalized;
    }

    public void MoveAPiece(GameObject target) {
        _selectedPiece.transform.position = target.transform.position+GameManager.Instance._globalOffset;
        _selectedMarkerNode.HasAPiece(false, null);
        _targetMarkerNode.HasAPiece(true, _selectedPiece);
        ChangeDefaultColor(_selectedPiece);
        Debug.Log("Move successful");
    }

    private void ChangeColorOnSelected(Color color) {
        if (_spriteRenderer != null) {
            _spriteRenderer.color = color;
        }
        else {
            Debug.LogWarning("SpriteRenderer is null. Cannot change color.");
        }
    }
    private void ChangeDefaultColor(GameObject gameObject) {
        if (gameObject != null) {
            if (gameObject.TryGetComponent(out _spriteRenderer)) {
                if (gameObject.CompareTag("Prey")) {
                    ChangeColorOnSelected(defaultColorPrey);
                }
                else ChangeColorOnSelected(defaultColorPredator);
            }
        }
    }

    private class SelectedData {
        private GameObject _selectedPiece;
        private Marker _selectedPieceMarker;
        private Marker _targetPieceMarker;
    }
}