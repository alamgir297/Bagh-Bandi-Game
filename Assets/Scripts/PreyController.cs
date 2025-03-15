using UnityEngine;

public class PreyController : MonoBehaviour {

    [SerializeField] private GameObject _bakriPrefab;
    [SerializeField] private GameObject _markerList;
    SpriteRenderer renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        renderer = _bakriPrefab.GetComponent<SpriteRenderer>();
        PlaceBakriOnBoard();
    }

    // Update is called once per frame
    void Update() {

    }

    private void PlaceBakriOnBoard() {
        int count = 0;
        foreach(Transform child in _markerList.transform) {
            if (count < 16) {
                Vector3 worldPosition = child.position;
                GameObject bakri = Instantiate(_bakriPrefab, worldPosition, child.transform.rotation);
                bakri.transform.SetParent(transform, true);
                bakri.name = "bakri " + count;
                count++;
            }
        }
    }
    void SetColor(Color color) {
        renderer.color = color;
    }
}
