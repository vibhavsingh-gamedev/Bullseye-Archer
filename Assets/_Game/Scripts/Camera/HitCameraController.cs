using UnityEngine;
using System.Collections;

public class HitCameraController : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Main player camera (FPS view)")]
    public Camera playerCamera;

    [Tooltip("Hit camera (replay view)")]
    public Camera hitCamera;

    [Header("Hit Camera Settings")]
    [Tooltip("Arrow se kitni door camera ho")]
    public float distanceFromArrow = 1.2f;

    [Tooltip("Camera kitna upar ho arrow se")]
    public float heightOffset = 0.25f;

    [Tooltip("Camera kitne seconds dikhe replay")]
    public float replayDuration = 4.5f;

    [Header("Smooth Transition")]
    [Tooltip("Initial zoom-in duration (seconds)")]
    public float zoomInDuration = 0.6f;

    [Tooltip("Final zoom-out duration (seconds)")]
    public float zoomOutDuration = 0.4f;

    /*
    [Header("Smooth Transition")]
    [Tooltip("Camera kitni smoothly move kare")]
    public float cameraMoveSpeed = 5f;

    [Tooltip("Camera kitni smoothly rotate kare")]
    public float cameraRotateSpeed = 3f;
    */

    [Header("Cinematic Effects")]
    [Tooltip("Camera arrow ke around ghoome ya nahi")]
    public bool orbitAroundArrow = true;

    [Tooltip("Orbit speed (degrees per second)")]
    public float orbitSpeed = 15f;

    [Tooltip("Orbit starting angle offset")]
    public float orbitStartAngle = -30f;

    [Header("FOV Effects")]
    [Tooltip("Dramatic zoom effect")]
    public bool useFOVZoom = true;

    [Tooltip("Starting FOV (wide)")]
    public float startFOV = 50f;

    [Tooltip("Ending FOV (zoomed in)")]
    public float endFOV = 30f;

    // Singleton pattern - easy access from anywhere
    public static HitCameraController Instance { get; private set; }

    private bool isReplayActive = false;
    private Coroutine activeReplay = null;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initial state - hit camera off, player camera on
        if (hitCamera != null) hitCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
    }

    // Ye method Arrow script call karega jab stick hoga
    public void ShowHitReplay(Transform stuckArrow)
    {
        if (isReplayActive)
        {
            // Already replay chal raha hai, naya start karne se pehle stop karo
            if (activeReplay != null) StopCoroutine(activeReplay);
        }

        activeReplay = StartCoroutine(HitReplayRoutine(stuckArrow));
    }

    IEnumerator HitReplayRoutine(Transform stuckArrow)
    {
        if (stuckArrow == null || hitCamera == null || playerCamera == null)
        {
            Debug.LogError("HitCamera setup incomplete!");
            yield break;
        }

        isReplayActive = true;

        Debug.Log("Hit Replay Started!");

        // Disable player camera control
        CameraController playerCamController = playerCamera.GetComponent<CameraController>();
        if (playerCamController != null) playerCamController.controlEnabled = false;

        // Get initial position from PLAYER camera (smooth transition feel)
        Vector3 initialCameraPos = playerCamera.transform.position;
        Quaternion initialCameraRot = playerCamera.transform.rotation;

        // Switch to hit camera at player's position (no jarring jump!)
        hitCamera.transform.position = initialCameraPos;
        hitCamera.transform.rotation = initialCameraRot;
        hitCamera.fieldOfView = startFOV;

        // Switch to hit camera (Switching Cameras)
        playerCamera.gameObject.SetActive(false);
        hitCamera.gameObject.SetActive(true);

        // Initial position - behind arrow, looking at it
        Vector3 arrowPos = stuckArrow.position;
        Vector3 arrowForward = stuckArrow.forward;

        
        // PHASE 1: SMOOTH ZOOM IN (zoomInDuration)

        Vector3 targetInitialPos = CalculateOrbitPosition(arrowPos, arrowForward, orbitStartAngle);
        Quaternion targetInitialRot = Quaternion.LookRotation(arrowPos - targetInitialPos);

        float zoomTime = 0f;
        Vector3 startPos = hitCamera.transform.position;
        Quaternion startRot = hitCamera.transform.rotation;

        while (zoomTime < zoomInDuration)
        {
            zoomTime += Time.deltaTime;
            float t = zoomTime / zoomInDuration;

            // Ease-in-out for smooth feel
            float easedT = EaseInOutCubic(t);

            // Update arrow reference (in case it moved)
            if (stuckArrow != null)
            {
                arrowPos = stuckArrow.position;
                arrowForward = stuckArrow.forward;
                targetInitialPos = CalculateOrbitPosition(arrowPos, arrowForward, orbitStartAngle);
                targetInitialRot = Quaternion.LookRotation(arrowPos - targetInitialPos);
            }

            hitCamera.transform.position = Vector3.Lerp(startPos, targetInitialPos, easedT);
            hitCamera.transform.rotation = Quaternion.Slerp(startRot, targetInitialRot, easedT);

            // FOV zoom effect
            if (useFOVZoom)
            {
                hitCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, easedT);
            }

            yield return null;
        }

        
        // PHASE 2: CINEMATIC ORBIT (main replay)
        
        float orbitTime = 0f;
        float currentAngle = orbitStartAngle;
        float orbitDuration = replayDuration - zoomInDuration - zoomOutDuration;

        while (orbitTime < orbitDuration)
        {
            if (stuckArrow == null) break;

            orbitTime += Time.deltaTime;

            arrowPos = stuckArrow.position;
            arrowForward = stuckArrow.forward;

            if (orbitAroundArrow)
            {
                currentAngle += orbitSpeed * Time.deltaTime;
            }

            Vector3 targetPos = CalculateOrbitPosition(arrowPos, arrowForward, currentAngle);

            // Smooth lerp for any tiny adjustments
            hitCamera.transform.position = Vector3.Lerp(
                hitCamera.transform.position,
                targetPos,
                Time.deltaTime * 8f
            );

            Quaternion lookRotation = Quaternion.LookRotation(arrowPos - hitCamera.transform.position);
            hitCamera.transform.rotation = Quaternion.Slerp(
                hitCamera.transform.rotation,
                lookRotation,
                Time.deltaTime * 6f
            );

            yield return null;
        }

        
        // PHASE 3: SMOOTH ZOOM OUT (zoomOutDuration)
        
        Vector3 finalCameraPos = playerCamera.transform.position;
        Quaternion finalCameraRot = playerCamera.transform.rotation;

        Vector3 zoomOutStartPos = hitCamera.transform.position;
        Quaternion zoomOutStartRot = hitCamera.transform.rotation;
        float zoomOutTime = 0f;
        float currentFOV = hitCamera.fieldOfView;

        while (zoomOutTime < zoomOutDuration)
        {
            zoomOutTime += Time.deltaTime;
            float t = zoomOutTime / zoomOutDuration;
            float easedT = EaseInOutCubic(t);

            // Update final position (player camera might have moved? No, we disabled it)
            finalCameraPos = playerCamera.transform.position;
            finalCameraRot = playerCamera.transform.rotation;

            hitCamera.transform.position = Vector3.Lerp(zoomOutStartPos, finalCameraPos, easedT);
            hitCamera.transform.rotation = Quaternion.Slerp(zoomOutStartRot, finalCameraRot, easedT);

            if (useFOVZoom)
            {
                hitCamera.fieldOfView = Mathf.Lerp(currentFOV, startFOV, easedT);
            }

            yield return null;
        }

        // Switch back
        hitCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        // SMART CHECK: Only re-enable if game is ACTIVELY playing
        // Don't override GameManager's control during transitions or game over
        if (playerCamController != null)
        {
            bool shouldEnableControl = true;

            // Check if GameManager exists and game is in proper state
            if (GameManager.Instance != null)
            {
                // Only enable if round is ACTIVE (not transition, not game over)
                shouldEnableControl = GameManager.Instance.IsRoundActive() &&
                                      GameManager.Instance.IsGameActive();
            }

            playerCamController.controlEnabled = shouldEnableControl;
            Debug.Log("Replay ended - Camera control: " + (shouldEnableControl ? "ENABLED" : "KEPT DISABLED"));
        }

        isReplayActive = false;
        activeReplay = null;

        Debug.Log("Hit Replay Ended - Back to player view");
    }

    Vector3 CalculateOrbitPosition(Vector3 arrowPos, Vector3 arrowForward, float angle)
    {
        // Calculate position around arrow at given angle
        Vector3 orbitOffset = Quaternion.AngleAxis(angle, Vector3.up)
                              * (-arrowForward * distanceFromArrow);

        return arrowPos + orbitOffset + Vector3.up * heightOffset;
    }

    // Ease-in-out cubic for smooth animations
    float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    // Public method to manually end replay (optional)
    public void EndReplay()
    {
        if (activeReplay != null)
        {
            StopCoroutine(activeReplay);
            activeReplay = null;
        }

        if (hitCamera != null) hitCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        isReplayActive = false;
    }
}