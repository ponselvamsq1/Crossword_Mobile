using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Pop / fade / slide-in for UI panels whenever they are enabled. Runs on unscaled time so it also plays while paused.</summary>
[RequireComponent(typeof(CanvasGroup))]
public class CyberPanelPop : MonoBehaviour
{
    [Range(0.5f, 1f)] public float fromScale = 0.88f;
    public float duration = 0.38f;
    public float delay = 0f;
    public bool fadeOnly = false;
    public Vector2 slideFrom = Vector2.zero;

    CanvasGroup group;
    RectTransform rt;
    Vector3 baseScale;
    Vector2 basePos;
    bool captured;

    void Capture()
    {
        if (captured) return;
        captured = true;
        group = GetComponent<CanvasGroup>();
        rt = (RectTransform)transform;
        baseScale = rt.localScale;
        basePos = rt.anchoredPosition;
    }

    void OnEnable()
    {
        Capture();
        KillAndReset();

        group.alpha = 0f;
        group.DOFade(1f, duration * 0.8f).SetDelay(delay).SetUpdate(true).SetTarget(this);
        if (!fadeOnly)
        {
            rt.localScale = baseScale * fromScale;
            rt.DOScale(baseScale, duration).SetDelay(delay).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);
        }
        if (slideFrom != Vector2.zero)
        {
            rt.anchoredPosition = basePos + slideFrom;
            rt.DOAnchorPos(basePos, duration * 1.4f).SetDelay(delay).SetEase(Ease.OutCubic).SetUpdate(true).SetTarget(this);
        }
    }

    void OnDisable()
    {
        if (!captured) return;
        KillAndReset();
        group.alpha = 1f;
    }

    void KillAndReset()
    {
        DOTween.Kill(this);
        DOTween.Kill(group);
        DOTween.Kill(rt);
        rt.localScale = baseScale;
        rt.anchoredPosition = basePos;
    }
}
