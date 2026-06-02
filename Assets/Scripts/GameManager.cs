//Controla a máquina de estados do jogo (Turno do Jogador, Turno da IA, Verificação de Vitória).
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public string currentTurn = "Player";

    [Header("Configurações de Tabuleiro")]
    [Tooltip("Quantidade máxima de tropas permitida em um único território para evitar objetos imortais.")]
    public int maxTroopsPerTerritory = 15;

    void Awake() {
        if (instance == null) instance = this;
    }

    void Start() {
        SetupInitialOwners();
        // Concede as tropas iniciais do primeiro turno
        DistributeTurnTroops(currentTurn);
        
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

    private System.Collections.IEnumerator BeginAITurn()
    {
        yield return new WaitForSeconds(1.5f);
        AIPlay();
    }

    private System.Collections.IEnumerator FinishAITurn()
    {
        yield return new WaitForSeconds(1.5f);
        EndTurn();
    }

    public void SelectTerritory(Territory t) {
        if (currentTurn != "Player") {
            Debug.Log("Não é turno do jogador.");
            return;
        }

        if (UIManager.instance != null) {
            UIManager.instance.UpdateDiceDisplay(null, null); // Limpa os dados anteriores
        }

        if (selectedOrigin == null) {
            if (t.owner != "Player") {
                Debug.Log($"Território '{t.name}' pertence a '{t.owner}'. Selecione um território do jogador.");
                if (UIManager.instance != null) {
                    UIManager.instance.UpdateStatus($"Selecione um território do jogador. '{t.name}' é {t.owner}.");
                }
                if (AudioManager.instance != null) {
                    AudioManager.instance.PlayError();
                }
                return;
            }

            if (t.troops <= 1) {
                Debug.Log("Território com apenas 1 tropa não pode mover ou atacar.");
                if (UIManager.instance != null) {
                    UIManager.instance.UpdateStatus("Este território não tem tropas suficientes para agir (mínimo 2).");
                }
                if (AudioManager.instance != null) {
                    AudioManager.instance.PlayError();
                }
                return;
            }

            selectedOrigin = t;
            Debug.Log("Origem selecionada: " + t.name);
            if (UIManager.instance != null) {
                UIManager.instance.UpdateSelectionInfo(selectedOrigin, null);
                UIManager.instance.UpdateStatus("Origem confirmada. Escolha um vizinho para ATACAR (Inimigo) ou REFORÇAR (Seu).");
                UIManager.instance.UpdateBattleResult("Aguardando destino...");
            }
        } else {
            // Validação de Vizinhos: O destino DEVE ser vizinho da origem
            bool isNeighbor = false;
            if (selectedOrigin.neighbors != null) {
                foreach (Territory neighbor in selectedOrigin.neighbors) {
                    if (neighbor == t) {
                        isNeighbor = true;
                        break;
                    }
                }
            }

            if (!isNeighbor) {
                Debug.LogWarning($"Ação inválida! {t.name} não faz fronteira com {selectedOrigin.name}.");
                if (UIManager.instance != null) {
                    UIManager.instance.UpdateStatus("Ação inválida! Escolha um território vizinho.");
                    UIManager.instance.UpdateSelectionInfo(null, null);
                }
                if (AudioManager.instance != null) {
                    AudioManager.instance.PlayError();
                }
                selectedOrigin = null;
                return;
            }

            // TÁTICO / TABULEIRO: Diferenciação de Ação (Mover vs Atacar)
            if (t.owner == "Player") {
                // Ação: Mover/Manejar tropas entre territórios do próprio jogador
                MoveTroops(selectedOrigin, t);
            } else {
                // Ação: Atacar território inimigo ou neutro
                if (UIManager.instance != null) {
                    UIManager.instance.UpdateSelectionInfo(selectedOrigin, t);
                    UIManager.instance.UpdateStatus($"Atacando {t.name}...\n{selectedOrigin.name} ({selectedOrigin.troops}) vs {t.name} ({t.troops})");
                }
                
                // Executa a batalha corrigida
                Battle(selectedOrigin, t);

                // combate concluído, agora o turno termina
                EndTurn();
            }

            selectedOrigin = null;
        }
    }

    public Territory GetSelectedOrigin() {
        return selectedOrigin;
    }

    // MECÂNICA TÁTICA: Mover metade das tropas para reforço de fronteira
    private void MoveTroops(Territory origin, Territory destination) {
        if (origin.troops <= 1) return;

        // Calcula metade das tropas disponíveis para o remanejamento
        int troopsToMove = (origin.troops - 1) / 2;

        if (troopsToMove <= 0) {
            if (UIManager.instance != null) UIManager.instance.UpdateStatus("Tropas insuficientes para dividir.");
            if (AudioManager.instance != null) AudioManager.instance.PlayError();
            return;
        }

        // ANTI-IMORTALIDADE: Valida se o destino vai estourar o limite máximo permitido
        if (destination.troops + troopsToMove > maxTroopsPerTerritory) {
            troopsToMove = maxTroopsPerTerritory - destination.troops;
            if (troopsToMove <= 0) {
                if (UIManager.instance != null) UIManager.instance.UpdateStatus("Destino já atingiu o limite máximo de tropas táticas!");
                if (AudioManager.instance != null) AudioManager.instance.PlayError();
                return;
            }
        }

        origin.RemoveTroops(troopsToMove);
        destination.AddTroops(troopsToMove);

        if (UIManager.instance != null) {
            UIManager.instance.UpdateStatus($"Movimentação concluída: {troopsToMove} tropas movidas para {destination.name}.");
            UIManager.instance.UpdateSelectionInfo(null, null);
        }

        if (AudioManager.instance != null) {
            AudioManager.instance.PlayConfirm();
        }

        EndTurn();
    }

    // CAPTURA / TABULEIRO: Sistema de ganho passivo a cada início de turno
    private void DistributeTurnTroops(string faction) {
        Territory[] allTerritories = FindObjectsOfType<Territory>();
        int countGenerated = 0;

        foreach (Territory t in allTerritories) {
            if (t.owner == faction) {
                // ANTI-IMORTALIDADE: Só ganha tropa se estiver abaixo do teto máximo definido
                if (t.troops < maxTroopsPerTerritory) {
                    t.AddTroops(1);
                    countGenerated++;
                }
            }
        }
        Debug.Log($"Fase de Alocação: Fação {faction} recebeu +1 tropa em {countGenerated} territórios.");
    }

    public void EndTurn()
    {
        currentTurn = (currentTurn == "Player") ? "AI" : "Player";

        Debug.Log($"Turno atual: {currentTurn}");

        DistributeTurnTroops(currentTurn);

        if (UIManager.instance != null)
            UIManager.instance.UpdateTurn(currentTurn);

        if (currentTurn == "AI")
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateStatus("IA está pensando...");
                UIManager.instance.UpdateSelectionInfo(null, null);
            }

            StartCoroutine(BeginAITurn());
        }
        else
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlayNotification();

            if (UIManager.instance != null)
                UIManager.instance.UpdateStatus("Seu turno.");
        }
    }

    public void AIPlay() {
        Territory[] allTerritories = FindObjectsOfType<Territory>();
        Territory[] aiTerritories = System.Array.FindAll(allTerritories, t => t.owner == "AI");

        // Filtra apenas territórios da IA que tenham capacidade de atacar/mover (tropas > 1)
        List<Territory> validOrigins = new List<Territory>();
        foreach(Territory t in aiTerritories) {
            if (t.troops > 1) validOrigins.Add(t);
        }

        if (validOrigins.Count == 0)
        {
            Debug.Log("IA não possui tropas suficientes para atacar.");

            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateStatus("IA não possui tropas suficientes.");
                UIManager.instance.UpdateBattleResult("IA passou o turno.");
            }

            StartCoroutine(FinishAITurn());
            return;
        }

        List<Territory> neutralTargets = new List<Territory>();
        List<Territory> playerTargets = new List<Territory>();
        List<Territory> originForNeutral = new List<Territory>();
        List<Territory> originForPlayer = new List<Territory>();

        foreach (Territory aiOrigin in validOrigins) {
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
        }

        if (origin == null || destination == null) {
            Debug.Log("IA escolheu passar o turno estrategicamente.");
            if (UIManager.instance != null) {
                UIManager.instance.UpdateStatus("IA encerrou as ações voluntariamente.");
                UIManager.instance.UpdateBattleResult("Nenhum ataque realizado.");
            }
            EndTurn();
            return;
        }

        Debug.Log($"IA ataca de {origin.name} para {destination.name}");
        if (UIManager.instance != null) {
            if (origin != null && destination != null) {
                UIManager.instance.UpdateSelectionInfo(origin, destination);
                UIManager.instance.UpdateStatus($"IA atacando {destination.name}...\n{origin.name} ({origin.troops}) vs {destination.name} ({destination.troops})");
            }
        }
        
        Battle(origin, destination);
        StartCoroutine(FinishAITurn());
    }

    public void Battle(Territory attacker, Territory defender)
    {
        if (attacker == null || defender == null)
            return;

        int attackerTroopsBeforeBattle = attacker.troops;

        int attackerDiceCount = Mathf.Clamp(attacker.troops - 1, 1, 3);
        int defenderDiceCount = Mathf.Clamp(defender.troops, 1, 2);

        List<int> attackerResults = new List<int>();
        List<int> defenderResults = new List<int>();

        for (int i = 0; i < attackerDiceCount; i++)
            attackerResults.Add(Random.Range(1, 7));

        for (int i = 0; i < defenderDiceCount; i++)
            defenderResults.Add(Random.Range(1, 7));

        attackerResults.Sort((a, b) => b.CompareTo(a));
        defenderResults.Sort((a, b) => b.CompareTo(a));

        if (UIManager.instance != null)
            UIManager.instance.UpdateDiceDisplay(attackerResults, defenderResults);

        int comparisons = Mathf.Min(attackerResults.Count, defenderResults.Count);

        int attackerLosses = 0;
        int defenderLosses = 0;

        for (int i = 0; i < comparisons; i++)
        {
            if (attackerResults[i] > defenderResults[i])
                defenderLosses++;
            else
                attackerLosses++;
        }

        Debug.Log("===== RESULTADO DOS DADOS =====");
        Debug.Log($"Atacante: [{string.Join(", ", attackerResults)}]");
        Debug.Log($"Defensor: [{string.Join(", ", defenderResults)}]");

        Debug.Log("===== ANTES DAS BAIXAS =====");
        Debug.Log($"Território atacante: {attacker.name}");
        Debug.Log($"Tropas atacante: {attacker.troops}");

        Debug.Log($"Território defensor: {defender.name}");
        Debug.Log($"Tropas defensor: {defender.troops}");

        Debug.Log("===== PERDAS CALCULADAS =====");
        Debug.Log($"Perdas atacante: {attackerLosses}");
        Debug.Log($"Perdas defensor: {defenderLosses}");

        attacker.RemoveTroops(attackerLosses);
        defender.RemoveTroops(defenderLosses);

        Debug.Log("===== APÓS AS BAIXAS =====");
        Debug.Log($"Atacante restante: {attacker.troops}");
        Debug.Log($"Defensor restante: {defender.troops}");

        bool conquered = defender.troops <= 0;

        Debug.Log("===== VERIFICAÇÃO DE CONQUISTA =====");
        Debug.Log($"Defensor ficou com {defender.troops} tropas");
        Debug.Log($"Conquistado? {conquered}");

        string battleMessage;

        if (conquered)
        {
            string newOwner = attacker.owner;

            Color newColor =
                (newOwner == "Player")
                ? Color.red
                : Color.blue;

            defender.ChangeOwner(newOwner, newColor);

            int troopsToOccupy = attacker.troops - 1;

            Debug.Log("===== OCUPAÇÃO =====");
            Debug.Log($"Tropas no atacante após batalha: {attacker.troops}");
            Debug.Log($"Tropas enviadas para ocupação: {troopsToOccupy}");

            attacker.RemoveTroops(troopsToOccupy);

            defender.troops = troopsToOccupy;
            defender.AddTroops(0);

            Debug.Log("===== ESTADO FINAL =====");
            Debug.Log($"{attacker.name} ficou com {attacker.troops} tropas");
            Debug.Log($"{defender.name} ficou com {defender.troops} tropas");
            Debug.Log($"Dono atual de {defender.name}: {defender.owner}");

            if (newOwner == "Player")
            {
                battleMessage =
                    $"Vitória! {defender.name} foi conquistado!\n" +
                    $"Tropas enviadas: {troopsToOccupy}";
            }
            else
            {
                battleMessage =
                    $"A IA conquistou {defender.name}!";
            }
            Debug.Log("===== TERRITÓRIO CONQUISTADO =====");
            Debug.Log($"Novo dono: {attacker.owner}");
            Debug.Log($"Território conquistado: {defender.name}");
        }
        else
        {
            if (attacker.owner == "Player")
            {
                battleMessage =
                    $"Ataque encerrado.\n" +
                    $"Perdas do atacante: {attackerLosses}\n" +
                    $"Perdas do defensor: {defenderLosses}";
            }
            else
            {
                battleMessage =
                    $"{defender.name} resistiu ao ataque da IA.";
            }
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateBattleResult(battleMessage);
        }

        if (AudioManager.instance != null)
        {
            if (attacker.owner == "Player")
            {
                if (conquered)
                    AudioManager.instance.PlayVictory();
                else
                    AudioManager.instance.PlayDefeat();
            }
            else if (conquered && defender.owner == "AI")
            {
                AudioManager.instance.PlayDefeat();
            }
        }

        Debug.Log(
            $"Batalha: {attacker.name} -> {defender.name} | " +
            $"AtkLoss={attackerLosses} DefLoss={defenderLosses}"
        );
    }
}
// Extensão estendida apenas para compatibilidade de nomenclatura interna do áudio
public static class AudioFallbackExtension {
    public static void PlayKeepVictory(this AudioManager am) {
        am.PlayVictory();
    }
}