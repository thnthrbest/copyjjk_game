using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Effect Settings")]
    [Tooltip("Target scale multiplier when hovered")]
    public float hoverScaleMultiplier = 1.05f;

    [Tooltip("Target scale multiplier when clicked")]
    public float clickScaleMultiplier = 0.95f;

    [Tooltip("Duration of the transition animation")]
    public float transitionDuration = 0.1f;

    [Header("Hover Color Effect Settings")]
    [Tooltip("Image component used as the hover background (assign in Inspector)")]
    public Image hoverBackground;

    [Tooltip("Text component whose color changes on hover (assign in Inspector)")]
    public Text buttonText;

    [Tooltip("TMP Text component whose color changes on hover (assign in Inspector, if using TextMeshPro)")]
    public TMPro.TMP_Text buttonTMPText;

    [Tooltip("Color of the hover background")]
    public Color hoverBgColor = Color.white;

    [Tooltip("Text color when hovered")]
    public Color hoverTextColor = Color.black;

    // Cached original colors
    private Color originalTextColor;
    private Color originalTMPTextColor;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private Coroutine colorCoroutine;
    private Button button;

    private void Awake()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();

        // Cache original text colors
        if (buttonText != null)
            originalTextColor = buttonText.color;

        if (buttonTMPText != null)
            originalTMPTextColor = buttonTMPText.color;

        // Hide hover background by default
        if (hoverBackground != null)
        {
            Color c = hoverBgColor;
            c.a = 0f;
            hoverBackground.color = c;
        }
    }

    private void OnDisable()
    {
        // Reset scale and stop coroutines if disabled
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);

        transform.localScale = originalScale;
        SetHoverVisuals(false, instant: true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            StartScaleTransition(originalScale * hoverScaleMultiplier);
            StartColorTransition(hover: true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            StartScaleTransition(originalScale);
            StartColorTransition(hover: false);
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
            bool stillOver = RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera);

            Vector3 targetScale = stillOver ? originalScale * hoverScaleMultiplier : originalScale;
            StartScaleTransition(targetScale);

            if (!stillOver)
                StartColorTransition(hover: false);
        }
    }

    // ─── Scale ────────────────────────────────────────────────────────────────

    private void StartScaleTransition(Vector3 targetScale)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleLerp(targetScale));
    }

    private IEnumerator ScaleLerp(Vector3 targetScale)
    {
        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / transitionDuration);
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }

    // ─── Hover Color / Background ─────────────────────────────────────────────

    private void StartColorTransition(bool hover)
    {
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(ColorLerp(hover));
    }

    private IEnumerator ColorLerp(bool hover)
    {
        float elapsed = 0f;

        // Determine start values
        float startAlpha = hoverBackground != null ? hoverBackground.color.a : 0f;
        float targetAlpha = hover ? 1f : 0f;

        Color startTextColor = Color.white, targetTextColor = Color.white;
        Color startTMPColor = Color.white, targetTMPColor = Color.white;

        if (buttonText != null)
        {
            startTextColor = buttonText.color;
            targetTextColor = hover ? hoverTextColor : originalTextColor;
        }

        if (buttonTMPText != null)
        {
            startTMPColor = buttonTMPText.color;
            targetTMPColor = hover ? hoverTextColor : originalTMPTextColor;
        }

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;

            // Fade hover background
            if (hoverBackground != null)
            {
                Color c = hoverBgColor;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                hoverBackground.color = c;
            }

            // Lerp text colors
            if (buttonText != null)
                buttonText.color = Color.Lerp(startTextColor, targetTextColor, t);

            if (buttonTMPText != null)
                buttonTMPText.color = Color.Lerp(startTMPColor, targetTMPColor, t);

            yield return null;
        }

        // Snap to final values
        SetHoverVisuals(hover, instant: true);
        colorCoroutine = null;
    }

    private void SetHoverVisuals(bool hover, bool instant = false)
    {
        if (hoverBackground != null)
        {
            Color c = hoverBgColor;
            c.a = hover ? 1f : 0f;
            hoverBackground.color = c;
        }

        if (buttonText != null)
            buttonText.color = hover ? hoverTextColor : originalTextColor;

        if (buttonTMPText != null)
            buttonTMPText.color = hover ? hoverTextColor : originalTMPTextColor;
    }
}
