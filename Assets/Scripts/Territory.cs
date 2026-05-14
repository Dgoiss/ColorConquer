//Script anexado a cada região do mapa. Armazena: ID, Dono Atual (Player/IA), Quantidade de Tropas e Cor.
using UnityEngine;

public class Territory : MonoBehaviour {
    public int id;
    public string owner; // "Player" ou "AI"
    public int troops = 1;
    private SpriteRenderer sr;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    void OnMouseDown() {
        Debug.Log("Clicou em: " + gameObject.name);
        GameManager.instance.SelectTerritory(this);
    }


    public void ChangeOwner(string newOwner, Color color) {
        owner = newOwner;
        UpdateColor(color);
    }

    void UpdateColor(Color? color = null) {
        if (sr != null) {
            sr.color = color ?? sr.color;
        }
    }
}
