//Controla a máquina de estados do jogo (Turno do Jogador, Turno da IA, Verificação de Vitória).
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public string currentTurn = "Player";

    void Awake() {
        if (instance == null) instance = this;
    }

    private Territory selectedOrigin;

    public void SelectTerritory(Territory t) {
        if (selectedOrigin == null) {
            selectedOrigin = t;
            Debug.Log("Origem selecionada: " + t.name);
        } else {
            Debug.Log("Destino selecionado: " + t.name);
            Battle(selectedOrigin, t);
            selectedOrigin = null;
        }
    }


    public void EndTurn() {
        currentTurn = (currentTurn == "Player") ? "AI" : "Player";
        Debug.Log("Turno atual: " + currentTurn);
    }

    public void Battle(Territory attacker, Territory defender) {
        int result = (attacker.troops - defender.troops) + Random.Range(1, 6);
        if (result > 0) {
            defender.ChangeOwner(attacker.owner, Color.red); // Exemplo: cor vermelha para Player
            Debug.Log("Território conquistado!");
        } else {
            Debug.Log("Defesa bem-sucedida!");
        }
    }
}
