using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Effect settings")]
    [Tooltip("Target scale multiplier when hovered")]
    public float hoverScaleMultiplier = 1.05f;

    [Tooltip("Target scale multiplier when clicked")]
    public float clickScaleMultiplier = 0.95f;

    [Tooltip("Duration of the transition animation")]
    public float transitionDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private Button button;

    private void Awake()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
    }

    private void OnDisable()
    {
        // Reset scale and stop coroutines if disabled
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        transform.localScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            StartScaleTransition(originalScale * hoverScaleMultiplier);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            StartScaleTransition(originalScale);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            StartScaleTransition(originalScale * clickScaleMultiplier);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            // If pointer is still over the button, scale back to hover scale, else original scale
            Vector3 targetScale = RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(), 
                eventData.position, 
                eventData.pressEventCamera) ? originalScale * hoverScaleMultiplier : originalScale;
            
            StartScaleTransition(targetScale);
        }
    }

    private void StartScaleTransition(Vector3 targetScale)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleLerp(targetScale));
    }

    private IEnumerator ScaleLerp(Vector3 targetScale)
    {
        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = elapsed / transitionDuration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, percent);
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}
