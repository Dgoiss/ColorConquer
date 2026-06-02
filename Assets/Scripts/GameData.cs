using UnityEngine;

public class GameData : MonoBehaviour {
    public static GameData instance;

    // Guarda a cor escolhida pelo jogador (Padrão: Vermelho)
    public Color playerColor = Color.red; 
    public string playerColorName = "Vermelho";

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // Impede que este objeto suma ao mudar de cena
        } else {
            Destroy(gameObject);
        }
    }
}