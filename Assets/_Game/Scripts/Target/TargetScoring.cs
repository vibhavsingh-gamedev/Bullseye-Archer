using UnityEngine;

public class TargetScoring : MonoBehaviour
{
    [Header("Target Center Reference")]
    [Tooltip("Target ka exact center point")]
    public Transform targetCenter;

    [Header("Scoring Zones (Distance from center)")]
    [Tooltip("Bullseye max distance (smallest)")]
    public float bullseyeRadius = 0.08f;

    [Tooltip("Yellow ring max distance")]
    public float yellowRadius = 0.15f;

    [Tooltip("Red ring max distance")]
    public float redRadius = 0.25f;

    [Tooltip("Blue ring max distance")]
    public float blueRadius = 0.35f;

    [Tooltip("Black ring max distance")]
    public float blackRadius = 0.45f;

    [Tooltip("White ring max distance (largest)")]
    public float whiteRadius = 0.55f;

    [Header("Visual Debug")]
    [Tooltip("Editor mein zones dikhao")]
    public bool showGizmos = true;

    [Tooltip("Gizmo line thickness")]
    [Range(0.001f, 0.05f)]
    public float gizmoLineWidth = 0.005f;

    // Calculate score based on hit position (LOCAL SPACE)
    public ScoreResult CalculateScore(Vector3 worldHitPosition)
    {
        if (targetCenter == null)
        {
            Debug.LogError("Target Center not assigned!");
            return new ScoreResult(0, "Miss", Color.gray, false);
        }

        // FIX: Convert world position to TARGET's LOCAL space
        Vector3 localHitPos = targetCenter.InverseTransformPoint(worldHitPosition);

        // Use LOCAL X and Y (target face plane)
        // Z represents depth (penetration into target) - ignore karenge
        Vector2 hit2D = new Vector2(localHitPos.x, localHitPos.y);

        // Distance from local center (0,0)
        float distance = hit2D.magnitude;

        Debug.Log("Local Hit Distance: " + distance.ToString("F3") +
                  " | Local Pos: " + localHitPos.ToString("F3"));

        // Determine zone based on distance
        if (distance <= bullseyeRadius)
            return new ScoreResult(10, "BULLSEYE", Color.yellow, true);
        else if (distance <= yellowRadius)
            return new ScoreResult(9, "Yellow", new Color(1f, 0.92f, 0f), false);
        else if (distance <= redRadius)
            return new ScoreResult(7, "Red", new Color(1f, 0.2f, 0.2f), false);
        else if (distance <= blueRadius)
            return new ScoreResult(5, "Blue", new Color(0.4f, 0.6f, 1f), false);
        else if (distance <= blackRadius)
            return new ScoreResult(3, "Black", new Color(0.3f, 0.3f, 0.3f), false);
        else if (distance <= whiteRadius)
            return new ScoreResult(1, "White", Color.white, false);
        else
            return new ScoreResult(0, "Miss", Color.gray, false);
    }

    // UPDATED: Gizmos draw karte hain target ke LOCAL space mein (slant follows)
    private void OnDrawGizmos()
    {
        if (!showGizmos || targetCenter == null) return;

        // Save old matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // KEY: Use target's transformation matrix
        // Now gizmos rotate WITH the target
        Gizmos.matrix = targetCenter.localToWorldMatrix;

        // Draw circles in LOCAL X-Y plane (target face)
        // Bullseye - Yellow
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        DrawLocalCircle(bullseyeRadius, 64);

        // Yellow ring
        Gizmos.color = new Color(1f, 0.92f, 0f, 0.6f);
        DrawLocalCircle(yellowRadius, 64);

        // Red ring
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        DrawLocalCircle(redRadius, 64);

        // Blue ring
        Gizmos.color = new Color(0.4f, 0.6f, 1f, 0.6f);
        DrawLocalCircle(blueRadius, 64);

        // Black ring
        Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        DrawLocalCircle(blackRadius, 64);

        // White ring
        Gizmos.color = new Color(1f, 1f, 1f, 0.8f);
        DrawLocalCircle(whiteRadius, 64);

        // Restore matrix
        Gizmos.matrix = oldMatrix;
    }

    // Draw circle in LOCAL X-Y plane (Z = 0 is target face)
    void DrawLocalCircle(float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0  // Local Z = 0 (target face plane)
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}

// Helper class to return score data
[System.Serializable]
public class ScoreResult
{
    public int points;
    public string zoneName;
    public Color zoneColor;
    public bool isBullseye;

    public ScoreResult(int points, string zoneName, Color zoneColor, bool isBullseye)
    {
        this.points = points;
        this.zoneName = zoneName;
        this.zoneColor = zoneColor;
        this.isBullseye = isBullseye;
    }
}