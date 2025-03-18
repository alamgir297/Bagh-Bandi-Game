using UnityEngine;

public class PreyController : MonoBehaviour {
    private Marker _marker;

    [SerializeField] private GameObject _preyPrefab;
    [SerializeField] private GameObject _markerList;
    private SpriteRenderer _renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        PlacePreyOnBoard();
    }

    // Update is called once per frame
    void Update() {

    }

    private void PlacePreyOnBoard() {
        int count = 0;
        foreach (Transform child in _markerList.transform) {
            _marker = child.GetComponent<Marker>();
            if (count < 16) {
                Vector3 worldPosition = child.position;
                GameObject prey = Instantiate(_preyPrefab, worldPosition, child.transform.rotation);
                if (_marker.gameObject.TryGetComponent(out _renderer)) {
                    _renderer.color = Color.cyan;
                }
                _marker.HasAPrey(true, prey);
                prey.transform.SetParent(transform, true);
                prey.name = "prey (" + count + ")";
                //_marker.SetPreyRef(prey);
                count++;
            }
            if (_marker.gameObject.TryGetComponent(out _renderer)) {
                _renderer.color = Color.cyan;
            }
        }
    }
}
