using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class InputHandler : MonoBehaviour {
    private bool _isSelected;

    private GameObject _selectedPiece;
    private GameObject _selectedMarker;
    private Marker _selectedMarkerNode;

    private GameObject _previousSelected;
    private Marker _previousSelectedMarker;

    private GameObject _targetMarker;
    private Marker _targetMarkerNode;

    private SpriteRenderer _spriteRenderer;

    private SelectedData _selectedData = new SelectedData(null, null);
    Camera _mainCamera;

    public event Action<SelectedData> OnSelected;

    [SerializeField] Marker _startingMarker; 
    [SerializeField] LayerMask markerLayer;
    [SerializeField] LayerMask gamePieceLayer;
    [SerializeField] PlayerController _playerController;
    [SerializeField] GameLogic _gameLogic;

    Color defaultColor = Color.cyan;
    Color defaultColorPrey = Color.white;
    Color selectedColor = Color.green;
    Color defaultColorPredator = Color.white;

    private void Awake() {
        _isSelected = false;
        _previousSelectedMarker = _startingMarker;
        _mainCamera = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context) {
        if (!context.started) return ;
        Vector2 worldPosition = GetMouseWorldPosition();
        RaycastHit2D rayHit = CastRayInLayer(worldPosition, gamePieceLayer);
        if (_previousSelected != null) {
            _gameLogic.CaptureMovePerformed(false);
            _gameLogic.ToggleValidMoveIndicator(false);
            ChangeDefaultColor(_previousSelected);
        }
        HandleSelectedData(worldPosition);
        HandleTargetData(worldPosition);
    }
    private void HandleSelectedData(Vector2 worldPosition) {
        RaycastHit2D rayHit = CastRayInLayer(worldPosition, gamePieceLayer);
        if (rayHit.collider != null) {
                RaycastHit2D hit = CastRayInLayer(worldPosition, markerLayer);
            if (hit.collider != null) {
                _selectedPiece = rayHit.collider.gameObject;
                _selectedMarker = hit.collider.gameObject;
                _selectedMarkerNode = _selectedMarker.GetComponent<Marker>();
                if (!IsValidSelectionForPlayer(_selectedPiece)) {
                    ResetPiece(_selectedMarkerNode);
                    return;
                }

                SelectAPiece(_selectedMarkerNode);
                UpdateSelection(_selectedMarkerNode);
            }
            return;
        }
    }
    private void HandleTargetData(Vector2 worldPosition) {
        RaycastHit2D rayHit = CastRayInLayer(worldPosition, markerLayer);
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
    private bool IsValidSelectionForPlayer(GameObject selectedPiece) {
        PlayerTurn currentTurn = CurrentPlayerTurn();

        if (currentTurn == PlayerTurn.Predator && !selectedPiece.CompareTag("Predator")) {
            return false;
        }
        if (currentTurn == PlayerTurn.Prey && !selectedPiece.CompareTag("Prey")) {
            return false;
        }

        return true;
    }
    private Vector2 GetMouseWorldPosition() {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        return _mainCamera.ScreenToWorldPoint(mousePosition);
    }
    public bool IsSelectedAPiece() => _isSelected;
    public void IsSelectedAPiece(bool isSelected) {
        _isSelected = isSelected;
    }
    private void UpdateSelection(Marker selected) {
        _selectedData = new SelectedData(selected);
        OnSelected?.Invoke(_selectedData);
    }
    private void UpdateSelection(Marker selected, Marker target) {
        _selectedData = new SelectedData(selected, target);
        OnSelected?.Invoke(_selectedData);
    }
    public SelectedData GetSelectedData() {
        return _selectedData.IsBothSelected() ? _selectedData : new SelectedData(null, null);
    }
    private void ResetPiece(Marker pieceMarker) {
        GameObject piece = pieceMarker.GetPieceRef();
        if (piece != null) {
            ChangeDefaultColor(piece);
            IsSelectedAPiece(false);
            _selectedPiece = null;
            _targetMarker = null;
        }
    }

    private RaycastHit2D CastRayInLayer(Vector2 origin, LayerMask layer) {
        return Physics2D.Raycast(origin, Vector2.zero, 1f, layer);
    }
    private void SelectAPiece(Marker gamePieceMarker) {
        GameObject selectedPiece = gamePieceMarker.GetPieceRef();
        if (selectedPiece != null) {
            ChangeDefaultColor(selectedPiece);
            _previousSelected = selectedPiece;
            _previousSelectedMarker = gamePieceMarker;
            ChangeDefaultColor(_previousSelected);
        }
        _selectedPiece = selectedPiece;
        IsSelectedAPiece(true);
        if (_selectedPiece != null) {
            if (_selectedPiece.TryGetComponent(out _spriteRenderer)) {
                ChangeColor(_selectedPiece,selectedColor);
            }
        }
    }
    private PlayerTurn CurrentPlayerTurn() {
        return GameManager.Instance.CurrentPlayerTurn();
    }

    private void ChangeColor(GameObject gameObject, Color color) {
        if (gameObject != null && gameObject.TryGetComponent(out SpriteRenderer spriteRenderer)) {
            spriteRenderer.color = color;
        }
        else {
            Debug.LogWarning("SpriteRenderer is null or GameObject is null. Cannot change color.");
        }
    }
    private void ChangeDefaultColor(GameObject gameObject) {
        if (gameObject == null) return;
        Color defaultColor = gameObject.CompareTag("Prey") ? defaultColorPrey : defaultColorPredator;
        ChangeColor(gameObject, defaultColor);
    }

}

public class SelectedData {
    public Marker Selected { get ; }
    public Marker Target { get; }
    public SelectedData(Marker selected) {
        Selected = selected;
    }
    public SelectedData(Marker selected, Marker target) {
        Selected = selected;
        Target = target;
    }

    public bool IsPieceSelected() => Selected != null;
    public bool IsTargetSelected() => Target != null;
    public bool IsBothSelected() => Selected != null && Target != null;
}
