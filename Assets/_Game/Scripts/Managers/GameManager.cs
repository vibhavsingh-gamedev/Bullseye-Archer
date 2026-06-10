using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Total rounds in one game")]
    public int totalRounds = 5;

    [Tooltip("Arrows per round")]
    public int arrowsPerRound = 3;

    [Header("UI References - Game HUD")]
    [Tooltip("Current round display")]
    public TextMeshProUGUI roundText;

    [Tooltip("Arrows left display")]
    public TextMeshProUGUI arrowsText;

    [Tooltip("Arrows visual icons container")]
    public Transform arrowIconsContainer;

    [Tooltip("Arrow icon prefab")]
    public GameObject arrowIconPrefab;

    [Header("Round Transition Screen")]
    [Tooltip("Panel that shows between rounds")]
    public GameObject roundTransitionPanel;

    [Tooltip("Round transition text")]
    public TextMeshProUGUI roundTransitionText;

    [Tooltip("Round transition score text")]
    public TextMeshProUGUI roundTransitionScoreText;

    [Tooltip("Transition duration in seconds")]
    public float transitionDuration = 2.5f;

    [Header("Game Over Screen")]
    [Tooltip("Game over panel")]
    public GameObject gameOverPanel;

    [Tooltip("Final score text")]
    public TextMeshProUGUI finalScoreText;

    [Tooltip("Bullseye count text")]
    public TextMeshProUGUI bullseyeText;

    [Tooltip("Accuracy text")]
    public TextMeshProUGUI accuracyText;

    [Tooltip("Rating text (Gold/Silver/Bronze)")]
    public TextMeshProUGUI ratingText;

    [Header("Bow Reference")]
    [Tooltip("Bow controller to enable/disable shooting")]
    public BowController bowController;

    [Header("Camera Reference")]
    [Tooltip("Player camera (FPS) - for control toggle")]
    public Camera playerCamera;

    [Header("Delay Settings")]
    [Tooltip("Delay after arrow hit before next can shoot")]
    public float postHitDelay = 4f;

    // Singleton
    public static GameManager Instance { get; private set; }

    // Game state
    private int currentRound = 1;
    private int arrowsLeft = 0;
    private int arrowsShot = 0;
    public bool isGameActive = false;
    private bool isRoundActive = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Hide screens initially
        if (roundTransitionPanel != null) roundTransitionPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Start the game
        StartGame();
    }

    // Helper method to toggle camera control
    // Use direct reference instead of Camera.main
    void SetCameraControl(bool enabled)
    {
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera reference not assigned in GameManager!");
            return;
        }

        CameraController camController = playerCamera.GetComponent<CameraController>();
        if (camController != null)
        {
            camController.controlEnabled = enabled;
            Debug.Log("Camera Control: " + (enabled ? "ENABLED" : "DISABLED"));
        }
        else
        {
            Debug.LogError("CameraController script not found on PlayerCamera!");
        }
    }

    public void StartGame()
    {
        Debug.Log("Game Started!");

        // Reset everything
        currentRound = 1;
        arrowsShot = 0;
        isGameActive = true;

        // Reset score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        // Start first round
        StartCoroutine(StartRoundRoutine());
    }

    IEnumerator StartRoundRoutine()
    {
        isRoundActive = false;

        // Disable shooting during transition
        if (bowController != null) bowController.enabled = false;

        // Disable camera control during transition
        SetCameraControl(false);

        // Show round transition
        if (roundTransitionPanel != null)
        {
            roundTransitionPanel.SetActive(true);

            // Play round start sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRoundStart();
            }

            if (roundTransitionText != null)
            {
                if (currentRound == 1)
                    roundTransitionText.text = "Get Ready!\nRound " + currentRound;
                else
                    roundTransitionText.text = "Round " + currentRound;
            }

            if (roundTransitionScoreText != null)
            {
                if (ScoreManager.Instance != null && currentRound > 1)
                    roundTransitionScoreText.text = "Current Score: " + ScoreManager.Instance.GetTotalScore();
                else
                    roundTransitionScoreText.text = "Aim for the bullseye!";
            }
        }

        yield return new WaitForSeconds(transitionDuration);

        // Hide transition panel
        if (roundTransitionPanel != null)
            roundTransitionPanel.SetActive(false);

        // Setup round
        arrowsLeft = arrowsPerRound;
        isRoundActive = true;

        // Enable shooting
        if (bowController != null) bowController.enabled = true;

        // Re-enable camera control for gameplay
        SetCameraControl(true);

        // Update UI
        UpdateUI();
        SpawnArrowIcons();

        Debug.Log("Round " + currentRound + " Started! Arrows: " + arrowsLeft);
    }

    // Called by BowController when arrow is shot
    public void OnArrowShot()
    {
        if (!isRoundActive || !isGameActive) return;

        arrowsLeft--;
        arrowsShot++;

        Debug.Log("Arrow Shot! Remaining: " + arrowsLeft);

        UpdateUI();
        UpdateArrowIcons();

        // Check if round is over
        if (arrowsLeft <= 0)
        {
            StartCoroutine(EndRoundRoutine());
        }
    }

    IEnumerator EndRoundRoutine()
    {
        isRoundActive = false;

        // Disable shooting
        if (bowController != null) bowController.enabled = false;

        // Disable camera immediately when round ends
        SetCameraControl(false);

        // Wait for last arrow to hit and replay to play
        yield return new WaitForSeconds(postHitDelay);

        Debug.Log("Round " + currentRound + " Complete!");

        // Force disable camera AGAIN after replay (safety)
        SetCameraControl(false);

        // Check if game is over
        if (currentRound >= totalRounds)
        {
            EndGame();
        }
        else
        {
            // Next round
            currentRound++;
            StartCoroutine(StartRoundRoutine());
        }
    }

    void EndGame()
    {
        isGameActive = false;

        Debug.Log("GAME OVER!");

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Use helper method (no Camera.main)
        SetCameraControl(false);

        // Play game over sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver();
        }

        // Show game over screen
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Show stats
            if (ScoreManager.Instance != null)
            {
                int totalScore = ScoreManager.Instance.GetTotalScore();

                // Save high score
                int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
                if (totalScore > currentHighScore)
                {
                    PlayerPrefs.SetInt("HighScore", totalScore);
                    PlayerPrefs.Save();
                    Debug.Log("NEW HIGH SCORE: " + totalScore);
                }

                int bullseyes = ScoreManager.Instance.GetBullseyeCount();
                int totalShots = ScoreManager.Instance.GetTotalShots();
                int maxPossibleScore = totalRounds * arrowsPerRound * 10;

                if (finalScoreText != null)
                    finalScoreText.text = "Final Score: " + totalScore;

                if (bullseyeText != null)
                    bullseyeText.text = "Bullseyes: " + bullseyes;

                if (accuracyText != null)
                {
                    float accuracy = totalShots > 0 ? ((float)totalScore / maxPossibleScore) * 100f : 0f;
                    accuracyText.text = "Accuracy: " + accuracy.ToString("F1") + "%";
                }

                if (ratingText != null)
                {
                    string rating = GetRating(totalScore, maxPossibleScore);
                    ratingText.text = rating;
                }
            }
        }
    }

    string GetRating(int score, int maxScore)
    {
        float percentage = (float)score / maxScore;

        if (percentage >= 0.9f) return "GOLD!";
        if (percentage >= 0.7f) return "SILVER!";
        if (percentage >= 0.5f) return "BRONZE!";
        if (percentage >= 0.3f) return "GOOD!";
        return "KEEP PRACTICING!";
    }

    void UpdateUI()
    {
        if (roundText != null)
            roundText.text = "Round " + currentRound + " / " + totalRounds;

        if (arrowsText != null)
            arrowsText.text = "Arrows: " + arrowsLeft;
    }

    void SpawnArrowIcons()
    {
        if (arrowIconsContainer == null || arrowIconPrefab == null) return;

        // Clear existing icons
        foreach (Transform child in arrowIconsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new icons
        for (int i = 0; i < arrowsPerRound; i++)
        {
            Instantiate(arrowIconPrefab, arrowIconsContainer);
        }
    }

    void UpdateArrowIcons()
    {
        if (arrowIconsContainer == null) return;

        // Dim the used arrows
        int childCount = arrowIconsContainer.childCount;
        int usedCount = arrowsPerRound - arrowsLeft;

        for (int i = 0; i < childCount; i++)
        {
            Image icon = arrowIconsContainer.GetChild(i).GetComponent<Image>();
            if (icon != null)
            {
                if (i < usedCount)
                {
                    // Used - dim it
                    Color c = icon.color;
                    c.a = 0.3f;
                    icon.color = c;
                }
                else
                {
                    // Available - full color
                    Color c = icon.color;
                    c.a = 1f;
                    icon.color = c;
                }
            }
        }
    }

    // Public method - called by Restart button
    public void RestartGame()
    {
        Debug.Log("Restarting Game...");

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Lock cursor back for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Use helper method
        SetCameraControl(true);

        // Clear all arrows from scene
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject arrow in arrows)
        {
            Destroy(arrow);
        }

        StartGame();
    }

    // Public method - quit game
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoToMainMenu()
    {
        Debug.Log("Going to Main Menu...");

        // Reset cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Getters for other scripts
    public bool IsGameActive() => isGameActive;
    public bool IsRoundActive() => isRoundActive;
    public int GetArrowsLeft() => arrowsLeft;
    public int GetCurrentRound() => currentRound;
}