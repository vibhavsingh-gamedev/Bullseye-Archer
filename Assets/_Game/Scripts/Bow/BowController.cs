using UnityEngine;

public class BowController : MonoBehaviour
{
    [Header("Arrow Setup")]
    [Tooltip("Arrow prefab jo shoot hoga")]
    public GameObject arrowPrefab;

    [Tooltip("Yahan se arrow spawn aur shoot hoga")]
    public Transform arrowSpawnPoint;

    [Tooltip("Visual arrow jo bow pe nocked dikhta hai")]
    public GameObject nockedArrowVisual;

    [Header("Shooting Power")]
    [Tooltip("Minimum power (tap shoot)")]
    public float minPower = 20f;

    [Tooltip("Maximum power (full charge)")]
    public float maxPower = 60f;

    [Tooltip("Power kitni tezi se badhe per second")]
    public float chargeSpeed = 40f;

    [Header("Shooting Cooldown")]
    [Tooltip("Do shots ke beech minimum gap")]
    public float shootCooldown = 0.5f;

    [Header("UI Reference (Optional)")]
    [Tooltip("Power bar UI reference")]
    public UnityEngine.UI.Slider powerBar;

    // Internal variables
    private float currentPower = 0f;
    private bool isCharging = false;
    private float lastShootTime = 0f;

    void Start()
    {
        // Power bar hide karo start mein
        if (powerBar != null)
        {
            powerBar.gameObject.SetActive(false);
            powerBar.minValue = 0;
            powerBar.maxValue = 1;
        }

        // Validations
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow Prefab assign nahi kiya BowController mein!");
        }

        if (arrowSpawnPoint == null)
        {
            Debug.LogError("Arrow Spawn Point assign nahi kiya BowController mein!");
        }
    }

    void Update()
    {
        // Pause check integration
        if (PauseManager.Instance != null && PauseManager.Instance.IsGamePaused())
        {
            return; // Don't process input if the game is paused
        }

        HandleShootingInput();
        UpdatePowerBar();
        HandleNockedArrow();

        // Cooldown ke baad nocked arrow wapas dikhao
        if (nockedArrowVisual != null && !nockedArrowVisual.activeSelf)
        {
            if (Time.time - lastShootTime >= shootCooldown)
            {
                nockedArrowVisual.SetActive(true);
            }
        }
    }

    void HandleShootingInput()
    {
        // Cooldown check
        if (Time.time - lastShootTime < shootCooldown) return;

        // Left mouse button press kiya - charging start
        if (Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }

        // Mouse button hold hai - power badhao
        if (Input.GetMouseButton(0) && isCharging)
        {
            ChargePower();
        }

        // Mouse button release kiya - shoot karo!
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            ShootArrow();
        }
    }

    void StartCharging()
    {
        isCharging = true;
        currentPower = minPower;

        // Power bar show karo
        if (powerBar != null)
        {
            powerBar.gameObject.SetActive(true);
        }

        Debug.Log(" Charging started. Min Power: " + currentPower);

        // Play bow draw sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBowDraw();
        }
    }

    void ChargePower()
    {
        currentPower += chargeSpeed * Time.deltaTime;
        currentPower = Mathf.Clamp(currentPower, minPower, maxPower);
    }

    void ShootArrow()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null)
        {
            Debug.LogError("Cannot shoot - missing references!");
            return;
        }

        // BUG FIX: Power save karo PEHLE reset karne se
        float shootPower = currentPower;

        // Debug PEHLE print karo
        Debug.Log(" Arrow shot with power: " + shootPower);

        // Stop bow draw, play release sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBowDraw();
            AudioManager.Instance.PlayBowRelease();
            AudioManager.Instance.PlayArrowWhoosh();
        }

        // Arrow spawn karo at spawn point position & rotation
        GameObject newArrow = Instantiate(
            arrowPrefab,
            arrowSpawnPoint.position,
            arrowSpawnPoint.rotation
        );

        // Arrow ka Arrow script find karo
        Arrow arrowScript = newArrow.GetComponent<Arrow>();

        if (arrowScript != null)
        {
            // Force calculate karo (forward direction * power)
            Vector3 shootForce = arrowSpawnPoint.forward * shootPower;
            Debug.Log(" Shoot Force: " + shootForce + " | Magnitude: " + shootForce.magnitude);
            arrowScript.Launch(shootForce);
        }
        else
        {
            // Agar Arrow script nahi hai, direct force lagao
            Rigidbody arrowRb = newArrow.GetComponent<Rigidbody>();
            if (arrowRb != null)
            {
                arrowRb.AddForce(arrowSpawnPoint.forward * shootPower, ForceMode.Impulse);
            }
        }

        // Nocked arrow visual hide karo
        if (nockedArrowVisual != null)
        {
            nockedArrowVisual.SetActive(false);
        }

        // Reset (AFTER force applied)
        isCharging = false;
        currentPower = 0f;
        lastShootTime = Time.time;

        // Power bar hide karo
        if (powerBar != null)
        {
            powerBar.gameObject.SetActive(false);
        }

        // Optional: Bow recoil animation yahan add kar sakte hain baad mein
        Debug.Log("Arrow shot with power: " + currentPower);

        // NEW: Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnArrowShot();
        }
    }

    void UpdatePowerBar()
    {
        if (powerBar != null && isCharging)
        {
            // Normalize power (0 to 1) for slider
            float normalizedPower = (currentPower - minPower) / (maxPower - minPower);
            powerBar.value = normalizedPower;
        }
    }

    void HandleNockedArrow()
    {
        if (nockedArrowVisual != null && !nockedArrowVisual.activeSelf)
        {
            if (Time.time - lastShootTime >= shootCooldown)
            {
                nockedArrowVisual.SetActive(true);
            }
        }
    }
}