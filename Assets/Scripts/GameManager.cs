using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;


    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

public enum PlayerTurn {
    Prey,
    Predator
}
