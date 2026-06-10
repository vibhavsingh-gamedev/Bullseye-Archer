using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    [Tooltip("Arrow ki life - kitne second baad destroy ho")]
    public float arrowLifetime = 10f;

    [Tooltip("Arrow flying ke time tip aage rakhe")]
    public bool rotateInFlight = true;

    [Header("Hit Effects")]
    [Tooltip("Hit hone par arrow stick ho jaye")]
    public bool stickOnHit = true;

    [Tooltip("Stuck arrow kitni der target pe rahe")]
    public float stickDuration = 10f;

    [Header("VFX")]
    public GameObject hitParticlePrefab;

    // Internal variables
    private Rigidbody rb;
    private Collider arrowCollider;
    private bool hasHit = false;
    private bool isFlying = false;
    private bool isDisplayOnly;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        arrowCollider = GetComponent<Collider>();
    }

    void Start()
    {
        // Lifetime ke baad destroy
        Destroy(gameObject, arrowLifetime);
    }

    void FixedUpdate()
    {
        // Arrow ko flying direction ki taraf rotate karte raho (realistic flight)
        if (isFlying && !hasHit && rotateInFlight && rb != null)
        {
            if (rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
            }
        }
    }

    // Ye method BowController call karega arrow shoot karte time
    public void Launch(Vector3 force)
    {
        if (rb != null)
        {
            isFlying = true;
            
            // Reset velocity first
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Apply force
            rb.AddForce(force, ForceMode.Impulse);

            Debug.Log("Arrow Launched! Force: " + force.magnitude);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDisplayOnly) return;
        if (hasHit) return;

        hasHit = true;
        isFlying = false;

        Debug.Log("Arrow hit: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag);

        // Get exact hit position (more accurate)
        Vector3 hitPosition = collision.contacts[0].point;

        if (hitParticlePrefab != null)
        {
            Debug.Log("Particle Spawned!");

            GameObject particles = Instantiate(
                hitParticlePrefab,
                hitPosition,
                Quaternion.identity
            );

            Destroy(particles, 2f);
        }

        // NEW: Check if hit object has TargetScoring
        TargetScoring targetScoring = collision.gameObject.GetComponentInParent<TargetScoring>();
        if (targetScoring != null)
        {
            // Calculate score based on distance from center
            ScoreResult result = targetScoring.CalculateScore(hitPosition);

            if (AudioManager.Instance != null)
            {
                if (result.isBullseye)
                {
                    AudioManager.Instance.PlayBullseye();

                    // Trigger Camera Shake on Bullseye
                    if (CameraShaker.Instance != null)
                    {
                        CameraShaker.Instance.ShakeCamera(0.2f, 0.15f); // intensity = 0.2, duration = 0.15s
                    }
                }
                else
                {
                    AudioManager.Instance.PlayArrowHit();
                }
            }

            // Play appropriate hit sound
            // (3D sound):
            //if (AudioManager.Instance != null && AudioManager.Instance.arrowHitSound != null)
            //{
            //    AudioSource.PlayClipAtPoint(
            //        AudioManager.Instance.arrowHitSound,
            //        hitPosition,
            //        AudioManager.Instance.sfxVolume
            //    );
            //}

            // Send to ScoreManager
            if (ScoreManager.Instance != null && result.points > 0)
            {
                ScoreManager.Instance.AddScore(
                    result.points,
                    result.zoneName,
                    hitPosition,
                    result.zoneColor,
                    result.isBullseye
                );
            }
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayArrowHit();
            }

            // NEW: Play hit sound for non-target hits too
            //if (AudioManager.Instance != null && AudioManager.Instance.arrowHitSound != null)
            //{
            //    AudioSource.PlayClipAtPoint(
            //        AudioManager.Instance.arrowHitSound,
            //        hitPosition,
            //        AudioManager.Instance.sfxVolume
            //    );
            //}
        }

        if (stickOnHit)
        {
            StickToObject(collision);
        }
    }

    /*
    void OnTriggerEnter(Collider other)
    {
        // Display arrows ko skip karo
        if (isDisplayOnly) return;

        // Agar already hit ho gaya, skip
        if (hasHit) return;

        // Check if it's a score zone
        ScoreZone zone = other.GetComponent<ScoreZone>();
        if (zone != null)
        {
            Debug.Log("Hit zone: " + zone.zoneName + " | Score: " + zone.scoreValue);

            // Add score via ScoreManager
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(
                    zone.scoreValue,
                    zone.zoneName,
                    transform.position,
                    zone.textColor,
                    zone.isBullseye
                );
            }
        }
    }
    */

    void StickToObject(Collision collision)
    {
        // Stop all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable collider (no more collisions)
        if (arrowCollider != null)
        {
            arrowCollider.enabled = false;
        }

        // Parent to hit object (so arrow moves with target if it moves)
        transform.SetParent(collision.transform);

        Debug.Log("Arrow stuck on: " + collision.gameObject.name);

        // Trigger hit replay camera
        if (HitCameraController.Instance != null)
        {
            HitCameraController.Instance.ShowHitReplay(transform);
        }

        // Extend lifetime so player can see arrows stuck
        Destroy(gameObject, stickDuration);
    }
}