// Script de região do mapa.
using TMPro;
using UnityEngine;

public class Territory : MonoBehaviour {
    public int id;
    public string owner = "Neutral"; // "Player", "AI" ou "Neutral"
    public int troops = 1;
    public Territory[] neighbors;
    private SpriteRenderer sr;
    public TextMeshPro troopsText;

    // Cores base caso não haja GameData
    private static readonly Color playerColor = Color.blue;
    private static readonly Color aiColor = Color.red;
    private static readonly Color neutralColor = Color.gray;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        UpdateColor();
        UpdateTroopsText();
    }

    void OnMouseDown() {
        if (GameManager.instance != null) {
            Debug.Log("Clicou em: " + gameObject.name);
            // Som de confirmação no turno do jogador
            if (GameManager.instance.currentTurn == "Player" && owner == "Player") {
                if (AudioManager.instance != null) {
                    AudioManager.instance.PlayConfirm();
                }
            }

            GameManager.instance.SelectTerritory(this);
        } else {
            Debug.LogError("GameManager não encontrado na cena!");
        }
    }

    void OnMouseEnter() {
        // Exibe informações ao passar o mouse
        if (GameManager.instance != null && GameManager.instance.currentTurn == "Player" && UIManager.instance != null) {
            UIManager.instance.UpdateHoverInfo(this);
        }
    }

    void OnMouseExit() {
        // Limpa o painel ao sair com o mouse
        if (GameManager.instance != null && GameManager.instance.currentTurn == "Player" && UIManager.instance != null) {
            UIManager.instance.UpdateHoverInfo(null);
        }
    }

    public void ChangeOwner(string newOwner, Color color) {
        owner = newOwner;
        UpdateColor(color);
        UpdateTroopsText();
    }

    public void AddTroops(int amount) {
        troops += amount;
        UpdateTroopsText();
    }

    public void RemoveTroops(int amount) {
        troops = Mathf.Max(0, troops - amount);
        UpdateTroopsText();
    }

    private void UpdateTroopsText() {
        if (troopsText != null) {
            troopsText.text = troops.ToString();
        }
    }

    private void UpdateColor(Color? color = null) {
        if (sr == null) return;

        if (color != null) {
            sr.color = color.Value;
            return;
        }

        if (owner == "Player") {
            sr.color = (GameData.instance != null) ? GameData.instance.playerColor : Color.red;
        } else if (owner == "AI") {
            sr.color = aiColor;
        } else {
            sr.color = neutralColor;
        }
    }
}