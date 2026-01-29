using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RetroPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float scaleAmount = 1.08f;
    [SerializeField] private float pulseSpeed = 2.5f;

    [Header("Alpha Pulse")]
    [SerializeField] private bool pulseAlpha = true;
    [SerializeField] private float minAlpha = 0.75f;

    private RectTransform rect;
    private Vector3 baseScale;
    private Graphic graphic;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;
        graphic = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        rect.localScale = baseScale;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;

        float scale = Mathf.Lerp(1f, scaleAmount, t);
        rect.localScale = baseScale * scale;

        if (pulseAlpha && graphic != null)
        {
            Color c = graphic.color;
            c.a = Mathf.Lerp(minAlpha, 1f, t);
            graphic.color = c;
        }
    }
}
