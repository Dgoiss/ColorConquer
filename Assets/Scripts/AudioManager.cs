using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fontes de Áudio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Música de Fundo")]
    // 1. Coloque o seu arquivo de música nesta variável pelo Inspector do Unity
    public AudioClip backgroundMusic; 

    [Header("Categorias de Som")]
    // Categorias solicitadas pelo designer
    public AudioClip Som_Confirmacao; // cliques válidos / seleção
    public AudioClip Som_Erro; // cliques inválidos / comandos bloqueados
    public AudioClip Som_Combate_Vitoria; // jogador vence ataque
    public AudioClip Som_Combate_Derrota; // jogador perde ataque
    public AudioClip Som_Notificacao; // turno volta para o jogador

    [Header("Clipes Legados (compatibilidade)")]
    // Mantém os campos antigos caso já estejam atribuídos no Inspector
    public AudioClip clickTerritorySound;
    public AudioClip clickInvalidSound;
    public AudioClip battleVictorySound;
    public AudioClip battleDefeatSound;
    public AudioClip turnTransitionSound;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 2. O método Start roda assim que o jogo começa
    void Start()
    {
        // Se você atribuiu uma música no Inspector, ela começará a tocar aqui
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Métodos de conveniência para cada categoria (verificam fontes e fallback para clipes antigos)
    public void PlayConfirm()
    {
        if (sfxSource == null) return;
        AudioClip clip = Som_Confirmacao != null ? Som_Confirmacao : clickTerritorySound;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayError()
    {
        if (sfxSource == null) return;
        AudioClip clip = Som_Erro != null ? Som_Erro : clickInvalidSound;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayVictory()
    {
        if (sfxSource == null) return;
        AudioClip clip = Som_Combate_Vitoria != null ? Som_Combate_Vitoria : battleVictorySound;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayDefeat()
    {
        if (sfxSource == null) return;
        AudioClip clip = Som_Combate_Derrota != null ? Som_Combate_Derrota : battleDefeatSound;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayNotification()
    {
        if (sfxSource == null) return;
        AudioClip clip = Som_Notificacao != null ? Som_Notificacao : turnTransitionSound;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    // 3. Esta função configura o AudioSource da música para Loop e toca o som
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true; // Garante que vai ficar repetindo infinitamente
            musicSource.Play();
        }
    }
}