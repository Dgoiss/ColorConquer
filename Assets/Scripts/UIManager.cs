//Atualiza textos de conquistas e menus.
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public Text turnText;

    void Update() {
        if (GameManager.instance != null) {
            turnText.text = "Turno: " + GameManager.instance.currentTurn;
        }
    }
}
