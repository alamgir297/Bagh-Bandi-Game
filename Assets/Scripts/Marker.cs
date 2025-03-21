using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Marker : MonoBehaviour {

    public bool _hasPiece;
    public GameObject _pieceRef;
    private InputHandler _inputHandler;
    private PlayerController _predator;
    private SpriteRenderer _renderer;
    [SerializeField] private List<Marker> markerGraph = new List<Marker>();

    private void Awake() {
        _inputHandler = GameObject.Find("InputHandler").GetComponent<InputHandler>();
        _predator = GameObject.Find("Predator").GetComponent<PlayerController>();
    }

    public bool IsConnected(Marker target) {
        return markerGraph.Contains(target);
    }

    public bool HasAPiece() => _hasPiece;
    public void HasAPiece(bool hasPiece, GameObject pieceRef) {
        _hasPiece = hasPiece;
        _pieceRef = pieceRef;
    }
    public GameObject GetPieceRef() {
        if (HasAPiece()) {
            return _pieceRef;
        }
        return null;
    }

    public List<Marker> GetCaptureList() {
        List<Marker> temp = new();
        foreach (Marker target in markerGraph) {
            if (IsConnected(target) && target.HasAPiece()) {
                temp.Add(target);
            }
        }
        return temp;
    }

    public List<Marker> GetValidList() {
        List<Marker> temp = new();
        foreach(Marker target in markerGraph) {
            if (IsConnected(target) && !target.HasAPiece()) {
                temp.Add(target);
            }
        }
        return temp;
    }

}
