using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputHandler : MonoBehaviour {
    private bool _isSelected;
    private bool _isCaptureMovePossible;
    private GameObject _selectedPiece;
    private GameObject _selectedMarker;
    private GameObject _previousSelected;
    private GameObject _targerMarker;
    private SpriteRenderer _spriteRenderer;
    private Marker _selectedMarkerConnection;
    private Marker _targetMarkerConnection;
    private List<Marker> _validMoves= new();
    private Dictionary<Marker, Marker> _captureMoves = new Dictionary<Marker, Marker>();
    Camera _mainCamera;
    [SerializeField] LayerMask markerLayer;
    [SerializeField] LayerMask gamePieceLayer;

    Color defaultColor = Color.cyan;
    Color defaultColorPrey = Color.blue;
    Color selectedColor = Color.green;
    Color defaultColorPredator = Color.red;

    private void Awake() {
        _isSelected = false;
        _mainCamera = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context) {
        if (!context.started) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
        RaycastHit2D rayHit = CastRayInLayer(worldPosition, gamePieceLayer);

        if (rayHit.collider != null) {
            RaycastHit2D hit = CastRayInLayer(worldPosition, markerLayer);
            if (hit.collider != null) {
                _selectedMarker = hit.collider.gameObject;
                _selectedMarkerConnection = _selectedMarker.GetComponent<Marker>();
                SelectAPiece(rayHit.collider.gameObject);
                GenerateValidMoves();
                PreyCaptureMove();
                ToggleValidMoveIndicator(true);
            }
            return;
        }
        else {
            rayHit = CastRayInLayer(worldPosition, markerLayer);
            if (rayHit.collider != null) {
                if (_isSelected) {
                    _targerMarker = rayHit.collider.gameObject;
                    return;
                }

                Debug.Log("select a valid cell");
            }
            else {

                Debug.Log("Not a valid click");
            }
        }
    }
    //public void OnClickNew(InputAction.CallbackContext context) {
    //    if (!context.started) return;

    //    Vector2 mousePosition = Mouse.current.position.ReadValue();
    //    Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
    //    RaycastHit2D rayHit = CastRayInLayer(worldPosition, markerLayer);

    //    if (rayHit.collider != null) {
    //            _selectedPiece= 
    //            _selectedMarker = rayHit.collider.gameObject;
    //            _selectedMarkerConnection = _selectedMarker.GetComponent<Marker>();
    //            SelectAPiece(rayHit.collider.gameObject);
    //            GenerateValidMoves();
    //            PreyCaptureMove();
    //            ToggleValidMoveIndicator(true);
    //        return;
    //    }
    //    else {
    //        rayHit = CastRayInLayer(worldPosition, markerLayer);
    //        if (rayHit.collider != null) {
    //            if (_isSelected) {
    //                _targerMarker = rayHit.collider.gameObject;
    //                return;
    //            }

    //            Debug.Log("select a valid cell");
    //        }
    //        else {

    //            Debug.Log("Not a valid click");
    //        }
    //    }
    //}
    public bool IsSelectedAPiece() => _isSelected;
    
    
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
            ToggleValidMoveIndicator(false);
            if (_validMoves.Count == 0) {
                Debug.Log("list is empty ");
            }
        }
        _selectedPiece = gamePiece;
        _isSelected = true;
        if (_selectedPiece.TryGetComponent(out _spriteRenderer)) {
            ChangeColorOnSelected(selectedColor);
        }
    }


    public void NormalMove() {
        if (_selectedPiece != null && _targerMarker != null) {
            _targetMarkerConnection = _targerMarker.GetComponent<Marker>();
            if (_validMoves.Contains(_targetMarkerConnection)) {
                MoveAPiece(_targerMarker);
                if (_captureMoves.ContainsKey(_targetMarkerConnection)) {
                    Marker pieceToBeRemovedMarker = _captureMoves[_targetMarkerConnection];
                    GameObject pieceToBeRemoved = pieceToBeRemovedMarker.GetPreyRef();
                    if (pieceToBeRemoved != null) {
                        pieceToBeRemovedMarker.HasAPrey(false,null);
                        Destroy(pieceToBeRemoved);
                    }
                    _targetMarkerConnection.HasAPrey(false, null);
                    _captureMoves.Clear();
                }
            }
            else {
                Debug.Log("plz select a valid marker");
            }

            // Reset color to default when the piece is moved or invalid move
            ToggleValidMoveIndicator(false);
            ChangeDefaultColor(_selectedPiece);
            _isSelected = false;
            _selectedPiece = null;
            _targerMarker = null;
        }
    }

    public void PreyCaptureMove() {
        foreach (Marker target in _selectedMarkerConnection.GetCaptureList()) {
            Vector2 direction = CalculateDirection(_selectedMarker.transform.position, target.gameObject.transform.position);
            RaycastHit2D[] rayHit = CastRayInDirectionAll(target.gameObject.transform.position, direction, 2f, markerLayer);
            if (rayHit.Length > 1) {
                Marker preyMarker = rayHit[0].collider.GetComponent<Marker>();
                Marker landingMarker = rayHit[1].collider.GetComponent<Marker>();

                if (landingMarker.gameObject != null && !landingMarker.HasAPrey()) {
                    AddToMoveList(landingMarker);
                    _captureMoves[landingMarker] = preyMarker;
                    Debug.Log("Valid capture move found at: " + landingMarker.gameObject.name);
                }
            }
        }
    }

    private void GenerateValidMoves() {
        _validMoves.Clear();
        _captureMoves.Clear();
        _validMoves = _selectedMarkerConnection.GetValidList();
        ToggleValidMoveIndicator(true);
    }
    private void AddToMoveList(Marker marker) {
        _validMoves.Add(marker);
    }


    private void ToggleValidMoveIndicator(bool isOn) {
        foreach (Marker target in _validMoves) {
            if (target.gameObject.TryGetComponent(out _spriteRenderer)) {
                if(isOn)
                    _spriteRenderer.color = Color.red;
                else _spriteRenderer.color = defaultColor;
            }
        }
    }

    private Vector2 CalculateDirection(Vector2 source, Vector2 target) {
        return (target - source).normalized;
    }

    public void MoveAPiece(GameObject target) {
        _selectedPiece.transform.position = target.transform.position;
        _selectedMarkerConnection.HasAPrey(false,null);
        _targetMarkerConnection.HasAPrey(true, _selectedPiece);
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
        if(gameObject.TryGetComponent(out _spriteRenderer)) {
        if (_selectedPiece.CompareTag("Prey")) {
            ChangeColorOnSelected(defaultColorPrey);
        }
        else ChangeColorOnSelected(defaultColorPredator);
        }
    }

    private class SelectedData {
        private GameObject _selectedPiece;
        private Marker _selectedPieceMarker;
        private Marker _targetPieceMarker;
    }
}
