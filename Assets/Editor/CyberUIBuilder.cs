using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates every procedural sprite of the cybersecurity reskin into Assets/UI_Cyber.
/// Menu: Cyber > Build UI Sprites. Deterministic (fixed seeds), safe to re-run.
/// </summary>
public static class CyberUIBuilder
{
    public const string Folder = "Assets/UI_Cyber";

    static readonly Color Cyan    = new Color(0.10f, 0.90f, 1.00f, 1f);
    static readonly Color Magenta = new Color(1.00f, 0.25f, 0.72f, 1f);
    static readonly Color Lime    = new Color(0.45f, 1.00f, 0.55f, 1f);
    static readonly Color Gold    = new Color(1.00f, 0.82f, 0.25f, 1f);
    static readonly Color Ink0    = new Color(0.050f, 0.095f, 0.165f, 1f);
    static readonly Color Ink1    = new Color(0.025f, 0.050f, 0.100f, 1f);

    [MenuItem("Cyber/Build UI Sprites")]
    public static void Build()
    {
        Directory.CreateDirectory(Folder);

        Save("bg_game",   BackgroundGame(),  Vector4.zero, 100f, null);
        Save("bg_intro",  BackgroundIntro(), Vector4.zero, 100f, null);
        Save("floor_grid", FloorGrid(),      Vector4.zero, 100f, "Assets/AnimationIMG/NewAnimation/wame dead/wame grass.png");
        Save("web_cyber", WebNetwork(),      Vector4.zero, 100f, "Assets/AnimationIMG/NewAnimation/web.png");
        Save("tree_cyber", CircuitTree(),    Vector4.zero, 100f, "Assets/AnimationIMG/NewAnimation/bug dead/bug tree.png");

        Save("panel_glass",    PanelGlass(),    new Vector4(44, 44, 44, 44), 100f, null);
        Save("button_primary", ButtonPrimary(), new Vector4(30, 30, 30, 30), 100f, null);
        Save("timer_plate",    TimerPlate(),    Vector4.zero, 100f, null);
        Save("star_bar",       StarBar(),       Vector4.zero, 100f, null);

        Save("icon_pause",  Icon(Cyan,    IconGlyph.Pause),  Vector4.zero, 100f, null);
        Save("icon_resume", Icon(Lime,    IconGlyph.Resume), Vector4.zero, 100f, null);
        Save("icon_next",   Icon(Lime,    IconGlyph.Next),   Vector4.zero, 100f, null);
        Save("icon_replay", Icon(Cyan,    IconGlyph.Replay), Vector4.zero, 100f, null);
        Save("icon_levels", Icon(Cyan,    IconGlyph.Levels), Vector4.zero, 100f, null);
        Save("icon_exit",   Icon(Magenta, IconGlyph.Exit),   Vector4.zero, 100f, null);
        Save("icon_hint",   Icon(Gold,    IconGlyph.Hint),   Vector4.zero, 100f, null);
        Save("icon_shield", ShieldIcon(),   Vector4.zero, 100f, null);

        Save("star_filled", Star(true),  Vector4.zero, 100f, null);
        Save("star_empty",  Star(false), Vector4.zero, 100f, null);

        Save("card_easy",   Card(Lime,    12, 11), Vector4.zero, 100f, null);
        Save("card_medium", Card(Gold,    15, 23), Vector4.zero, 100f, null);
        Save("card_hard",   Card(Magenta, 18, 37), Vector4.zero, 100f, null);

        Save("fx_glow",   FxGlow(),   Vector4.zero, 100f, null);
        Save("fx_ring",   FxRing(),   Vector4.zero, 100f, null);
        Save("fx_spark",  FxSpark(),  Vector4.zero, 100f, null);
        Save("fx_square", FxSquare(), Vector4.zero, 100f, null);
        Save("fx_plate",  HoloPlate(), Vector4.zero, 100f, null);
        Save("fx_rays",   Rays(),     Vector4.zero, 100f, null);
        Save("hud_ring",  HudRing(),  Vector4.zero, 100f, null);

        AssetDatabase.Refresh();
        Debug.Log("Cyber UI sprites built.");
    }

    // ---- backgrounds -------------------------------------------------------------------------

