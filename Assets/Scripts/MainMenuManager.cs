using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    [Header("UI Text")]
    public TextMeshProUGUI selectedColorText;

    [Header("Painéis")]
    public GameObject painelCores; // Painel de seleção de cores

    void Start() {
        if (GameData.instance != null && selectedColorText != null) {
            selectedColorText.text = "Cor Selecionada: " + GameData.instance.playerColorName;
        }
        
        // Fecha o painel no início
        if(painelCores != null) painelCores.SetActive(false);
    }

    // Alterna abertura do painel
    public void AbrirFecharPainel() {
        if (painelCores != null) {
            // Alterna o estado atual do painel
            bool estadoAtual = painelCores.activeSelf;
            painelCores.SetActive(!estadoAtual);
        }
    }

    public void SelectColor(string colorName) {
        if (GameData.instance == null) return;

        switch (colorName.ToLower()) {
            case "azul":
                GameData.instance.playerColor = Color.blue;
                GameData.instance.playerColorName = "Azul";
                break;
            case "verde":
                GameData.instance.playerColor = new Color(0f, 0.6f, 0f); // Cor verde do jogador
                GameData.instance.playerColorName = "Verde";
                break;
            case "amarelo":
                GameData.instance.playerColor = new Color(0.9f, 0.9f, 0f);
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

        // Fecha o painel depois de escolher
        if (painelCores != null) {
            painelCores.SetActive(false);
        }

        if (AudioManager.instance != null) AudioManager.instance.PlayConfirm();
    }

    public void StartGame() {
        SceneManager.LoadScene("MainGameScene"); 
    }
}