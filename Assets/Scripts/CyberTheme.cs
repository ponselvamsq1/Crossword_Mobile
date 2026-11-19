using UnityEngine;

/// <summary>Shared palette for the cybersecurity reskin.</summary>
public static class CyberTheme
{
    public static readonly Color Background    = new Color(0.03f, 0.05f, 0.09f, 1f);
    public static readonly Color Cyan          = new Color(0.10f, 0.90f, 1.00f, 1f);
    public static readonly Color Magenta       = new Color(1.00f, 0.20f, 0.70f, 1f);
    public static readonly Color Lime          = new Color(0.45f, 1.00f, 0.55f, 1f);
    public static readonly Color Gold          = new Color(1.00f, 0.82f, 0.25f, 1f);

    // Crossword grid cell tints (multiplied with the white cell sprite).
    public static readonly Color CellNormal     = new Color(0.07f, 0.12f, 0.19f, 1f);
    public static readonly Color CellActiveWord = new Color(0.05f, 0.38f, 0.50f, 1f);
    public static readonly Color CellSelected   = new Color(0.08f, 0.58f, 0.76f, 1f);
    public static readonly Color CellBlocked    = new Color(0.03f, 0.05f, 0.09f, 1f);
    public static readonly Color CellCorrect    = new Color(0.05f, 0.62f, 0.38f, 1f);

    public static readonly Color CellText       = new Color(0.85f, 0.97f, 1.00f, 1f);
    public static readonly Color CellHintText   = Gold;
}
