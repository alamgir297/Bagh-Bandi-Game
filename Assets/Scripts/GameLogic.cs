using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
    private bool _isCaptureMove;
    private bool _captureMovePerformed;
    private SpriteRenderer _spriteRenderer;

    private List<Marker> _validMoves = new();
    private Dictionary<Marker, Marker> _captureMoves = new Dictionary<Marker, Marker>();
    private Dictionary<Marker, Marker> _captureMovesValidate = new Dictionary<Marker, Marker>();

    [SerializeField] LayerMask markerLayer;
    [SerializeField] LayerMask gamePieceLayer;

    public List<Marker> GenerateValidMoves(Marker selected) {
        return selected.GetValidList();
    }

    public void ShowValidMovesForThisPiece(Marker selected) {
        ToggleValidMoveIndicator(false);
        _validMoves = GenerateValidMoves(selected);
        if (CurrentPlayerTurn() == PlayerTurn.Predator) {
            _captureMoves= GetCaptureMove(selected);
            if (_validMoves.Count == 0) {
                GameManager.Instance.IsWinnerPlayer(PlayerTurn.Prey);
                GameManager.Instance.GameOver();
            }
        }

        ToggleValidMoveIndicator(true);
    }

    public Dictionary<Marker, Marker> GetCaptureMove(Marker selectedMarker) {
        Dictionary<Marker, Marker> tempCaptureMoves = new Dictionary<Marker, Marker>();
        foreach (Marker target in selectedMarker.GetCaptureList()) {
            Vector2 direction = CalculateDirection(selectedMarker.transform.position, target.gameObject.transform.position);
            RaycastHit2D[] rayHit = CastRayInDirectionAll(target.gameObject.transform.position, direction, 2f, markerLayer);
            if (rayHit.Length > 1) {
                Debug.Log("we are looking for shotruj");
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
            ToggleValidMoveIndicator(false);
        }
    }

    public void PredatorMove(Marker selectedMarker, Marker targetMarker) {
        if (selectedMarker != null && targetMarker != null) {
            if (_validMoves.Contains(targetMarker)) {
                MoveAPiece(selectedMarker, targetMarker);
                if (_captureMoves.ContainsKey(targetMarker)) {
                    CaptureMovePerformed(true);
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
                else CaptureMovePerformed(false);
                if (!IsCaptureMove()) {
                    GameManager.Instance.ChangePlayerTurn();
                }
                _captureMoves.Clear();
            }
            else {
                Debug.Log("plz select a valid marker");
                return;
            }

            // Reset color to default when the piece is moved or invalid move
            ToggleValidMoveIndicator(false);
            IsCaptureMove(false);
        }
    }

    public void MoveAPiece(Marker selectedMarker, Marker targetMarker) {
        GameObject selectedPiece = selectedMarker.GetPieceRef();
        if (selectedPiece != null) {
            selectedPiece.transform.position = targetMarker.gameObject.transform.position + GameManager.Instance._globalOffset;
            selectedMarker.HasAPiece(false, null);
            targetMarker.HasAPiece(true, selectedPiece);
            //ChangeDefaultColor(selectedPiece);
            Debug.Log("Move successful");
        }
    }

    private bool IsCaptureMove() => _isCaptureMove;
    private void IsCaptureMove(bool isCaptureMove) {
        _isCaptureMove = isCaptureMove;
    }
    public bool CaptureMovePerformed() => _captureMovePerformed;
    public void CaptureMovePerformed(bool isPerformed) {
        _captureMovePerformed = isPerformed;
    }
    private Vector2 CalculateDirection(Vector2 source, Vector2 target) {
        return (target - source).normalized;
    }
    private RaycastHit2D[] CastRayInDirectionAll(Vector2 origin, Vector2 direction, float distance, LayerMask layer) {
        Debug.DrawRay(origin, direction * distance, Color.red, 0.5f); // Visualize the ray
        return Physics2D.RaycastAll(origin, direction, distance, layer);
    }

    public void PrintValidMoves(List<Marker> list) {
        if (list.Count != 0) {
            foreach(Marker child in list) {
                Debug.Log("targets: " + child.name);
            }
        }
    }
    private void AddToMoveList(Marker marker) {
        _validMoves.Add(marker);
    }
    private PlayerTurn CurrentPlayerTurn() {
        return GameManager.Instance.CurrentPlayerTurn();
    }

    public void ToggleValidMoveIndicator(bool isOn) {
        foreach (Marker target in _validMoves) {
            if (target.gameObject.TryGetComponent(out _spriteRenderer)) {
                if (isOn)
                    _spriteRenderer.color = Color.red;
                else _spriteRenderer.color = Color.cyan;
            }
        }
    }

}
