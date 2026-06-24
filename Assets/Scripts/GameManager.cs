//Controla a máquina de estados do jogo (Turno do Jogador, Turno da IA, Verificação de Vitória).
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
        // Apenas garante que a interface saiba que o jogador precisa escolher
        if (UIManager.instance != null) {
            UIManager.instance.UpdateStatus("Escolha seu território inicial clicando em um país!", "Fase de Seleção");
        }
        // Não damos países a ninguém aqui agora!
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
        // 1. Se a IA conseguiu dominar o território inicial do Player
        if (territorioInicialPlayer != null && territorioInicialPlayer.owner == "AI") { //[cite: 14]
            jogoAcabou = true; //[cite: 14]
            
            // 🛑 PARAR TODOS OS SONS E MÚSICAS DO JOGO
            PararTodosOsSons();

            // 🎵 TOCAR MÚSICA DO FIM DO JOGO
            if (musicaFimDeJogo != null) {
                musicaFimDeJogo.Play();
            } else if (AudioManager.instance != null) {
                AudioManager.instance.PlayDefeat(); // Fallback de som caso não uses o AudioSource direto //[cite: 14]
            }

            // 📺 EXIBIR TEXTO NO MEIO DA TELA
            if (UIManager.instance != null) {
                string msg = "<color=red>GAME OVER</color>\n\n" +
                             "A <color=red>IA (Inimigo)</color> dominou a Capital do <color=blue>Jogador</color>!\n\n" +
                             "<color=red><b>A IA VENCEU A PARTIDA!</b></color>";
                
                UIManager.instance.MostrarTelaGameOver(msg);
                UIManager.instance.UpdateStatus("FIM DE JOGO: A IA capturou sua Capital!", "<color=red><b>DERROTA CRÍTICA!</b></color>"); //[cite: 14]
            }
            
            Debug.Log("Jogo Encerrado: Vitória da IA por captura de base."); //[cite: 14]
        }
        // 2. Se o Player conseguiu dominar o território inicial da IA
        else if (territorioInicialAI != null && territorioInicialAI.owner == "Player") { //[cite: 14]
            jogoAcabou = true; //[cite: 14]
            
            // 🛑 PARAR TODOS OS SONS E MÚSICAS DO JOGO
            PararTodosOsSons();

            // 🎵 TOCAR MÚSICA DO FIM DO JOGO
            if (musicaFimDeJogo != null) {
                musicaFimDeJogo.Play();
            } else if (AudioManager.instance != null) {
                AudioManager.instance.PlayVictory(); // Fallback de som caso não uses o AudioSource direto //[cite: 14]
            }

            // 📺 EXIBIR TEXTO NO MEIO DA TELA
            if (UIManager.instance != null) {
                string corJogadorHex = (GameData.instance != null) ? GameData.instance.playerColorName : "Jogador";
                
                string msg = "<color=green>VITÓRIA!</color>\n\n" +
                             $"O <color=blue>{corJogadorHex}</color> invadiu e conquistou a Capital da <color=red>IA</color>!\n\n" +
                             "<color=green><b>PARABÉNS, VOCÊ DOMINOU O IMPÉRIO!</b></color>";
                
                UIManager.instance.MostrarTelaGameOver(msg);
                UIManager.instance.UpdateStatus("PARABÉNS! Você conquistou a Capital inimiga!", "<color=green><b>VITÓRIA SUPREMA!</b></color>"); //[cite: 14]
            }
            
            Debug.Log("Jogo Encerrado: Vitória do Player por captura de base."); //[cite: 14]
        }
    }

    // Método auxiliar focado em silenciar o ambiente antes da música da vitória/derrota
    private void PararTodosOsSons() {
        // 1. Para a música de fundo principal que associaste no Inspector
        if (musicaFundoAtual != null) {
            musicaFundoAtual.Stop();
        }

        // 2. Procura e desliga QUALQUER AudioSource que esteja ativo na cena para garantir silêncio absoluto (efeitos de dados, cliques, etc)
        AudioSource[] todosOsSons = FindObjectsOfType<AudioSource>();
        foreach (AudioSource som in todosOsSons) {
            // Só não para a música do fim de jogo que vai começar agora!
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
            
            // 🔥 ADICIONADO: Guarda o território inicial da IA
            territorioInicialAI = escolhaIA;
            
            Debug.Log("A IA escolheu a capital em: " + escolhaIA.name);

            if (AudioManager.instance != null) {
                AudioManager.instance.PlayNotification(); 
            }
        }
    }

    public void SelectTerritory(Territory t) {
        if (jogoAcabou) return; // Se o jogo acabou, não faz mais nada
    // LÓGICA DE ESCOLHA INICIAL
        if (!escolheuTerritorioInicial) {
            if (t.owner == "Neutral") {
                Color corJogador = (GameData.instance != null) ? GameData.instance.playerColor : Color.blue;
                
                t.ChangeOwner("Player", corJogador);
                t.troops = 3;
                t.AddTroops(0);
                
                // 🔥 ADICIONADO: Guarda o território inicial do jogador
                territorioInicialPlayer = t;
                
                if (AudioManager.instance != null) {
                    AudioManager.instance.PlayConfirm(); 
                }
                
                escolheuTerritorioInicial = true;
                
                // IA escolhe o dela agora
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
                UIManager.instance.UpdateStatus("Origem confirmada. Escolha um vizinho para ATACAR (Inimigo) ou REFORÇAR (Seu).", "Aguardando destino...");
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

        // 🎲 LÓGICA DE UM DADO COM BÓNUS
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

        // Determina o vencedor da batalha
        if (resultadoFinalAtacante > resultadoFinalDefensor)
        {
            // 🔥 CONQUISTA INSTANTÂNEA:
            // Remove todas as tropas do defensor para forçar a conquista imediata
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

            // Move as tropas para o novo território
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

        // Sons e logs mantidos...
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