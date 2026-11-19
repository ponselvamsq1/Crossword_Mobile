using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates the procedural sprites for the crossword grid cells and (CrosswordCellView loads them from Resources).
/// Run from the menu: Cyber > Build Grid Assets. Safe to run more than once.
/// </summary>
public static class CyberGridAssetBuilder
{
    const string Folder = "Assets/Resources/CyberUI/Grid"; // loaded at runtime by CrosswordCellView

    [MenuItem("Cyber/Build Grid Assets")]
    public static void Build()
    {
        Directory.CreateDirectory(Folder);

        SaveSprite("cell_fill",    DrawRoundRect(64, 6f, 0f, true, 0.0f), 10);
        SaveSprite("cell_frame",   DrawFrame(64, 6f, 2f, true), 10);
        SaveSprite("cell_focus",   DrawFocus(64), 16);
        SaveSprite("cell_blocked", DrawBlocked(64), 10);
        SaveSprite("cell_pip",     DrawCircle(16), 0);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Cyber grid assets built.");
    }

    // ---- sprite output -----------------------------------------------------------------------

    static Sprite SaveSprite(string name, Texture2D tex, int border)
    {
        string path = Folder + "/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spriteBorder = new Vector4(border, border, border, border);
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---- drawing -----------------------------------------------------------------------------

    static float RoundRectDistance(float x, float y, float size, float radius)
    {
        float half = size * 0.5f;
        float qx = Mathf.Abs(x - half) - (half - radius);
        float qy = Mathf.Abs(y - half) - (half - radius);
        float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
        return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
    }

    static Texture2D NewTexture(int size)
    {
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        return t;
    }

    static Texture2D DrawRoundRect(int size, float radius, float unused, bool shade, float unused2)
    {
        Texture2D t = NewTexture(size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = RoundRectDistance(x + 0.5f, y + 0.5f, size, radius);
            float a = Mathf.Clamp01(0.5f - d);
            float v = shade ? Mathf.Lerp(0.82f, 1f, y / (float)(size - 1)) : 1f; // brighter towards the top
            t.SetPixel(x, y, new Color(v, v, v, a));
        }
        t.Apply();
        return t;
    }

    static Texture2D DrawFrame(int size, float radius, float stroke, bool brackets)
    {
        Texture2D t = NewTexture(size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = RoundRectDistance(x + 0.5f, y + 0.5f, size, radius);
            float a = Mathf.Clamp01(0.5f - (Mathf.Abs(d + stroke * 0.5f) - stroke * 0.5f)) * 0.55f;

            if (brackets)
            {
                // brighter, thicker tech-style corner brackets
                float cx = Mathf.Min(x + 0.5f, size - x - 0.5f);
                float cy = Mathf.Min(y + 0.5f, size - y - 0.5f);
                bool onCorner = (cx < 14f && cy < 3.2f) || (cy < 14f && cx < 3.2f);
                if (onCorner && d < 0f) a = Mathf.Max(a, Mathf.Clamp01(0.5f - d) * 1f);
            }
            t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        t.Apply();
        return t;
    }

    static Texture2D DrawFocus(int size)
    {
        Texture2D t = NewTexture(size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = RoundRectDistance(x + 0.5f, y + 0.5f, size, 6f);
            float ring = Mathf.Clamp01(0.5f - (Mathf.Abs(d + 1.8f) - 1.8f));          // crisp 3.6px ring
            float glow = Mathf.Clamp01(1f - Mathf.Abs(d + 4f) / 8f) * 0.45f;           // soft inner glow
            float a = Mathf.Max(ring, d < 0f ? glow : 0f);
            t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        t.Apply();
        return t;
    }

    static Texture2D DrawBlocked(int size)
    {
        Texture2D t = NewTexture(size);
        Color baseColor = new Color(0.035f, 0.06f, 0.10f, 1f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = RoundRectDistance(x + 0.5f, y + 0.5f, size, 4f);
            float a = Mathf.Clamp01(0.5f - d);
            float hatch = ((x + y) % 12 < 1) ? 0.025f : 0f;                              // faint diagonal pattern
            float edge = Mathf.Clamp01(1f - Mathf.Abs(d + 0.8f)) * 0.06f;               // hairline edge
            Color c = new Color(baseColor.r + hatch + edge, baseColor.g + hatch + edge * 1.4f, baseColor.b + hatch + edge * 1.8f, a);
            t.SetPixel(x, y, c);
        }
        t.Apply();
        return t;
    }

    static Texture2D DrawCircle(int size)
    {
        Texture2D t = NewTexture(size);
        float r = size * 0.5f - 1f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = new Vector2(x + 0.5f - size * 0.5f, y + 0.5f - size * 0.5f).magnitude - r;
            t.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(0.5f - d)));
        }
        t.Apply();
        return t;
    }
}
