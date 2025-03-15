using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour {
    private bool _isSelected;
    private GameObject _selectedPiece;
    private GameObject _previousSelected;
    private GameObject _moveMarker;
    private SpriteRenderer _spriteRenderer;
  
    
    Camera _mainCamera;
    [SerializeField] LayerMask markerLayer;
    [SerializeField] LayerMask gamePieceLayer;

    Color defaultColor = Color.blue;
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
        RaycastHit2D rayHit= CastRayInLayer(worldPosition, gamePieceLayer);

        if (rayHit.collider != null) {
            Debug.Log("Clicked on: " + rayHit.collider.gameObject.name);
            SelectAPiece(rayHit.collider.gameObject);
            return;
        }
        else {
            rayHit = CastRayInLayer(worldPosition, markerLayer);
            if (rayHit.collider != null) {
                if (_isSelected) {
                    Debug.Log("Clicked on: " + rayHit.collider.gameObject.name);
                    _moveMarker = rayHit.collider.gameObject;
                    return;
                }

                Debug.Log("Can't select an empty cell");
            }
            else {

                Debug.Log("No hit");
            }
        }
    }

    private RaycastHit2D CastRayInLayer(Vector2 position ,LayerMask layer) {
        return Physics2D.Raycast(position, Vector2.zero, 1f, layer);
    }
    private void SelectAPiece(GameObject gamePiece) {
        if (_selectedPiece != null) {
            if(_selectedPiece.TryGetComponent(out _spriteRenderer)) {
                ChangeDefaultColor();
            }
            _previousSelected = _selectedPiece;
        }
        _selectedPiece = gamePiece;
        _isSelected = true;

        if(_selectedPiece.TryGetComponent(out _spriteRenderer)) {
            ChangeColorOnSelected(selectedColor);
        }
    }

    public bool IsSelectedAPiece() => _isSelected;

    public void MoveAPiece() {
        if (_selectedPiece != null && _moveMarker != null) {
            _selectedPiece.transform.position = _moveMarker.transform.position;

            // Reset color to default when the piece is moved
            ChangeDefaultColor();
            _isSelected = false;
            _selectedPiece = null;
            _moveMarker = null;
        }
    }

    private void ChangeColorOnSelected(Color color) {
        if (_spriteRenderer != null) {
            _spriteRenderer.color = color;
        }
        else {
            Debug.LogWarning("SpriteRenderer is null. Cannot change color.");
        }
    }
    private void ChangeDefaultColor() {
        if (_selectedPiece.CompareTag("Prey")) {
            ChangeColorOnSelected(defaultColorPrey);
        }
        else ChangeColorOnSelected(defaultColorPredator);
    }
}

public enum PlayerType {
    Prey,
    Predator,
    Selected
}