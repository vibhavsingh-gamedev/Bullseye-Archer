using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    public Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    public Vector3 clickScale = new Vector3(0.95f, 0.95f, 0.95f);
    public float animationSpeed = 15f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Smoothly lerp to the target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }

    void OnDisable()
    {
        // Reset scale if button gets disabled
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
}