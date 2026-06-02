using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    [Header("UI Text")]
    public TextMeshProUGUI selectedColorText;

    void Start() {
        // Inicializa o texto com a cor padrão
        if (GameData.instance != null && selectedColorText != null) {
            selectedColorText.text = "Cor Selecionada: " + GameData.instance.playerColorName;
        }
    }

    // Método chamado pelos botões de seleção de cor
    public void SelectColor(string colorName) {
        if (GameData.instance == null) return;

        switch (colorName.ToLower()) {
            case "vermelho":
                GameData.instance.playerColor = Color.red;
                GameData.instance.playerColorName = "Vermelho";
                break;
            case "verde":
                GameData.instance.playerColor = new Color(0f, 0.6f, 0f); // Verde escuro para melhor legibilidade
                GameData.instance.playerColorName = "Verde";
                break;
            case "amarelo":
                GameData.instance.playerColor = new Color(0.9f, 0.9f, 0f); // Amarelo visível
                GameData.instance.playerColorName = "Amarelo";
                break;
            case "laranja":
                GameData.instance.playerColor = new Color(1f, 0.5f, 0f);
                GameData.instance.playerColorName = "Laranja";
                break;
        }

        if (selectedColorText != null) {
            selectedColorText.text = "Cor Selecionada: " + GameData.instance.playerColorName;
        }

        // Opcional: Tocar som de clique se houver um AudioManager
        if (AudioManager.instance != null) AudioManager.instance.PlayConfirm();
    }

    // Método chamado pelo botão "Iniciar Jogo"
    public void StartGame() {
        // Substitua pelo nome exato da sua cena principal do tabuleiro
        SceneManager.LoadScene("MainGameScene"); 
    }
}