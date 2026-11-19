using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Hover / press feedback for buttons, optionally with a slow idle pulse (used on the Start button).</summary>
public class CyberButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.06f;
    public float pressScale = 0.94f;
    public bool idlePulse = false;

    Vector3 baseScale;
    bool captured, hovering;

    void Capture()
    {
        if (captured) return;
        captured = true;
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        Capture();
        transform.localScale = baseScale;
        if (idlePulse) StartPulse();
    }

    void OnDisable()
    {
        DOTween.Kill(transform);
        transform.localScale = baseScale;
        hovering = false;
    }

    void StartPulse()
    {
        DOTween.Kill(transform);
        transform.localScale = baseScale;
        transform.DOScale(baseScale * 1.045f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    void ScaleTo(float s)
    {
        DOTween.Kill(transform);
        transform.DOScale(baseScale * s, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() =>
        {
            if (idlePulse && !hovering) StartPulse();
        });
    }

    public void OnPointerEnter(PointerEventData e) { hovering = true; ScaleTo(hoverScale); }
    public void OnPointerExit(PointerEventData e)  { hovering = false; ScaleTo(1f); }
    public void OnPointerDown(PointerEventData e)  { ScaleTo(pressScale); }
    public void OnPointerUp(PointerEventData e)    { ScaleTo(hovering ? hoverScale : 1f); }
}
