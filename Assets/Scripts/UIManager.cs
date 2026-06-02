//Atualiza textos de conquistas e menus.
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager instance;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playerColorText;
    public TextMeshProUGUI battleResultText;
    public TextMeshProUGUI diceResultText;

    [Header("Painel Unificado de Inspeção")]
    // Este é o único TextMeshPro que você usará para inspecionar passando o mouse
    public TextMeshProUGUI hoverInfoText; 

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
            UpdateHoverInfo(null); // Inicializa o painel único vazio
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

    // === SOLUÇÃO DO ERRO CS1061 ===
    // O GameManager precisa que esse método exista. Vamos mantê-lo aqui para compatibilidade do sistema de cliques!
    public void UpdateSelectionInfo(Territory origin, Territory target) {
        // Se o GameManager mandar limpar as seleções (null, null), nós limpamos também o nosso painel de mouse por segurança
        if (origin == null && target == null) {
            UpdateHoverInfo(null);
        }
    }

    // Método para atualizar o seu texto único de inspeção por mouse (Tactical / Colour)
    public void UpdateHoverInfo(Territory t) {
        if (hoverInfoText == null) return;

        if (t == null) {
            hoverInfoText.text = "<b>Inspetor</b>";
            return;
        }

        // Define uma cor em formato rich-text dependendo de quem é o dono (Categoria: Colour do sorteio)
        string donoColorido = t.owner;
        if (t.owner == "Player") donoColorido = "<color=red>Seu Exército</color>";
        else if (t.owner == "AI") donoColorido = "<color=blue>Inimigo (IA)</color>";
        else donoColorido = "<color=white>Neutro</color>";

        // Exibe todas as informações consolidadas no mesmo componente TextMeshPro
        hoverInfoText.text = $"<b>Região:</b> {t.name}\n" +
                             $"<b>Controle:</b> {donoColorido}\n" +
                             $"<b>Força Militar:</b> {t.troops} {(t.troops == 1 ? "tropa" : "tropas")}";
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

    public void UpdateDiceDisplay(System.Collections.Generic.List<int> attackerDice, System.Collections.Generic.List<int> defenderDice) {
        if (diceResultText == null) return;

        if (attackerDice == null || defenderDice == null || (attackerDice.Count == 0 && defenderDice.Count == 0)) {
            diceResultText.text = "";
            return;
        }

        string attackerStr = string.Join(", ", attackerDice);
        string defenderStr = string.Join(", ", defenderDice);

        diceResultText.text = $"<b>Dados do Atacante:</b> [{attackerStr}]\n" +
                            $"<b>Dados do Defensor:</b> [{defenderStr}]";
    }
}