using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("=== BOW SOUNDS ===")]
    [Tooltip("Bow string draw sound (when charging)")]
    public AudioClip bowDrawSound;

    [Tooltip("Bow release/twang sound (when shooting)")]
    public AudioClip bowReleaseSound;

    [Header("=== ARROW SOUNDS ===")]
    [Tooltip("Arrow flying through air")]
    public AudioClip arrowWhooshSound;

    [Tooltip("Arrow hitting target (normal)")]
    public AudioClip arrowHitSound;

    [Tooltip("Arrow hitting bullseye (special)")]
    public AudioClip bullseyeSound;

    [Header("=== UI SOUNDS ===")]
    [Tooltip("Button click sound")]
    public AudioClip buttonClickSound;

    [Tooltip("Round start sound")]
    public AudioClip roundStartSound;

    [Tooltip("Game over fanfare")]
    public AudioClip gameOverSound;

    [Header("=== BACKGROUND MUSIC ===")]
    [Tooltip("Background ambient music")]
    public AudioClip backgroundMusic;

    [Header("=== VOLUME SETTINGS ===")]
    [Range(0f, 1f)]
    [Tooltip("Master volume for SFX")]
    public float sfxVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("Master volume for music")]
    public float musicVolume = 0.3f;

    // Audio sources
    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioSource bowDrawSource; // Separate for looping bow draw

    // Singleton
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create audio sources
        SetupAudioSources();
    }

    void Start()
    {
        // Start background music
        PlayBackgroundMusic();
    }

    void SetupAudioSources()
    {
        // SFX Source (one-shot sounds)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;

        // Music Source (looping)
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        // Bow Draw Source (special - can be stopped mid-play)
        bowDrawSource = gameObject.AddComponent<AudioSource>();
        bowDrawSource.playOnAwake = false;
        bowDrawSource.loop = false;
        bowDrawSource.volume = sfxVolume;
    }

    
    // BOW SOUNDS

    public void PlayBowDraw()
    {
        if (bowDrawSound != null && bowDrawSource != null)
        {
            bowDrawSource.clip = bowDrawSound;
            bowDrawSource.Play();
        }
    }

    public void StopBowDraw()
    {
        if (bowDrawSource != null && bowDrawSource.isPlaying)
        {
            bowDrawSource.Stop();
        }
    }

    public void PlayBowRelease()
    {
        PlaySFX(bowReleaseSound);
    }

    
    // ARROW SOUNDS

    public void PlayArrowWhoosh()
    {
        PlaySFX(arrowWhooshSound);
    }

    public void PlayArrowHit()
    {
        PlaySFX(arrowHitSound);
    }

    public void PlayBullseye()
    {
        PlaySFX(bullseyeSound);
    }

    
    // UI SOUNDS

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayRoundStart()
    {
        PlaySFX(roundStartSound);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverSound);
    }

    
    // BACKGROUND MUSIC

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PauseBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    
    // HELPER METHODS

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    // Volume control methods (for settings menu later)
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (bowDrawSource != null) bowDrawSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }
}