    static CyberImg BaseBackground(int seed, float glowX, float glowY)
    {
        const int W = 1920, H = 1080;
        CyberImg img = new CyberImg(W, H, Color.black);
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            float t = y / (float)H;
            float dx = (x - W * 0.5f) / (W * 0.5f), dy = (y - H * 0.5f) / (H * 0.5f);
            float vig = 1f - 0.5f * Mathf.Pow(Mathf.Clamp01((dx * dx + dy * dy) * 0.55f), 1.4f);
            Color c = Color.Lerp(new Color(0.020f, 0.038f, 0.085f), new Color(0.035f, 0.075f, 0.140f), t) * vig;
            float n = (Mathf.PerlinNoise(x * 0.004f + seed, y * 0.004f) - 0.5f) * 0.02f + (Random.value - 0.5f) * 0.006f;
            img.P[y * W + x] = new Color(Mathf.Max(0, c.r + n), Mathf.Max(0, c.g + n), Mathf.Max(0, c.b + n * 1.3f), 1f);
        }

        img.Glow(W * glowX, H * glowY, 900f, new Color(0.05f, 0.45f, 0.65f, 1f), 0.20f);
        img.Glow(W * (1f - glowX), H * (1f - glowY), 700f, new Color(0.45f, 0.10f, 0.50f, 1f), 0.12f);

        Random.InitState(seed);

        // faint hexagon lattice
        float s = 54f;
        for (float cy = -s; cy < H + s; cy += s * 1.5f)
        {
            int row = Mathf.RoundToInt(cy / (s * 1.5f));
            for (float cx = -s; cx < W + s; cx += s * 1.7320508f)
            {
                float ox = (row % 2 == 0) ? 0f : s * 0.8660254f;
                if (Random.value < 0.42f) continue;
                Vector2[] hex = new Vector2[6];
                for (int i = 0; i < 6; i++)
                {
                    float a = (60f * i - 30f) * Mathf.Deg2Rad;
                    hex[i] = new Vector2(cx + ox + s * 0.92f * Mathf.Cos(a), cy + s * 0.92f * Mathf.Sin(a));
                }
                float alpha = Random.Range(0.03f, 0.07f);
                for (int i = 0; i < 6; i++) img.Seg(hex[i].x, hex[i].y, hex[(i + 1) % 6].x, hex[(i + 1) % 6].y, 1.2f, new Color(0.2f, 0.8f, 1f, alpha));
            }
        }

        // circuit traces with pads
        float[] dirs = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        for (int k = 0; k < 46; k++)
        {
            Vector2 p = new Vector2(Random.Range(0, W), Random.Range(0, H));
            float dir = dirs[Random.Range(0, dirs.Length)];
            Color col = Random.value < 0.8f ? Cyan : (Random.value < 0.5f ? Magenta : Lime);
            float a = Random.Range(0.07f, 0.17f);
            int segs = Random.Range(3, 7);
            img.Disc(p.x, p.y, 4.5f, CyberImg.A(col, a * 1.6f));
            for (int i = 0; i < segs; i++)
            {
                float len = Random.Range(60f, 220f);
                Vector2 q = p + new Vector2(Mathf.Cos(dir * Mathf.Deg2Rad), Mathf.Sin(dir * Mathf.Deg2Rad)) * len;
                img.Seg(p.x, p.y, q.x, q.y, 3.4f, CyberImg.A(col, a * 0.35f));
                img.Seg(p.x, p.y, q.x, q.y, 1.5f, CyberImg.A(col, a));
                p = q;
                dir = Mathf.Repeat(dir + (Random.value < 0.5f ? 45f : -45f), 360f);
            }
            img.Ring(p.x, p.y, 7f, 2f, CyberImg.A(col, a * 1.7f));
            img.Glow(p.x, p.y, 26f, col, a * 1.4f);
        }

