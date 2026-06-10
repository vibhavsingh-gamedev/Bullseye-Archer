using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Display")]
    [Tooltip("Total score show karne wala text")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Last hit zone name")]
    public TextMeshProUGUI lastHitText;

    [Header("Floating Score Text")]
    [Tooltip("Floating '+10' text prefab")]
    public GameObject floatingTextPrefab;

    [Tooltip("Floating text canvas")]
    public Canvas floatingTextCanvas;

    [Header("Animations")]
    [Tooltip("Score pop animation duration")]
    public float scorePopDuration = 0.5f;

    [Tooltip("Score text pop scale")]
    public float scorePopScale = 1.3f;

    // Singleton pattern
    public static ScoreManager Instance { get; private set; }

    // Game state
    private int totalScore = 0;
    private int totalShots = 0;
    private int bullseyeCount = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();

        if (lastHitText != null)
            lastHitText.text = "";
    }

    // Ye method Arrow script call karega jab arrow zone mein lage
    public void AddScore(int points, string zoneName, Vector3 worldPosition, Color textColor, bool isBullseye)
    {
        totalScore += points;
        totalShots++;

        if (isBullseye)
            bullseyeCount++;

        // UI update with pop effect
        UpdateScoreUI();
        StartCoroutine(ScorePopEffect());

        // Last hit text update
        if (lastHitText != null)
        {
            lastHitText.text = zoneName + " +" + points;
            lastHitText.color = textColor;
            StartCoroutine(FadeOutLastHit());
        }

        // Floating text spawn karo target pe
        SpawnFloatingText(points, worldPosition, textColor, isBullseye);

        // Console debug
        if (isBullseye)
            Debug.Log("BULLSEYE! +" + points + " | Total: " + totalScore);
        else
            Debug.Log("-" + zoneName + " Hit! +" + points + " | Total: " + totalScore);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore;
        }
    }

    IEnumerator ScorePopEffect()
    {
        if (scoreText == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 popScale = Vector3.one * scorePopScale;

        float halfDuration = scorePopDuration / 2f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            scoreText.transform.localScale = Vector3.Lerp(originalScale, popScale, t);
            yield return null;
        }

        // Scale back
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            scoreText.transform.localScale = Vector3.Lerp(popScale, originalScale, t);
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }

    IEnumerator FadeOutLastHit()
    {
        if (lastHitText == null) yield break;

        yield return new WaitForSeconds(2f);

        float fadeDuration = 1f;
        float elapsed = 0f;
        Color originalColor = lastHitText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            Color c = originalColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            lastHitText.color = c;
            yield return null;
        }

        lastHitText.text = "";
        Color finalColor = lastHitText.color;
        finalColor.a = 1f;
        lastHitText.color = finalColor;
    }

    void SpawnFloatingText(int points, Vector3 worldPosition, Color color, bool isBullseye)
    {
        if (floatingTextPrefab == null || floatingTextCanvas == null) return;

        // World position to screen position
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPosition);

        // Agar arrow camera ke peeche hai, skip karo
        if (screenPos.z < 0) return;

        // Floating text spawn
        GameObject floatingText = Instantiate(floatingTextPrefab, floatingTextCanvas.transform);
        floatingText.transform.position = screenPos;

        // Configure text
        TextMeshProUGUI tmp = floatingText.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = isBullseye ? "BULLSEYE!\n+" + points : "+" + points;
            tmp.color = color;
            tmp.fontSize = isBullseye ? 70 : 50;
        }

        // Animate
        StartCoroutine(AnimateFloatingText(floatingText));
    }

    IEnumerator AnimateFloatingText(GameObject textObj)
    {
        if (textObj == null) yield break;

        float duration = 1.5f;
        float elapsed = 0f;

        Vector3 startPos = textObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 150f;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        Color originalColor = tmp != null ? tmp.color : Color.white;

        while (elapsed < duration)
        {
            if (textObj == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Move up
            textObj.transform.position = Vector3.Lerp(startPos, endPos, t);

            // Scale up then down
            float scale = Mathf.Sin(t * Mathf.PI) * 0.5f + 1f;
            textObj.transform.localScale = Vector3.one * scale;

            // Fade out
            if (tmp != null)
            {
                Color c = originalColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                tmp.color = c;
            }

            yield return null;
        }

        Destroy(textObj);
    }

    // Public getters
    public int GetTotalScore() => totalScore;
    public int GetTotalShots() => totalShots;
    public int GetBullseyeCount() => bullseyeCount;

    public void ResetScore()
    {
        totalScore = 0;
        totalShots = 0;
        bullseyeCount = 0;
        UpdateScoreUI();
    }
}