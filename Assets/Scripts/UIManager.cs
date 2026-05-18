//Atualiza textos de conquistas e menus.
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager instance;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI originInfoText;
    public TextMeshProUGUI targetInfoText;
    public TextMeshProUGUI playerColorText;
    public TextMeshProUGUI battleResultText;

    void Awake() {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if(GameManager.instance != null)
        {
            UpdateTurn(GameManager.instance.currentTurn);
            UpdatePlayerColor();
            UpdateStatus("Clique no seu território para começar.");
            UpdateSelectionInfo(null, null);
            UpdateBattleResult("Bem-vindo ao jogo!");
        }
    }

    public void UpdateTurn(string currentTurn) {
        if (turnText != null) {
            turnText.text = "Turno atual: " + currentTurn;
        }
    }

    public void UpdateStatus(string status) {
        if (statusText != null) {
            statusText.text = status;
        }
    }

    public void UpdateSelectionInfo(Territory origin, Territory target) {
        if (originInfoText != null) {
            if (origin != null) {
                originInfoText.text = $"Seu território selecionado:\n{origin.name}\nTropas: {origin.troops}\nDono: {origin.owner}";
            } else {
                originInfoText.text = "Seu território selecionado:\nNenhum";
            }
        }

        if (targetInfoText != null) {
            if (target != null) {
                targetInfoText.text = $"Território de destino:\n{target.name}\nTropas: {target.troops}\nDono: {target.owner}";
            } else {
                targetInfoText.text = "Território de destino:\nNenhum";
            }
        }
    }

    public void UpdatePlayerColor() {
        if (playerColorText != null) {
            playerColorText.text = "Sua cor: Vermelho";
        }
    }

    public void UpdateBattleResult(string result) {
        if (battleResultText != null) {
            battleResultText.text = result;
        }
    }
}