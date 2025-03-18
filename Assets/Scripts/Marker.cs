using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Marker : MonoBehaviour {

    public bool _hasAPrey;
    public GameObject _preyRef;
    public bool _hasPredator;
    public GameObject _predatorRef;
    private InputHandler _inputHandler;
    private PredatorController _predator;
    private SpriteRenderer _renderer;
    [SerializeField] private List<Marker> markerGraph = new List<Marker>();

    private void Awake() {
        _inputHandler = GameObject.Find("InputHandler").GetComponent<InputHandler>();
        _predator = GameObject.Find("Predator").GetComponent<PredatorController>();
    }

    public bool IsConnected(Marker target) {
        return markerGraph.Contains(target);
    }

    public bool HasAPrey() => _hasAPrey;
    public void HasAPrey(bool hasAPrey, GameObject preyRef) {
        _hasAPrey = hasAPrey;
        _preyRef = preyRef;
    }
    public GameObject GetPreyRef() {
        if (HasAPrey()) {
            return _preyRef;
        }
        return null;
    }

    //public bool HasAPredator() => _hasPredator;
    //public void HasAPredator(bool hasPredator, GameObject predatorRef) {
    //    _hasPredator = hasPredator;
    //    _predatorRef = predatorRef;
    //}
    //public GameObject GetPredatorRef() {
    //    if (HasAPrey()) {
    //        return _preyRef;
    //    }
    //    return null;
    //}

    public List<Marker> GetCaptureList() {
        List<Marker> temp = new();
        foreach (Marker target in markerGraph) {
            if (IsConnected(target) && target.HasAPrey()) {
                temp.Add(target);
            }
        }
        return temp;
    }

    public List<Marker> GetValidList() {
        List<Marker> temp = new();
        foreach(Marker target in markerGraph) {
            if (IsConnected(target) && !target.HasAPrey()) {
                temp.Add(target);
            }
        }
        return temp;
    }

}
