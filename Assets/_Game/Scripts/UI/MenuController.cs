using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of game scene to load")]
    public string gameSceneName = "GameScene";

    [Header("Panels")]
    [Tooltip("How To Play panel")]
    public GameObject howToPlayPanel;

    [Tooltip("Settings panel")]
    public GameObject settingsPanel;

    [Header("High Score")]
    [Tooltip("High score display text")]
    public TextMeshProUGUI highScoreText;

    [Header("Settings UI")]
    [Tooltip("SFX volume slider")]
    public Slider sfxSlider;

    [Tooltip("Music volume slider")]
    public Slider musicSlider;

    void Start()
    {
        // Hide panels initially
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Show cursor in menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load high score
        LoadHighScore();

        // Load volume settings
        LoadVolumeSettings();

        // Setup slider listeners
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    
    // BUTTON CLICK HANDLERS

    public void OnPlayButtonClick()
    {
        Debug.Log("Play button clicked!");
        PlayClickSound();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnHowToPlayClick()
    {
        Debug.Log("How to play clicked!");
        PlayClickSound();
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void OnSettingsClick()
    {
        Debug.Log("Settings clicked!");
        PlayClickSound();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnQuitClick()
    {
        Debug.Log("Quit clicked!");
        PlayClickSound();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnBackClick()
    {
        Debug.Log("Back clicked!");
        PlayClickSound();

        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    
    // VOLUME HANDLERS

    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    void LoadVolumeSettings()
    {
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.3f);

        if (sfxSlider != null) sfxSlider.value = sfxVol;
        if (musicSlider != null) musicSlider.value = musicVol;
    }

    
    // HIGH SCORE

    void LoadHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE\n" + highScore;
    }

     
    // HELPERS

    void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}