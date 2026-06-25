// Controla o fluxo de jogo e as ações de território.
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public string currentTurn = "Player";

    private Territory territorioInicialPlayer;
    private Territory territorioInicialAI;
    private bool jogoAcabou = false; // Bloqueia ações caso o jogo termine
    private bool escolheuTerritorioInicial = false;

    [Header("Configurações de Tabuleiro")]
    [Tooltip("Quantidade máxima de tropas em um território.")]
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
        // Pede que o jogador escolha um território inicial
        if (UIManager.instance != null) {
            UIManager.instance.UpdateStatus("Escolha seu território inicial clicando em um país!", "Fase de Seleção");
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

    [Header("Áudio de Fim de Jogo")]
    public AudioSource musicaFundoAtual; // Arraste o AudioSource que toca a música do jogo aqui
    public AudioSource musicaFimDeJogo;  // Arraste um novo AudioSource com a música de Game Over aqui

    private void VerificarCondicaoDeVitoria() {
        // Verifica se um território inicial foi capturado
        if (territorioInicialPlayer != null && territorioInicialPlayer.owner == "AI") {
            jogoAcabou = true;
            PararTodosOsSons();

            if (musicaFimDeJogo != null) {
                musicaFimDeJogo.Play();
            } else if (AudioManager.instance != null) {
                AudioManager.instance.PlayDefeat();
            }

            if (UIManager.instance != null) {
                string msg = "<color=red>GAME OVER</color>\n\n" +
                             "A <color=red>IA (Inimigo)</color> dominou a Capital do <color=blue>Jogador</color>!\n\n" +
                             "<color=red><b>A IA VENCEU A PARTIDA!</b></color>";
                
                UIManager.instance.MostrarTelaGameOver(msg);
                UIManager.instance.UpdateStatus("FIM DE JOGO: A IA capturou sua Capital!", "<color=red><b>DERROTA CRÍTICA!</b></color>");
            }
            
            Debug.Log("Jogo Encerrado: Vitória da IA por captura de base.");
        }
else if (territorioInicialAI != null && territorioInicialAI.owner == "Player") {
            jogoAcabou = true;
            PararTodosOsSons();

                if (musicaFimDeJogo != null) {
                musicaFimDeJogo.Play();
            } else if (AudioManager.instance != null) {
                AudioManager.instance.PlayVictory();
            }

            if (UIManager.instance != null) {
                string corJogadorHex = (GameData.instance != null) ? GameData.instance.playerColorName : "Jogador";
                
                string msg = "<color=green>VITÓRIA!</color>\n\n" +
                             $"O <color=blue>{corJogadorHex}</color> invadiu e conquistou a Capital da <color=red>IA</color>!\n\n" +
                             "<color=green><b>PARABÉNS, VOCÊ DOMINOU O IMPÉRIO!</b></color>";
                
                UIManager.instance.MostrarTelaGameOver(msg);
                UIManager.instance.UpdateStatus("PARABÉNS! Você conquistou a Capital inimiga!", "<color=green><b>VITÓRIA SUPREMA!</b></color>");
            }
            
            Debug.Log("Jogo Encerrado: Vitória do Player por captura de base.");
        }
    }

    // Para todos os sons antes da música de vitória/derrota
    private void PararTodosOsSons() {
        if (musicaFundoAtual != null) {
            musicaFundoAtual.Stop();
        }

        AudioSource[] todosOsSons = FindObjectsOfType<AudioSource>();
        foreach (AudioSource som in todosOsSons) {
            if (som != musicaFimDeJogo) {
                som.Stop();
            }
        }
    }

    private void EscolhaInicialIA() {
        Territory[] todos = FindObjectsOfType<Territory>();
        List<Territory> neutros = new List<Territory>();

        foreach (Territory t in todos) {
            if (t.owner == "Neutral") neutros.Add(t);
        }

        if (neutros.Count > 0) {
            int indiceAleatorio = Random.Range(0, neutros.Count);
            Territory escolhaIA = neutros[indiceAleatorio];

            escolhaIA.ChangeOwner("AI", Color.red);
            escolhaIA.troops = 3;
            escolhaIA.AddTroops(0);
            
            // Guarda o território inicial da IA
            territorioInicialAI = escolhaIA;
            
            Debug.Log("A IA escolheu a capital em: " + escolhaIA.name);

            if (AudioManager.instance != null) {
                AudioManager.instance.PlayNotification(); 
            }
        }
    }

    public void SelectTerritory(Territory t) {
        if (jogoAcabou) return; // Não faz nada se o jogo acabou
        // Escolha inicial de território
        if (!escolheuTerritorioInicial) {
            if (t.owner == "Neutral") {
                Color corJogador = (GameData.instance != null) ? GameData.instance.playerColor : Color.blue;
                
                t.ChangeOwner("Player", corJogador);
                t.troops = 3;
                t.AddTroops(0);
                
                // Guarda o território inicial do jogador
                territorioInicialPlayer = t;
                
                if (AudioManager.instance != null) {
                    AudioManager.instance.PlayConfirm(); 
                }
                
                escolheuTerritorioInicial = true;
                
                // IA escolhe seu território inicial
                EscolhaInicialIA();
                
                if (UIManager.instance != null) {
                    UIManager.instance.UpdateStatus("Território escolhido! Defenda sua capital e ataque a do inimigo.");
                }
                
                DistributeTurnTroops("Player");
            }
            return;
        }

        if (currentTurn != "Player") {
            Debug.Log("Não é turno do jogador.");
            return;
        }

        if (UIManager.instance != null) {
            UIManager.instance.UpdateDiceDisplay(null, null); // Limpa dados de dados anteriores
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
                UIManager.instance.UpdateStatus("Origem confirmada. Escolha um vizinho para ATACAR (Inimigo) ou REFORÇAR (Seu).", "Aguardando destino...");
            }
        } else {
            // Verifica se o destino é vizinho da origem
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

            // Escolhe ação: mover ou atacar
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

    // Move tropas entre territórios aliados
    private void MoveTroops(Territory origin, Territory destination) {
        if (origin.troops <= 1) return;

        // Calcula tropas que podem ser movidas
        int troopsToMove = (origin.troops - 1) / 2;

        if (troopsToMove <= 0) {
            if (UIManager.instance != null) UIManager.instance.UpdateStatus("Tropas insuficientes para dividir.");
            if (AudioManager.instance != null) AudioManager.instance.PlayError();
            return;
        }

        // Ajusta para não exceder o limite de tropas
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

    // Adiciona tropas de turno a cada território da facção
    private void DistributeTurnTroops(string faction) {
        Territory[] allTerritories = FindObjectsOfType<Territory>();
        int countGenerated = 0;

        foreach (Territory t in allTerritories) {
            if (t.owner == faction) {
                // Só adiciona tropa se não ultrapassar o limite
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

        // Filtra territórios da IA com tropas suficientes
        List<Territory> validOrigins = new List<Territory>();
        foreach(Territory t in aiTerritories) {
            if (t.troops > 1) validOrigins.Add(t);
        }

        if (validOrigins.Count == 0)
        {
            Debug.Log("IA não possui tropas suficientes para atacar.");

            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateStatus("IA não possui tropas suficientes.", "IA passou o turno.");
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
                UIManager.instance.UpdateStatus("IA encerrou as ações voluntariamente.", "Nenhum ataque realizado.");
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
        if (jogoAcabou) return;

        if (attacker == null || defender == null)
            return;

        // Calcula o bônus de ataque e defesa
        int bonusAtacante = Mathf.Clamp(attacker.troops - 1, 0, 5);
        int bonusDefensor = Mathf.Clamp(defender.troops, 0, 5);

        int dadoAtacante = Random.Range(1, 7);
        int dadoDefensor = Random.Range(1, 7);

        int resultadoFinalAtacante = dadoAtacante + bonusAtacante;
        int resultadoFinalDefensor = dadoDefensor + bonusDefensor;

        if (UIManager.instance != null)
        {
            List<int> listaAtk = new List<int> { dadoAtacante };
            List<int> listaDef = new List<int> { dadoDefensor };
            UIManager.instance.UpdateDiceDisplay(listaAtk, listaDef);
            
            if (UIManager.instance.diceResultText != null)
            {
                UIManager.instance.diceResultText.text = 
                    $"<b>Ataque:</b> Dado ({dadoAtacante}) + Tropas ({bonusAtacante}) = <b>{resultadoFinalAtacante}</b>\n" +
                    $"<b>Defesa:</b> Dado ({dadoDefensor}) + Tropas ({bonusDefensor}) = <b>{resultadoFinalDefensor}</b>";
            }
        }

        // Decide o resultado da batalha
        if (resultadoFinalAtacante > resultadoFinalDefensor)
        {
            // Remove tropas do defensor em caso de vitória
            defender.RemoveTroops(defender.troops); 
        }
        else
        {
            // Se o atacante perder, ele ainda perde apenas 1 tropa (penalidade por falha)
            attacker.RemoveTroops(1); 
        }

        bool conquered = defender.troops <= 0;
        string battleMessage;

        if (conquered)
        {
            string newOwner = attacker.owner;
            Color chosenPlayerColor = (GameData.instance != null) ? GameData.instance.playerColor : Color.blue;
            Color newColor = (newOwner == "Player") ? chosenPlayerColor : Color.red;

            defender.ChangeOwner(newOwner, newColor);

            VerificarCondicaoDeVitoria();

            if (jogoAcabou) return;

            // Move tropas para o território conquistado
            int troopsToOccupy = attacker.troops - 1;
            attacker.RemoveTroops(troopsToOccupy);
            defender.troops = troopsToOccupy;
            defender.AddTroops(0);

            if (newOwner == "Player")
                battleMessage = $"Vitória Relâmpago! {defender.name} foi dominado!";
            else
                battleMessage = $"A IA dominou {defender.name} instantaneamente!";
        }
        else
        {
            battleMessage = attacker.owner == "Player" ? "Ataque repelido! Perdeu 1 tropa." : "Defendeu com sucesso!";
        }

        if (UIManager.instance != null)
            UIManager.instance.UpdateStatus(battleMessage);

        // Reproduz sons conforme o resultado
        if (AudioManager.instance != null)
        {
            if (attacker.owner == "Player")
            {
                if (conquered) AudioManager.instance.PlayVictory();
                else AudioManager.instance.PlayDefeat();
            }
            else if (conquered && defender.owner == "AI")
            {
                AudioManager.instance.PlayDefeat();
            }
        }
    }
}

public static class AudioFallbackExtension {
    public static void PlayKeepVictory(this AudioManager am) {
        am.PlayVictory();
    }
}