using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Marker : MonoBehaviour {


    [SerializeField] private List<Marker> markerGraph = new List<Marker>();

    public bool IsConnected(Marker target) {
        return markerGraph.Contains(target);
    }
}
