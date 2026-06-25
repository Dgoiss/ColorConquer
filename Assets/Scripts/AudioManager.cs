using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fontes de Áudio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Música de Fundo")]
    public AudioClip backgroundMusic; 

    [Header("Categorias de Som")]
    public AudioClip Som_Confirmacao; // som de confirmação
    public AudioClip Som_Erro; // som de erro
    public AudioClip Som_Combate_Vitoria; // som de vitória
    public AudioClip Som_Combate_Derrota; // som de derrota
    public AudioClip Som_Notificacao; // som de notificação de turno

    [Header("Clipes Legados (compatibilidade)")]
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

    void Start()
    {
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