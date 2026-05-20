//Controla a máquina de estados do jogo (Turno do Jogador, Turno da IA, Verificação de Vitória).
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public string currentTurn = "Player";

    void Awake() {
        if (instance == null) instance = this;
    }

    void Start() {
        SetupInitialOwners();
        if (UIManager.instance != null) {
            UIManager.instance.UpdateTurn(currentTurn);
        }
    }

    private void SetupInitialOwners() {
        Territory[] allTerritories = FindObjectsOfType<Territory>();
        if (allTerritories.Length == 0) return;

        bool hasPlayer = System.Array.Exists(allTerritories, t => t.owner == "Player");
        bool hasAI = System.Array.Exists(allTerritories, t => t.owner == "AI");

        if (!hasPlayer && !hasAI) {
            allTerritories[0].ChangeOwner("Player", Color.red);
            if (allTerritories.Length > 1) {
                allTerritories[1].ChangeOwner("AI", Color.blue);
            }
            return;
        }

        if (!hasPlayer) {
            Territory neutral = System.Array.Find(allTerritories, t => t.owner == "Neutral");
            if (neutral != null) neutral.ChangeOwner("Player", Color.red);
        }

        if (!hasAI) {
            Territory neutral = System.Array.Find(allTerritories, t => t.owner == "Neutral");
            if (neutral != null) neutral.ChangeOwner("AI", Color.blue);
        }
    }

    private Territory selectedOrigin;

    public void SelectTerritory(Territory t) {
        if (currentTurn != "Player") {
            Debug.Log("Não é turno do jogador.");
            return;
        }

        if (selectedOrigin == null) {
            if (t.owner != "Player") {
                Debug.Log($"Território '{t.name}' pertence a '{t.owner}'. Selecione um território do jogador.");
                if (UIManager.instance != null) {
                    UIManager.instance.UpdateStatus($"Selecione um território do jogador. '{t.name}' é {t.owner}.");
                }
                return;
            }

            selectedOrigin = t;
            Debug.Log("Origem selecionada: " + t.name);
            if (UIManager.instance != null) {
                UIManager.instance.UpdateSelectionInfo(selectedOrigin, null);
                UIManager.instance.UpdateStatus("Território confirmado. Agora escolha um território inimigo ou neutro para atacar.");
                UIManager.instance.UpdateBattleResult("Aguardando destino...");
            }
        } else {
            if (t.owner == "Player") {
                Debug.Log("Destino inválido. Escolha um território inimigo ou neutro.");
                if (UIManager.instance != null) {
                    UIManager.instance.UpdateStatus("Destino inválido. Seleção reiniciada.");
                    UIManager.instance.UpdateSelectionInfo(null, null);
                }
                selectedOrigin = null;
                return;
            }

            Debug.Log("Destino selecionado: " + t.name);
            if (UIManager.instance != null) {
                UIManager.instance.UpdateSelectionInfo(selectedOrigin, t);
                UIManager.instance.UpdateStatus($"Atacando {t.name}...\n{selectedOrigin.name} ({selectedOrigin.troops}) vs {t.name} ({t.troops})");
            }
            Battle(selectedOrigin, t);
            selectedOrigin = null;
        }
    }

    public void EndTurn() {
        currentTurn = (currentTurn == "Player") ? "AI" : "Player";
        Debug.Log("Turno atual: " + currentTurn);
        UIManager.instance.UpdateTurn(currentTurn);

        if (currentTurn == "AI") {
            if (UIManager.instance != null) {
                UIManager.instance.UpdateStatus("IA está jogando...");
                UIManager.instance.UpdateSelectionInfo(null, null);
            }
            AIPlay();
        }
    }

    public void AIPlay() {
        Territory[] allTerritories = FindObjectsOfType<Territory>();
        Territory[] aiTerritories = System.Array.FindAll(allTerritories, t => t.owner == "AI");

        if (aiTerritories.Length == 0) {
            Debug.Log("IA não possui territórios para jogar.");
            currentTurn = "Player";
            UIManager.instance.UpdateTurn(currentTurn);
            return;
        }

        List<Territory> neutralTargets = new List<Territory>();
        List<Territory> playerTargets = new List<Territory>();
        List<Territory> originForNeutral = new List<Territory>();
        List<Territory> originForPlayer = new List<Territory>();

        foreach (Territory aiOrigin in aiTerritories) {
            if (aiOrigin == null || aiOrigin.neighbors == null) continue;
            foreach (Territory neighbor in aiOrigin.neighbors) {
                if (neighbor == null) continue;
                if (neighbor.owner == "Neutral") {
                    originForNeutral.Add(aiOrigin);
                    neutralTargets.Add(neighbor);
                } else if (neighbor.owner == "Player") {
                    originForPlayer.Add(aiOrigin);
                    playerTargets.Add(neighbor);
                }
            }
        }

        Territory origin = null;
        Territory destination = null;

        if (neutralTargets.Count > 0) {
            int choice = Random.Range(0, neutralTargets.Count);
            origin = originForNeutral[choice];
            destination = neutralTargets[choice];
        } else if (playerTargets.Count > 0) {
            int choice = Random.Range(0, playerTargets.Count);
            origin = originForPlayer[choice];
            destination = playerTargets[choice];
        } else {
            Territory[] playerTerritories = System.Array.FindAll(allTerritories, t => t.owner == "Player");
            if (playerTerritories.Length > 0) {
                origin = aiTerritories[Random.Range(0, aiTerritories.Length)];
                destination = playerTerritories[Random.Range(0, playerTerritories.Length)];
            }
        }

        if (origin == null || destination == null || origin.Equals(null) || destination.Equals(null)) {
            Debug.Log("IA não encontrou um alvo válido para atacar.");
            if (UIManager.instance != null) {
                UIManager.instance.UpdateStatus("IA não encontrou alvo válido. Turno retorna para o jogador.");
                UIManager.instance.UpdateBattleResult("Nenhum ataque realizado.");
            }
            currentTurn = "Player";
            UIManager.instance.UpdateTurn(currentTurn);
            return;
        }

        Debug.Log($"IA ataca de {origin.name} para {destination.name}");
        if (UIManager.instance != null) {
            UIManager.instance.UpdateSelectionInfo(origin, destination);
            UIManager.instance.UpdateStatus($"IA atacando {destination.name}...\n{origin.name} ({origin.troops}) vs {destination.name} ({destination.troops})");
        }
        Battle(origin, destination);

        EndTurn();
    }

    public void Battle(Territory attacker, Territory defender) {
        if (attacker == null || defender == null || attacker.Equals(null) || defender.Equals(null)) {
            Debug.LogWarning("Battle aborted: attacker or defender inválido.");
            return;
        }

        int result = (attacker.troops - defender.troops) + Random.Range(1, 6);

        attacker.RemoveTroops(1);

        string battleMessage;

        if (result > 0) {
            if (attacker.owner == "Player") {
                defender.ChangeOwner("Player", Color.red);
                battleMessage = $"Você conquistou {defender.name}!\nTropas restantes: {defender.troops}";
            } else {
                defender.ChangeOwner("AI", Color.blue);
                battleMessage = $"IA conquistou {defender.name}!\nTropas restantes: {defender.troops}";
            }

            defender.troops = Mathf.Max(1, attacker.troops - 1);
            Debug.Log("Território conquistado!");
        } else {
            defender.RemoveTroops(1);
            if (attacker.owner == "Player") {
                battleMessage = $"Você perdeu o ataque em {defender.name}. Tropas defensoras restantes: {defender.troops}";
            } else {
                battleMessage = $"IA falhou em atacar {defender.name}. Você defendeu com sucesso!";
            }
            Debug.Log("Defesa bem-sucedida!");
        }

        if (UIManager.instance != null) {
            UIManager.instance.UpdateBattleResult(battleMessage);
            UIManager.instance.UpdateSelectionInfo(null, null);
        }

        if (currentTurn == "Player") {
            EndTurn();
        }
    }
}
