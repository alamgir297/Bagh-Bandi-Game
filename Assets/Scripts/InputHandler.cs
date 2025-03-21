using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class InputHandler : MonoBehaviour {
    private bool _isSelected;
    private bool _isCaptureMove;
    private GameObject _selectedPiece;
    private GameObject _selectedMarker;
    private GameObject _previousSelected;
    private Marker _previousSelectedMarker;
    private GameObject _targetMarker;
    private SpriteRenderer _spriteRenderer;
    private Marker _selectedMarkerNode;
    private Marker _targetMarkerNode;
    private List<Marker> _validMoves = new();
    private Dictionary<Marker, Marker> _captureMoves = new Dictionary<Marker, Marker>();
    private Dictionary<Marker, Marker> _captureMovesValidate = new Dictionary<Marker, Marker>();
    private SelectedData _selectedData = new SelectedData(null, null);
    Camera _mainCamera;

    public event Action<SelectedData> OnSelected;

    [SerializeField] LayerMask markerLayer;
    [SerializeField] LayerMask gamePieceLayer;
    [SerializeField] PlayerController _playerController;

    Color defaultColor = Color.cyan;
    Color defaultColorPrey = Color.white;
    Color selectedColor = Color.green;
    Color defaultColorPredator = Color.white;

    private void Awake() {
        _isSelected = false;
        _mainCamera = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context) {
        if (!context.started) return ;
        Vector2 worldPosition = GetMouseWorldPosition();
        RaycastHit2D rayHit = CastRayInLayer(worldPosition, gamePieceLayer);
        if (_previousSelected != null) ChangeDefaultColor(_previousSelected);
        if (rayHit.collider != null) {
            RaycastHit2D hit = CastRayInLayer(worldPosition, markerLayer);
            if (hit.collider != null) {
                _selectedPiece = rayHit.collider.gameObject;
                if (CurrentPlayerTurn() == PlayerTurn.Predator) {
                    if (!_selectedPiece.CompareTag("Predator")) {
                        ResetPiece(_previousSelectedMarker);
                        return;
                    }
                }
                if (CurrentPlayerTurn() == PlayerTurn.Prey) {
                    if (!_selectedPiece.CompareTag("Prey")) {
                        ResetPiece(_previousSelectedMarker);
                        return;
                    }
                }
                _selectedMarker = hit.collider.gameObject;
                _selectedMarkerNode = _selectedMarker.GetComponent<Marker>();
                SelectAPiece(_selectedMarkerNode);
                GenerateValidMoves();
                if(CurrentPlayerTurn()== PlayerTurn.Predator) {
                    _captureMoves= GetCaptureMove(_selectedMarkerNode);
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
                    _targetMarkerNode = _targetMarker.GetComponent<Marker>();
                    UpdateSelection(_selectedMarkerNode, _targetMarkerNode);
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
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        return _mainCamera.ScreenToWorldPoint(mousePosition);
    }
    public bool IsSelectedAPiece() => _isSelected;

    private void UpdateSelection(Marker selected, Marker target) {
        _selectedData = new SelectedData(selected, target);
        OnSelected?.Invoke(_selectedData);
    }
    public SelectedData GetSelectedData() {
        return _selectedData.IsValidSelection() ? _selectedData : new SelectedData(null, null);
    }

    private void ResetPiece(Marker pieceMarker) {
        GameObject piece = pieceMarker.GetPieceRef();
        if (piece != null) {
            ChangeDefaultColor(piece);
            if (_validMoves.Count >= 0) {
            }
            ToggleValidMoveIndicator(false);
            _isSelected = false;
            _selectedPiece = null;
            _targetMarker = null;
        }
    }

    private RaycastHit2D CastRayInLayer(Vector2 origin, LayerMask layer) {
        return Physics2D.Raycast(origin, Vector2.zero, 1f, layer);
    }

    private RaycastHit2D[] CastRayInDirectionAll(Vector2 origin, Vector2 direction, float distance, LayerMask layer) {
        Debug.DrawRay(origin, direction * distance, Color.red, 0.5f); // Visualize the ray
        return Physics2D.RaycastAll(origin, direction, distance, layer);
    }
    private void SelectAPiece(Marker gamePieceMarker) {
        GameObject selectedPiece = gamePieceMarker.GetPieceRef();
        if (selectedPiece != null) {
            ChangeDefaultColor(selectedPiece);
            _previousSelected = selectedPiece;
            _previousSelectedMarker = gamePieceMarker;
            ChangeDefaultColor(_previousSelected);
            ToggleValidMoveIndicator(false);
        }
        _selectedPiece = selectedPiece;
        _isSelected = true;
        if (_selectedPiece != null) {
            if (_selectedPiece.TryGetComponent(out _spriteRenderer)) {
                ChangeColorOnSelected(selectedColor);
            }
        }
    }


    public void PreyMove(Marker selectedPieceMarker, Marker targetMarker) {
        if (selectedPieceMarker != null && targetMarker != null) {
            if (_validMoves.Contains(targetMarker)) {
                GameObject selectedPiece = selectedPieceMarker.GetPieceRef();
                if (selectedPiece != null) {
                    MoveAPiece(selectedPieceMarker, targetMarker);
                    GameManager.Instance.ChangePlayerTurn();
                }
            }
            else {
                Debug.Log("plz select a valid marker");
                return;
            }

            // Reset color to default when the piece is moved or invalid move
            ResetPiece(selectedPieceMarker);
        }
    }
    public void PredatorMove(Marker selectedMarker, Marker targetMarker) {
        if (selectedMarker != null && targetMarker != null) {
            if (_validMoves.Contains(targetMarker)) {
                MoveAPiece(selectedMarker,targetMarker);
                if (_captureMoves.ContainsKey(targetMarker)) {
                    _playerController.CaptureMove();
                    Marker pieceToBeRemovedMarker = _captureMoves[targetMarker];
                    GameObject pieceToBeRemoved = pieceToBeRemovedMarker.GetPieceRef();
                    if (pieceToBeRemoved != null) {
                        pieceToBeRemovedMarker.HasAPiece(false, null);
                        Destroy(pieceToBeRemoved);
                    }
                    _captureMovesValidate = GetCaptureMove(targetMarker);
                    if (_captureMovesValidate.Count > 0) {
                        IsCaptureMove(true);
                    }
                    _captureMovesValidate.Clear();
                }
                if (!IsCaptureMove())
                    GameManager.Instance.ChangePlayerTurn();
                _captureMoves.Clear();
            }
            else {
                Debug.Log("plz select a valid marker");
                return;
            }

            // Reset color to default when the piece is moved or invalid move
            ResetPiece(selectedMarker);
            IsCaptureMove(false);
        }
    }

    public void MoveAPiece(Marker selectedMarker, Marker targetMarker) {
        GameObject selectedPiece = selectedMarker.GetPieceRef();
        if (selectedPiece != null) {
            selectedPiece.transform.position = targetMarker.gameObject.transform.position + GameManager.Instance._globalOffset;
            selectedMarker.HasAPiece(false, null);
            targetMarker.HasAPiece(true, selectedPiece);
            ChangeDefaultColor(selectedPiece);
            Debug.Log("Move successful");
        }
    }

    public Dictionary<Marker,Marker> GetCaptureMove(Marker selectedMarker) {
        Dictionary<Marker, Marker> tempCaptureMoves = new Dictionary<Marker, Marker>();
        foreach (Marker target in selectedMarker.GetCaptureList()) {
            Vector2 direction = CalculateDirection(selectedMarker.transform.position, target.gameObject.transform.position);
            RaycastHit2D[] rayHit = CastRayInDirectionAll(target.gameObject.transform.position, direction, 2f, markerLayer);
            if (rayHit.Length > 1) {
                Marker preyMarker = rayHit[0].collider.GetComponent<Marker>();
                Marker landingMarker = rayHit[1].collider.GetComponent<Marker>();

                if (landingMarker.gameObject != null && !landingMarker.HasAPiece()) {
                    AddToMoveList(landingMarker);
                    tempCaptureMoves[landingMarker] = preyMarker;
                }
            }
        }
        return tempCaptureMoves;
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

}

public class SelectedData {
    public Marker Selected { get ; }
    public Marker Target { get; }
    public SelectedData(Marker selected, Marker target) {
        Selected = selected;
        Target = target;
    }
    public bool IsValidSelection() => Selected != null && Target != null;
    public void ResetSelection() {
    }
}
