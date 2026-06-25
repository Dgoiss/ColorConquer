// Atualiza textos da UI.
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager instance;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playerColorText;
    public TextMeshProUGUI diceResultText;

    private string statusMessage = "";
    private string battleResultMessage = "";

    [Header("Painel de Fim de Jogo")]
    public GameObject painelGameOver;          // Painel de game over
    public TextMeshProUGUI textoGameOver;

    [Header("Painel Unificado de Inspeção")]
    // Texto de inspeção por mouse
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
            UpdateStatus("Clique no seu território para começar.", "Bem-vindo ao jogo!");
            UpdateHoverInfo(null); // Inicia painel vazio
            RefreshUnifiedStatus();
        }
    }

    public void UpdateTurn(string currentTurn) {
        if (turnText != null) {
            turnText.text = "Turno atual: " + currentTurn;
        }
    }

    public void UpdateStatus(string status) {
        statusMessage = status;
        battleResultMessage = string.Empty;
        RefreshUnifiedStatus();
    }

    public void UpdateStatus(string status, string battleResult) {
        statusMessage = status;
        battleResultMessage = battleResult;
        RefreshUnifiedStatus();
    }

    public void UpdateBattleResult(string result) {
        battleResultMessage = result;
        RefreshUnifiedStatus();
    }

    private void RefreshUnifiedStatus() {
        if (statusText == null) return;

        string unified = statusMessage;

        if (!string.IsNullOrEmpty(battleResultMessage)) {
            if (!string.IsNullOrEmpty(unified)) {
                unified += "\n";
            }
            unified += battleResultMessage;
        }

        statusText.text = unified;
    }

    // Compatibilidade com GameManager
    public void UpdateSelectionInfo(Territory origin, Territory target) {
        // Limpa o painel se a seleção for removida
        if (origin == null && target == null) {
            UpdateHoverInfo(null);
        }
    }

    public void MostrarTelaGameOver(string mensagem) {
        if (painelGameOver != null) {
            painelGameOver.SetActive(true);
        }

        if (textoGameOver != null) {
            textoGameOver.text = mensagem;
        }
    }

    // Atualiza o texto do painel de inspeção
    public void UpdateHoverInfo(Territory t) {
        if (hoverInfoText == null) return;

        if (t == null) {
            hoverInfoText.text = "<b>Inspetor</b>";
            return;
        }

        // Prepara texto colorido para o dono
        string donoColorido = t.owner;
        if (t.owner == "Player") {
            // Usa a cor escolhida pelo jogador
            string corNomePt = (GameData.instance != null) ? GameData.instance.playerColorName : "Azul";
            
            // Seleciona a tag de cor para o texto
            string corHtml = "red"; // Fallback padrão
            switch (corNomePt.ToLower()) {
                case "verde": corHtml = "green"; break;
                case "amarelo": corHtml = "yellow"; break;
                case "laranja": corHtml = "orange"; break;
                case "roxo": corHtml = "purple"; break;
                case "azul": corHtml = "blue"; break;
            }

            // Define o texto colorido do dono
            donoColorido = $"<color={corHtml}>Seu Exército ({corNomePt})</color>";
        } 
        else if (t.owner == "AI") {
            donoColorido = "<color=red>Inimigo (IA)</color>";
        } 
        else {
            donoColorido = "<color=white>Neutro</color>";
        }

        // Exibe todas as informações consolidadas no mesmo componente TextMeshPro
        hoverInfoText.text = $"<b>Região:</b> {t.name}\n" +
                             $"<b>Controle:</b> {donoColorido}\n" +
                             $"<b>Força Militar:</b> {t.troops} {(t.troops == 1 ? "tropa" : "tropas")}";
    }

    public void UpdatePlayerColor() {
        if (playerColorText != null) {
            string corNome = (GameData.instance != null) ? GameData.instance.playerColorName : "Azul";
            playerColorText.text = "Sua cor: " + corNome;
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