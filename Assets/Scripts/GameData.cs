using UnityEngine;

public class GameData : MonoBehaviour {
    public static GameData instance;

    // Cor escolhida pelo jogador
    public Color playerColor = Color.red; 
    public string playerColorName = "Azul";

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantém o objeto ao trocar de cena
        } else {
            Destroy(gameObject);
        }
    }
}