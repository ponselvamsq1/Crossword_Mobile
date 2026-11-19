using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Slow breathing glow for a UI graphic.</summary>
[RequireComponent(typeof(UnityEngine.UI.Graphic))]
public class CyberPulse : MonoBehaviour
{
    public float minAlpha = 0.35f, maxAlpha = 1f, period = 2.4f;
    UnityEngine.UI.Graphic g;
    void Awake() { g = GetComponent<UnityEngine.UI.Graphic>(); }
    void Update()
    {
        Color c = g.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / period));
        g.color = c;
    }
}
