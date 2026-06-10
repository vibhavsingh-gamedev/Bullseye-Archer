using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("Iss zone ke points")]
    public int scoreValue = 1;

    [Tooltip("Zone ka naam (UI display ke liye)")]
    public string zoneName = "White";

    [Header("Visual Feedback")]
    [Tooltip("Special zone (bullseye) - alag effect")]
    public bool isBullseye = false;

    [Tooltip("Floating text color")]
    public Color textColor = Color.white;

    private void OnDrawGizmos()
    {
        // Editor mein zone visualize karne ke liye
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            Gizmos.color = textColor;
            Gizmos.color = new Color(textColor.r, textColor.g, textColor.b, 0.3f);
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
        }
    }
}