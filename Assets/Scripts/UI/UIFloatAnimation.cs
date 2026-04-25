using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIFloatAnimation : MonoBehaviour
{
    [Header("Float (bob up/down)")]
    public bool  floatEnabled  = true;
    public float floatHeight   = 12f;   // pixels
    public float floatSpeed    = 1.4f;

    [Header("Pulse (breathe scale)")]
    public bool  pulseEnabled  = true;
    public float pulseAmount   = 0.06f; // fraction of base scale
    public float pulseSpeed    = 1.1f;

    [Header("Wobble (gentle tilt)")]
    public bool  wobbleEnabled = true;
    public float wobbleAngle   = 4f;    // degrees
    public float wobbleSpeed   = 0.9f;

    [Header("Shimmer (color brightness pulse)")]
    public bool  shimmerEnabled = true;
    public float shimmerAmount  = 0.15f;
    public float shimmerSpeed   = 2.2f;

    [Header("Phase offset (0–1 to stagger multiple instances)")]
    [Range(0f, 1f)]
    public float phaseOffset = 0f;

    // -------------------------------------------------------

    private RectTransform _rt;
    private Graphic       _graphic;
    private Vector2       _basePos;
    private Vector3       _baseScale;
    private Color         _baseColor;

    private void Awake()
    {
        _rt        = GetComponent<RectTransform>();
        _graphic   = GetComponent<Graphic>();
        _basePos   = _rt.anchoredPosition;
        _baseScale = _rt.localScale;
        _baseColor = _graphic != null ? _graphic.color : Color.white;
    }

    private void Update()
    {
        float t = Time.time + phaseOffset * Mathf.PI * 2f;

        // --- float ---
        Vector2 pos = _basePos;
        if (floatEnabled)
            pos.y += Mathf.Sin(t * floatSpeed) * floatHeight;
        _rt.anchoredPosition = pos;

        // --- pulse ---
        Vector3 scale = _baseScale;
        if (pulseEnabled)
        {
            float s = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
            scale = _baseScale * s;
        }
        _rt.localScale = scale;

        // --- wobble ---
        float angle = 0f;
        if (wobbleEnabled)
            angle = Mathf.Sin(t * wobbleSpeed) * wobbleAngle;
        _rt.localRotation = Quaternion.Euler(0f, 0f, angle);

        // --- shimmer ---
        if (shimmerEnabled && _graphic != null)
        {
            float brightness = 1f + Mathf.Sin(t * shimmerSpeed) * shimmerAmount;
            _graphic.color = new Color(
                Mathf.Clamp01(_baseColor.r * brightness),
                Mathf.Clamp01(_baseColor.g * brightness),
                Mathf.Clamp01(_baseColor.b * brightness),
                _baseColor.a);
        }
    }

    private void OnDisable()
    {
        // restore to base state when disabled
        if (_rt != null)
        {
            _rt.anchoredPosition = _basePos;
            _rt.localScale       = _baseScale;
            _rt.localRotation    = Quaternion.identity;
        }
        if (_graphic != null)
            _graphic.color = _baseColor;
    }
}
