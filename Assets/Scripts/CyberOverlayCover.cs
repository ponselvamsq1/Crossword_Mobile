using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// While a pause / result overlay is open, hide the clue list so its text cannot show through the overlay panel.
/// Purely visual: the clue text and its scroll position are untouched.
/// </summary>
public class CyberOverlayCover : MonoBehaviour
{
    static int open;

    void OnEnable()  { open++; Apply(0f); }

    void OnDisable()
    {
        open = Mathf.Max(0, open - 1);
        if (open == 0) Apply(1f);
    }

    static void Apply(float alpha)
    {
        foreach (UnityEngine.UI.ScrollRect sr in Resources.FindObjectsOfTypeAll<UnityEngine.UI.ScrollRect>())
        {
            if (!sr.gameObject.scene.IsValid()) continue;
            CanvasGroup g = sr.GetComponent<CanvasGroup>();
            if (g == null) g = sr.gameObject.AddComponent<CanvasGroup>();
            g.alpha = alpha;
        }
    }
}
