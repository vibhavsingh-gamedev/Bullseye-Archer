using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject pausePanel;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ensure game is running normally at start
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        // ESC key check
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Agar game already khatam ho chuka hai (GameOverPanel active hai), toh pause mat hone do
            if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // This freezes all physics and animations!

        if (pausePanel != null) pausePanel.SetActive(true);

        // Show and unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (AudioManager.Instance != null) AudioManager.Instance.PauseBackgroundMusic();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resumes the game world

        if (pausePanel != null) pausePanel.SetActive(false);

        // Re-lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (AudioManager.Instance != null) AudioManager.Instance.ResumeBackgroundMusic();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time scale before loading scene!
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale before heading to menu
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        SceneManager.LoadScene("MainMenu");
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
}