        // falling data bits
        for (int k = 0; k < 90; k++)
        {
            float x = Random.Range(0, W), y0 = Random.Range(-100, H);
            int n = Random.Range(4, 16);
            for (int i = 0; i < n; i++)
            {
                float a = Mathf.Lerp(0.03f, 0.13f, i / (float)n);
                int w = Random.value < 0.5f ? 2 : 4;
                for (int yy = 0; yy < 7; yy++) for (int xx = 0; xx < w; xx++) img.Over((int)x + xx, (int)(y0 + i * 14) + yy, new Color(0.3f, 0.95f, 1f, a));
            }
        }
        return img;
    }

    static CyberImg BackgroundGame() { return BaseBackground(7, 0.12f, 0.95f); }

    static CyberImg BackgroundIntro()
    {
        CyberImg img = BaseBackground(21, 0.25f, 0.55f);
        float cx = 520f, cy = 560f;

        img.Glow(cx, cy, 620f, new Color(0.05f, 0.55f, 0.8f, 1f), 0.30f);

        // radar sweep
        for (int y = 0; y < img.H; y++)
        for (int x = 0; x < img.W; x++)
        {
            float dx = x - cx, dy = y - cy;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            if (r > 440f || r < 20f) continue;
            float ang = Mathf.Repeat(Mathf.Atan2(dy, dx) * Mathf.Rad2Deg + 25f, 360f);
            if (ang < 70f) img.Over(x, y, new Color(0.1f, 0.9f, 1f, 0.16f * Mathf.Pow(1f - ang / 70f, 2f) * (1f - r / 460f)));
        }

        foreach (float r in new[] { 140f, 210f, 290f, 370f, 440f })
            img.Ring(cx, cy, r, 1.6f, new Color(0.15f, 0.9f, 1f, 0.28f));
        for (int i = 0; i < 6; i++) img.Arc(cx, cy, 175f, 5f, i * 60f + 8f, i * 60f + 38f, new Color(0.15f, 0.9f, 1f, 0.55f));
        for (int i = 0; i < 4; i++) img.Arc(cx, cy, 330f, 4f, i * 90f + 20f, i * 90f + 64f, new Color(1f, 0.25f, 0.72f, 0.5f));
        for (int i = 0; i < 72; i++)
        {
            float a = i * 5f * Mathf.Deg2Rad;
            float len = i % 6 == 0 ? 26f : 11f;
            img.Seg(cx + Mathf.Cos(a) * 452f, cy + Mathf.Sin(a) * 452f, cx + Mathf.Cos(a) * (452f + len), cy + Mathf.Sin(a) * (452f + len), 2f, new Color(0.2f, 0.9f, 1f, i % 6 == 0 ? 0.6f : 0.28f));
        }
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f * Mathf.Deg2Rad;
            img.Seg(cx + Mathf.Cos(a) * 150f, cy + Mathf.Sin(a) * 150f, cx + Mathf.Cos(a) * 436f, cy + Mathf.Sin(a) * 436f, 1.2f, new Color(0.2f, 0.9f, 1f, 0.10f));
        }

        // orbiting nodes
        float[][] nodes = { new[] { 210f, 25f }, new[] { 290f, 140f }, new[] { 370f, 215f }, new[] { 290f, 300f }, new[] { 440f, 330f }, new[] { 210f, 250f } };
        foreach (float[] n in nodes)
        {
            float a = n[1] * Mathf.Deg2Rad;
            float x = cx + Mathf.Cos(a) * n[0], y = cy + Mathf.Sin(a) * n[0];
            img.Glow(x, y, 40f, Cyan, 0.7f);
            img.Disc(x, y, 7f, new Color(0.85f, 1f, 1f, 1f));
        }

        // central shield with a lock
        float R = 105f;
        Vector2[] shield =
        {
            new Vector2(cx, cy - R), new Vector2(cx + R * 0.85f, cy - R * 0.62f), new Vector2(cx + R * 0.85f, cy + R * 0.08f),
            new Vector2(cx + R * 0.48f, cy + R * 0.66f), new Vector2(cx, cy + R), new Vector2(cx - R * 0.48f, cy + R * 0.66f),
            new Vector2(cx - R * 0.85f, cy + R * 0.08f), new Vector2(cx - R * 0.85f, cy - R * 0.62f)
        };
        img.Glow(cx, cy, 190f, Cyan, 0.55f);
        img.Poly(shield, new Color(0.03f, 0.10f, 0.17f, 0.95f), Cyan, 5f);
        img.Arc(cx, cy - 12f, 26f, 8f, 180f, 360f, Cyan);
        img.Seg(cx - 26f, cy - 12f, cx - 26f, cy + 2f, 8f, Cyan);
        img.Seg(cx + 26f, cy - 12f, cx + 26f, cy + 2f, 8f, Cyan);
        img.RRect(cx - 38f, cy - 2f, cx + 38f, cy + 46f, 8f, Cyan, new Color(0.05f, 0.7f, 0.9f, 1f), Color.clear, 0f);
        img.Disc(cx, cy + 18f, 7f, Ink1);
        img.Seg(cx, cy + 20f, cx, cy + 34f, 6f, Ink1);

        // bottom scan strip
        for (int x = 0; x < img.W; x += 6) img.Over(x, img.H - 90, new Color(0.2f, 0.9f, 1f, 0.12f));
        return img;
    }

    static CyberImg FloorGrid()
    {
        const int W = 1920, H = 500;
        CyberImg img = new CyberImg(W, H, Color.clear);
        float horizon = 250f, vx = W * 0.5f;
        img.Glow(vx, horizon, 900f, new Color(0.05f, 0.55f, 0.85f, 1f), 0.10f);
        for (int i = -34; i <= 34; i++)
        {
            float bx = vx + i * 150f;
            for (int y = (int)horizon; y < H; y++)
            {
                float t = (y - horizon) / (H - horizon);
                float x = Mathf.Lerp(vx, bx, t);
                img.Over((int)x, y, new Color(0.15f, 0.9f, 1f, 0.05f + 0.35f * t));
                img.Over((int)x + 1, y, new Color(0.15f, 0.9f, 1f, 0.03f + 0.18f * t));
            }
        }
        for (int k = 0; k < 16; k++)
        {
            float t = Mathf.Pow(k / 15f, 2.1f);
            int y = (int)Mathf.Lerp(horizon, H - 2, t);
            for (int x = 0; x < W; x++) img.Over(x, y, new Color(0.15f, 0.9f, 1f, 0.05f + 0.34f * t));
        }
        for (int x = 0; x < W; x++) img.Over(x, (int)horizon, new Color(0.3f, 0.95f, 1f, 0.5f));
        return img;
    }

    static CyberImg WebNetwork()
    {
        CyberImg img = new CyberImg(500, 1080, Color.clear);
        Vector2 hub = new Vector2(-30f, 150f);
        int spokes = 7;
        float[] ang = new float[spokes];
        for (int i = 0; i < spokes; i++) ang[i] = Mathf.Lerp(-8f, 92f, i / (float)(spokes - 1)) * Mathf.Deg2Rad;
        Vector2[] prev = null;
        Color line = new Color(0.15f, 0.9f, 1f, 0.7f);
        for (int ring = 1; ring <= 6; ring++)
        {
            float r = ring * 92f;
            Vector2[] pts = new Vector2[spokes];
            for (int i = 0; i < spokes; i++) pts[i] = hub + new Vector2(Mathf.Cos(ang[i]), Mathf.Sin(ang[i])) * r;
            for (int i = 0; i < spokes - 1; i++)
            {
                Vector2 mid = (pts[i] + pts[i + 1]) * 0.5f;
                Vector2 sag = (hub - mid).normalized * 14f;
                img.Seg(pts[i].x, pts[i].y, mid.x + sag.x, mid.y + sag.y, 5f, new Color(0.15f, 0.9f, 1f, 0.07f));
                img.Seg(mid.x + sag.x, mid.y + sag.y, pts[i + 1].x, pts[i + 1].y, 5f, new Color(0.15f, 0.9f, 1f, 0.07f));
                img.Seg(pts[i].x, pts[i].y, mid.x + sag.x, mid.y + sag.y, 1.6f, line);
                img.Seg(mid.x + sag.x, mid.y + sag.y, pts[i + 1].x, pts[i + 1].y, 1.6f, line);
            }
            prev = pts;
            foreach (Vector2 p in pts) { img.Glow(p.x, p.y, 16f, Cyan, 0.5f); img.Disc(p.x, p.y, 3.2f, new Color(0.85f, 1f, 1f, 1f)); }
        }
        for (int i = 0; i < spokes; i++)
        {
            Vector2 end = hub + new Vector2(Mathf.Cos(ang[i]), Mathf.Sin(ang[i])) * 6f * 92f;
            img.Seg(hub.x, hub.y, end.x, end.y, 5f, new Color(0.15f, 0.9f, 1f, 0.08f));
            img.Seg(hub.x, hub.y, end.x, end.y, 1.8f, new Color(0.15f, 0.9f, 1f, 0.8f));
        }
        if (prev != null) img.Glow(hub.x + 60f, hub.y + 60f, 160f, Cyan, 0.18f);
        return img;
    }

    static CyberImg CircuitTree()
    {
        CyberImg img = new CyberImg(500, 1080, Color.clear);
        Color tr = new Color(0.45f, 1f, 0.55f, 1f);
        Vector2[] trunk = { new Vector2(60f, 1080f), new Vector2(60f, 900f), new Vector2(110f, 850f), new Vector2(110f, 640f), new Vector2(70f, 600f), new Vector2(70f, 470f) };
        for (int i = 0; i < trunk.Length - 1; i++)
        {
            img.Seg(trunk[i].x, trunk[i].y, trunk[i + 1].x, trunk[i + 1].y, 16f, new Color(tr.r, tr.g, tr.b, 0.07f));
            img.Seg(trunk[i].x, trunk[i].y, trunk[i + 1].x, trunk[i + 1].y, 6f, new Color(tr.r, tr.g, tr.b, 0.55f));
            img.Seg(trunk[i].x, trunk[i].y, trunk[i + 1].x, trunk[i + 1].y, 2f, new Color(0.9f, 1f, 0.95f, 0.9f));
        }
        Vector2[][] branches =
        {
            new[] { new Vector2(70f, 520f), new Vector2(130f, 470f), new Vector2(200f, 470f), new Vector2(240f, 420f) },
            new[] { new Vector2(70f, 470f), new Vector2(60f, 400f), new Vector2(120f, 350f), new Vector2(180f, 345f) },
            new[] { new Vector2(110f, 700f), new Vector2(190f, 690f), new Vector2(250f, 640f), new Vector2(330f, 640f) },
            new[] { new Vector2(110f, 780f), new Vector2(60f, 740f), new Vector2(40f, 690f) },
            new[] { new Vector2(130f, 470f), new Vector2(130f, 400f), new Vector2(170f, 405f) },
        };
        foreach (Vector2[] b in branches)
        {
            for (int i = 0; i < b.Length - 1; i++)
            {
                img.Seg(b[i].x, b[i].y, b[i + 1].x, b[i + 1].y, 9f, new Color(0.15f, 0.9f, 1f, 0.07f));
                img.Seg(b[i].x, b[i].y, b[i + 1].x, b[i + 1].y, 3.4f, new Color(0.15f, 0.9f, 1f, 0.75f));
            }
            Vector2 e = b[b.Length - 1];
            img.Glow(e.x, e.y, 46f, Cyan, 0.45f);
            Vector2[] hex = new Vector2[6];
            for (int i = 0; i < 6; i++) { float a = (60f * i) * Mathf.Deg2Rad; hex[i] = e + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 20f; }
            img.Poly(hex, new Color(0.03f, 0.12f, 0.18f, 0.95f), Cyan, 3f);
            img.Disc(e.x, e.y, 5f, new Color(0.85f, 1f, 1f, 1f));
        }
        img.Glow(60f, 1040f, 140f, tr, 0.25f);
        return img;
    }

    // ---- panels and buttons ------------------------------------------------------------------

    static CyberImg PanelGlass()
    {
        CyberImg img = new CyberImg(128, 128, Color.clear);
        img.Glow(64f, 64f, 90f, Cyan, 0.05f);
        img.RRect(3f, 3f, 125f, 125f, 18f, new Color(0.050f, 0.100f, 0.170f, 0.96f), new Color(0.025f, 0.055f, 0.105f, 0.97f), CyberImg.A(Cyan, 0.9f), 2.6f);
        img.RRect(10f, 10f, 118f, 118f, 12f, Color.clear, Color.clear, CyberImg.A(Cyan, 0.16f), 1.2f);
        Color b = CyberImg.A(new Color(0.7f, 1f, 1f, 1f), 0.95f);
        foreach (float[] c in new[] { new[] { 3f, 3f, 1f, 1f }, new[] { 125f, 3f, -1f, 1f }, new[] { 3f, 125f, 1f, -1f }, new[] { 125f, 125f, -1f, -1f } })
        {
            img.Seg(c[0] + c[2] * 6f, c[1], c[0] + c[2] * 26f, c[1], 4f, b);
            img.Seg(c[0], c[1] + c[3] * 6f, c[0], c[1] + c[3] * 26f, 4f, b);
        }
        return img;
    }

    static CyberImg ButtonPrimary()
    {
        CyberImg img = new CyberImg(256, 96, Color.clear);
        img.RRect(3f, 3f, 253f, 93f, 26f, new Color(0.07f, 0.26f, 0.40f, 1f), new Color(0.03f, 0.11f, 0.20f, 1f), CyberImg.A(Cyan, 1f), 3f);
        img.RRect(8f, 8f, 248f, 46f, 20f, new Color(1f, 1f, 1f, 0.10f), new Color(1f, 1f, 1f, 0.01f), Color.clear, 0f);
        img.RRect(9f, 9f, 247f, 87f, 20f, Color.clear, Color.clear, CyberImg.A(Cyan, 0.22f), 1.2f);
        return img;
    }

    static CyberImg TimerPlate()
    {
        CyberImg img = new CyberImg(352, 85, Color.clear);
        img.RRect(3f, 3f, 349f, 82f, 20f, new Color(0.05f, 0.10f, 0.17f, 0.97f), new Color(0.025f, 0.055f, 0.105f, 0.97f), CyberImg.A(Cyan, 0.95f), 2.6f);
        img.RRect(9f, 9f, 343f, 76f, 14f, Color.clear, Color.clear, CyberImg.A(Cyan, 0.15f), 1.2f);
        return img;
    }

    static CyberImg StarBar()
    {
        CyberImg img = new CyberImg(305, 114, Color.clear);
        img.Glow(152f, 57f, 150f, Gold, 0.06f);
        img.RRect(3f, 14f, 302f, 100f, 40f, new Color(0.06f, 0.10f, 0.17f, 0.98f), new Color(0.03f, 0.05f, 0.10f, 0.98f), CyberImg.A(Gold, 0.95f), 3f);
        img.RRect(10f, 21f, 295f, 93f, 33f, Color.clear, Color.clear, CyberImg.A(Gold, 0.2f), 1.2f);
        return img;
    }

    // ---- icons -------------------------------------------------------------------------------

    enum IconGlyph { Pause, Resume, Next, Replay, Levels, Exit, Hint }

    static CyberImg Icon(Color accent, IconGlyph g)
    {
        CyberImg img = new CyberImg(128, 128, Color.clear);
        img.Glow(64f, 64f, 70f, accent, 0.12f);
        img.RRect(6f, 6f, 122f, 122f, 28f, Ink0, Ink1, CyberImg.A(accent, 1f), 3.5f);
        img.RRect(13f, 13f, 115f, 115f, 22f, Color.clear, Color.clear, CyberImg.A(accent, 0.2f), 1.3f);
        Color c = accent;
        Color hi = Color.Lerp(accent, Color.white, 0.55f);
        switch (g)
        {
            case IconGlyph.Pause:
                img.RRect(42f, 36f, 56f, 92f, 5f, hi, c, Color.clear, 0f);
                img.RRect(72f, 36f, 86f, 92f, 5f, hi, c, Color.clear, 0f);
                break;
            case IconGlyph.Resume:
                img.Poly(new[] { new Vector2(50f, 34f), new Vector2(50f, 94f), new Vector2(98f, 64f) }, hi, Color.clear, 0f);
                break;
            case IconGlyph.Next:
                img.Poly(new[] { new Vector2(34f, 38f), new Vector2(34f, 90f), new Vector2(66f, 64f) }, hi, Color.clear, 0f);
                img.Poly(new[] { new Vector2(64f, 38f), new Vector2(64f, 90f), new Vector2(96f, 64f) }, hi, Color.clear, 0f);
                break;
            case IconGlyph.Replay:
                img.Arc(64f, 66f, 26f, 8f, 320f, 280f, hi);
                img.Poly(new[] { new Vector2(86f, 28f), new Vector2(98f, 52f), new Vector2(72f, 48f) }, hi, Color.clear, 0f);
                break;
            case IconGlyph.Levels:
                for (int i = 0; i < 3; i++)
                {
                    float y = 42f + i * 22f;
                    img.Disc(38f, y, 5.5f, hi);
                    img.Seg(54f, y, 92f, y, 8f, c);
                }
                break;
            case IconGlyph.Exit:
                img.Seg(34f, 36f, 34f, 92f, 7f, c);
                img.Seg(34f, 36f, 62f, 36f, 7f, c);
                img.Seg(34f, 92f, 62f, 92f, 7f, c);
                img.Seg(54f, 64f, 90f, 64f, 8f, hi);
                img.Poly(new[] { new Vector2(88f, 48f), new Vector2(88f, 80f), new Vector2(108f, 64f) }, hi, Color.clear, 0f);
                break;
            case IconGlyph.Hint:
                img.Glow(64f, 56f, 44f, accent, 0.5f);
                img.Disc(64f, 56f, 23f, hi);
                img.RRect(52f, 78f, 76f, 92f, 4f, c, c, Color.clear, 0f);
                img.RRect(55f, 96f, 73f, 103f, 3f, c, c, Color.clear, 0f);
                for (int i = 0; i < 5; i++)
                {
                    float a = (-160f + i * 35f) * Mathf.Deg2Rad;
                    img.Seg(64f + Mathf.Cos(a) * 31f, 56f + Mathf.Sin(a) * 31f, 64f + Mathf.Cos(a) * 41f, 56f + Mathf.Sin(a) * 41f, 4f, c);
                }
                break;
        }
        return img;
    }

    static CyberImg ShieldIcon()
    {
        CyberImg img = new CyberImg(153, 164, Color.clear);
        float cx = 76.5f, cy = 82f, R = 62f;
        img.Glow(cx, cy, 80f, Cyan, 0.5f);
        Vector2[] s =
        {
            new Vector2(cx, cy - R), new Vector2(cx + R * 0.85f, cy - R * 0.62f), new Vector2(cx + R * 0.85f, cy + R * 0.08f),
            new Vector2(cx + R * 0.48f, cy + R * 0.66f), new Vector2(cx, cy + R), new Vector2(cx - R * 0.48f, cy + R * 0.66f),
            new Vector2(cx - R * 0.85f, cy + R * 0.08f), new Vector2(cx - R * 0.85f, cy - R * 0.62f)
        };
        img.Poly(s, new Color(0.03f, 0.10f, 0.17f, 0.97f), Cyan, 4f);
        img.Arc(cx, cy - 8f, 15f, 5f, 180f, 360f, Cyan);
        img.Seg(cx - 15f, cy - 8f, cx - 15f, cy, 5f, Cyan);
        img.Seg(cx + 15f, cy - 8f, cx + 15f, cy, 5f, Cyan);
        img.RRect(cx - 23f, cy - 1f, cx + 23f, cy + 28f, 5f, Cyan, new Color(0.05f, 0.7f, 0.9f, 1f), Color.clear, 0f);
        img.Disc(cx, cy + 10f, 4f, Ink1);
        return img;
    }

    static CyberImg Star(bool filled)
    {
        CyberImg img = new CyberImg(128, 128, Color.clear);
        Vector2[] pts = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float a = (-90f + 36f * i) * Mathf.Deg2Rad;
            float r = i % 2 == 0 ? 54f : 23f;
            pts[i] = new Vector2(64f + Mathf.Cos(a) * r, 68f + Mathf.Sin(a) * r);
        }
        if (filled)
        {
            img.Glow(64f, 68f, 62f, Gold, 0.55f);
            img.Poly(pts, new Color(1f, 0.86f, 0.30f, 1f), new Color(1f, 0.96f, 0.7f, 1f), 4f);
        }
        else
        {
            img.Poly(pts, new Color(0.05f, 0.09f, 0.15f, 0.9f), CyberImg.A(Cyan, 0.5f), 3.5f);
        }
        return img;
    }

    // ---- level cards -------------------------------------------------------------------------

    static CyberImg Card(Color accent, int cells, int seed)
    {
        CyberImg img = new CyberImg(467, 655, Color.clear);
        for (int g = 3; g >= 1; g--) img.RRect(6f - g * 2f, 6f - g * 2f, 461f + g * 2f, 649f + g * 2f, 36f + g * 2f, Color.clear, Color.clear, CyberImg.A(accent, 0.05f), 5f);
        img.RRect(6f, 6f, 461f, 649f, 34f, new Color(0.06f, 0.11f, 0.19f, 0.97f), new Color(0.02f, 0.04f, 0.08f, 0.98f), CyberImg.A(accent, 1f), 4f);
        img.RRect(18f, 18f, 449f, 140f, 24f, CyberImg.A(accent, 0.20f), CyberImg.A(accent, 0.04f), Color.clear, 0f);
        img.RRect(16f, 16f, 451f, 639f, 26f, Color.clear, Color.clear, CyberImg.A(accent, 0.18f), 1.4f);

        Random.InitState(seed);
        float size = 236f, x0 = (467f - size) * 0.5f, y0 = 188f, cs = size / cells;
        img.Glow(233f, y0 + size * 0.5f, 190f, accent, 0.10f);
        for (int r = 0; r < cells; r++)
        for (int c = 0; c < cells; c++)
        {
            bool open = Random.value < 0.56f;
            float px = x0 + c * cs, py = y0 + r * cs;
            if (open)
            {
                img.RRect(px + 0.6f, py + 0.6f, px + cs - 0.6f, py + cs - 0.6f, Mathf.Min(2f, cs * 0.2f), CyberImg.A(accent, 0.30f), CyberImg.A(accent, 0.18f), CyberImg.A(accent, 0.8f), 1f);
            }
            else
            {
                img.RRect(px + 0.6f, py + 0.6f, px + cs - 0.6f, py + cs - 0.6f, Mathf.Min(2f, cs * 0.2f), new Color(0.02f, 0.04f, 0.08f, 0.9f), new Color(0.02f, 0.04f, 0.08f, 0.9f), Color.clear, 0f);
            }
        }
        img.Seg(40f, 470f, 427f, 470f, 2f, CyberImg.A(accent, 0.55f));
        img.Disc(40f, 470f, 4f, accent); img.Disc(427f, 470f, 4f, accent);
        return img;
    }

    // ---- effects -----------------------------------------------------------------------------

    static CyberImg FxGlow()
    {
        CyberImg img = new CyberImg(128, 128, Color.clear);
        img.Glow(64f, 64f, 64f, Color.white, 1f);
        return img;
    }

    static CyberImg FxRing()
    {
        CyberImg img = new CyberImg(256, 256, Color.clear);
        img.Ring(128f, 128f, 120f, 7f, new Color(1f, 1f, 1f, 0.12f));
        img.Ring(128f, 128f, 120f, 2.2f, new Color(1f, 1f, 1f, 0.95f));
        return img;
    }

    static CyberImg FxSpark()
    {
        CyberImg img = new CyberImg(64, 64, Color.clear);
        img.Glow(32f, 32f, 30f, Color.white, 0.5f);
        img.Poly(new[] { new Vector2(32f, 2f), new Vector2(37f, 27f), new Vector2(62f, 32f), new Vector2(37f, 37f), new Vector2(32f, 62f), new Vector2(27f, 37f), new Vector2(2f, 32f), new Vector2(27f, 27f) }, Color.white, Color.clear, 0f);
        return img;
    }

    static CyberImg FxSquare()
    {
        CyberImg img = new CyberImg(16, 16, Color.clear);
        img.RRect(1f, 1f, 15f, 15f, 3f, Color.white, Color.white, Color.clear, 0f);
        return img;
    }

    static CyberImg HoloPlate()
    {
        CyberImg img = new CyberImg(512, 256, Color.clear);
        for (int y = 0; y < 256; y++)
        for (int x = 0; x < 512; x++)
        {
            float dx = (x - 256f) / 250f, dy = (y - 128f) / 120f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            if (d < 1f) img.Over(x, y, new Color(0.1f, 0.85f, 1f, 0.28f * Mathf.Pow(1f - d, 1.4f)));
        }
        foreach (float k in new[] { 0.97f, 0.78f, 0.58f })
        {
            for (int i = 0; i < 360; i += 1)
            {
                float a0 = i * Mathf.Deg2Rad, a1 = (i + 1.4f) * Mathf.Deg2Rad;
                if ((i / 20) % 3 == 0 && k < 0.9f) continue;
                img.Seg(256f + Mathf.Cos(a0) * 246f * k, 128f + Mathf.Sin(a0) * 116f * k, 256f + Mathf.Cos(a1) * 246f * k, 128f + Mathf.Sin(a1) * 116f * k, 2.4f, new Color(0.3f, 0.95f, 1f, k > 0.9f ? 0.85f : 0.45f));
            }
        }
        return img;
    }

    static CyberImg HudRing()
    {
        CyberImg img = new CyberImg(512, 512, Color.clear);
        img.Ring(256f, 256f, 250f, 1.6f, new Color(1f, 1f, 1f, 0.55f));
        for (int i = 0; i < 8; i++) img.Arc(256f, 256f, 236f, 5f, i * 45f + 6f, i * 45f + 30f, new Color(1f, 1f, 1f, 0.9f));
        for (int i = 0; i < 72; i++)
        {
            float a = i * 5f * Mathf.Deg2Rad, len = i % 6 == 0 ? 12f : 6f;
            img.Seg(256f + Mathf.Cos(a) * 214f, 256f + Mathf.Sin(a) * 214f, 256f + Mathf.Cos(a) * (214f - len), 256f + Mathf.Sin(a) * (214f - len), 1.8f, new Color(1f, 1f, 1f, i % 6 == 0 ? 0.8f : 0.4f));
        }
        return img;
    }

    static CyberImg Rays()
    {
        CyberImg img = new CyberImg(512, 512, Color.clear);
        for (int y = 0; y < 512; y++)
        for (int x = 0; x < 512; x++)
        {
            float dx = x - 255.5f, dy = y - 255.5f;
            float d = Mathf.Sqrt(dx * dx + dy * dy) / 256f;
            if (d >= 1f) continue;
            float ang = Mathf.Atan2(dy, dx);
            float ray = Mathf.Pow(Mathf.Clamp01(Mathf.Cos(ang * 8f) * 0.5f + 0.5f), 3f);
            img.Over(x, y, new Color(1f, 1f, 1f, ray * 0.55f * Mathf.Pow(1f - d, 1.3f)));
        }
        return img;
    }

    // ---- output ------------------------------------------------------------------------------

    static void Save(string name, CyberImg img, Vector4 border, float ppu, string copyImportFrom)
    {
        string path = Folder + "/" + name + ".png";
        Texture2D tex = img.ToTexture();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 2048;

        TextureImporterSettings s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        s.spriteBorder = border;
        s.spritePixelsPerUnit = ppu;
        if (!string.IsNullOrEmpty(copyImportFrom))
        {
            // sprites that replace world-space art keep the original pivot and scale so they line up
            TextureImporter orig = AssetImporter.GetAtPath(copyImportFrom) as TextureImporter;
            if (orig != null)
            {
                TextureImporterSettings os = new TextureImporterSettings();
                orig.ReadTextureSettings(os);
                s.spritePixelsPerUnit = os.spritePixelsPerUnit;
                s.spriteAlignment = os.spriteAlignment;
                s.spritePivot = os.spritePivot;
            }
        }
        ti.SetTextureSettings(s);
        ti.SaveAndReimport();
    }
}
