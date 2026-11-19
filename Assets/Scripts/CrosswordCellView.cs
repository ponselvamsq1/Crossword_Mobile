using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual states for one crossword grid cell (blocked, idle, selected, filled).
/// Lives on the runtime cells; CellData and the managers keep all selection, typing and validation logic.
/// </summary>
[RequireComponent(typeof(CellData))]
public class CrosswordCellView : MonoBehaviour
{
    // Overlay graphics, created at runtime as the last children of the cell (drawn above the letter).
    Image frame, focusRing, filledPip, blockedMask;

    static readonly Color FrameIdle = new Color(0.10f, 0.90f, 1f, 0.35f);

    CellData data;
    Button button;
    int lastStateKey = -1;

    static Sprite fillSprite, frameSprite, focusSprite, blockedSprite, pipSprite;

    static Sprite Load(string name)
    {
        return Resources.Load<Sprite>("CyberUI/Grid/" + name);
    }

    static Image Overlay(Transform parent, string name, Sprite sprite, Image.Type type, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = type;
        img.color = color;
        img.raycastTarget = false;
        go.SetActive(false);
        return img;
    }

    void BuildOverlays()
    {
        if (fillSprite == null)
        {
            fillSprite = Load("cell_fill"); frameSprite = Load("cell_frame"); focusSprite = Load("cell_focus");
            blockedSprite = Load("cell_blocked"); pipSprite = Load("cell_pip");
        }

        data.bgImage.sprite = fillSprite;
        data.bgImage.type = Image.Type.Sliced;

        frame       = Overlay(transform, "CellFrame",   frameSprite,   Image.Type.Sliced, FrameIdle);
        blockedMask = Overlay(transform, "BlockedMask", blockedSprite, Image.Type.Sliced, Color.white);
        focusRing   = Overlay(transform, "FocusRing",   focusSprite,   Image.Type.Sliced, CyberTheme.Cyan);

        filledPip = Overlay(transform, "FilledPip", pipSprite, Image.Type.Simple, CyberTheme.Cyan);
        RectTransform pr = filledPip.rectTransform;
        pr.anchorMin = pr.anchorMax = new Vector2(1f, 0f);
        pr.pivot = new Vector2(1f, 0f);
        pr.sizeDelta = new Vector2(5f, 5f);
        pr.anchoredPosition = new Vector2(-4f, 4f);
    }

    void Awake()
    {
        data = GetComponent<CellData>();
        button = GetComponent<Button>();
        BuildOverlays();

        // The cell is driven by its own colours and overlays, not the button's tint.
        button.transition = Selectable.Transition.None;

        data.bgImage.color = CyberTheme.CellNormal;
        data.numberText.color = CyberTheme.Cyan;
        data.letterText.color = CyberTheme.CellText;
        data.letterText.fontStyle |= TMPro.FontStyles.UpperCase; // display only; the stored text is untouched
    }

    void Update()
    {
        bool blocked = !button.interactable;
        bool filled = !blocked && !string.IsNullOrEmpty(data.letterText.text);
        bool selected = !blocked && data.IsSelected;
        int key = (blocked ? 1 : 0) | (filled ? 2 : 0) | (selected ? 4 : 0);
        if (key == lastStateKey) return;
        lastStateKey = key;

        blockedMask.gameObject.SetActive(blocked);
        frame.gameObject.SetActive(!blocked);
        focusRing.gameObject.SetActive(selected);
        filledPip.gameObject.SetActive(filled && !selected);
    }
}
