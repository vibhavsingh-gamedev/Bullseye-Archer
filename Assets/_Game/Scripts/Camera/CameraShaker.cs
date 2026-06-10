using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    private Vector3 originalLocalPos;
    private float shakeIntensity = 0f;
    private float shakeFadeTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeIntensity > 0)
        {
            transform.localPosition = originalLocalPos + Random.insideUnitSphere * shakeIntensity;
            shakeIntensity = Mathf.MoveTowards(shakeIntensity, 0f, Time.deltaTime * shakeFadeTime);
        }
        else
        {
            transform.localPosition = originalLocalPos;
        }
    }

    public void ShakeCamera(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeFadeTime = intensity / duration;
    }
}