using UnityEngine;

/// <summary>Tiny software rasteriser used by the Cyber asset builders. Origin is top-left, y grows downwards.</summary>
public class CyberImg
{
    public readonly int W, H;
    public readonly Color[] P;

    public CyberImg(int w, int h, Color fill)
    {
        W = w; H = h; P = new Color[w * h];
        for (int i = 0; i < P.Length; i++) P[i] = fill;
    }

    public static float Cov(float d) { return Mathf.Clamp01(0.5f - d); }
    public static Color A(Color c, float a) { return new Color(c.r, c.g, c.b, c.a * a); }

    public void Over(int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= W || y >= H || c.a <= 0.0004f) return;
        int i = y * W + x;
        Color d = P[i];
        float a = c.a + d.a * (1f - c.a);
        if (a <= 0f) { P[i] = Color.clear; return; }
        float k = d.a * (1f - c.a);
        P[i] = new Color((c.r * c.a + d.r * k) / a, (c.g * c.a + d.g * k) / a, (c.b * c.a + d.b * k) / a, a);
    }

    public void Disc(float cx, float cy, float r, Color c)
    {
        int x0 = Mathf.FloorToInt(cx - r - 2), x1 = Mathf.CeilToInt(cx + r + 2), y0 = Mathf.FloorToInt(cy - r - 2), y1 = Mathf.CeilToInt(cy + r + 2);
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy)) - r;
            Over(x, y, A(c, Cov(d)));
        }
    }

    public void Ring(float cx, float cy, float r, float w, Color c)
    {
        float R = r + w;
        int x0 = Mathf.FloorToInt(cx - R - 2), x1 = Mathf.CeilToInt(cx + R + 2), y0 = Mathf.FloorToInt(cy - R - 2), y1 = Mathf.CeilToInt(cy + R + 2);
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            float d = Mathf.Abs(Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy)) - r) - w * 0.5f;
            Over(x, y, A(c, Cov(d)));
        }
    }

    /// <summary>Arc from a0 to a1 degrees (0 = +x, clockwise on screen).</summary>
    public void Arc(float cx, float cy, float r, float w, float a0, float a1, Color c)
    {
        float R = r + w;
        int x0 = Mathf.FloorToInt(cx - R - 2), x1 = Mathf.CeilToInt(cx + R + 2), y0 = Mathf.FloorToInt(cy - R - 2), y1 = Mathf.CeilToInt(cy + R + 2);
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
            float ang = Mathf.Repeat(Mathf.Atan2(dy, dx) * Mathf.Rad2Deg, 360f);
            float s = Mathf.Repeat(a0, 360f), e = Mathf.Repeat(a1, 360f);
            bool inside = s <= e ? (ang >= s && ang <= e) : (ang >= s || ang <= e);
            if (!inside) continue;
            float d = Mathf.Abs(Mathf.Sqrt(dx * dx + dy * dy) - r) - w * 0.5f;
            Over(x, y, A(c, Cov(d)));
        }
    }

    public void Seg(float ax, float ay, float bx, float by, float w, Color c)
    {
        float pad = w * 0.5f + 2f;
        int x0 = Mathf.FloorToInt(Mathf.Min(ax, bx) - pad), x1 = Mathf.CeilToInt(Mathf.Max(ax, bx) + pad);
        int y0 = Mathf.FloorToInt(Mathf.Min(ay, by) - pad), y1 = Mathf.CeilToInt(Mathf.Max(ay, by) + pad);
        float vx = bx - ax, vy = by - ay, len2 = Mathf.Max(vx * vx + vy * vy, 1e-6f);
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            float px = x + 0.5f - ax, py = y + 0.5f - ay;
            float t = Mathf.Clamp01((px * vx + py * vy) / len2);
            float dx = px - vx * t, dy = py - vy * t;
            float d = Mathf.Sqrt(dx * dx + dy * dy) - w * 0.5f;
            Over(x, y, A(c, Cov(d)));
        }
    }

    public void Glow(float cx, float cy, float R, Color c, float k)
    {
        int x0 = Mathf.FloorToInt(cx - R), x1 = Mathf.CeilToInt(cx + R), y0 = Mathf.FloorToInt(cy - R), y1 = Mathf.CeilToInt(cy + R);
        for (int y = Mathf.Max(0, y0); y <= Mathf.Min(H - 1, y1); y++)
        for (int x = Mathf.Max(0, x0); x <= Mathf.Min(W - 1, x1); x++)
        {
            float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy)) / R;
            if (d >= 1f) continue;
            Over(x, y, A(c, k * Mathf.Pow(1f - d, 2.2f)));
        }
    }

    public void RRect(float x0, float y0, float x1, float y1, float r, Color top, Color bottom, Color stroke, float sw)
    {
        float cx = (x0 + x1) * 0.5f, cy = (y0 + y1) * 0.5f, hx = (x1 - x0) * 0.5f, hy = (y1 - y0) * 0.5f;
        int ix0 = Mathf.FloorToInt(x0 - 2), ix1 = Mathf.CeilToInt(x1 + 2), iy0 = Mathf.FloorToInt(y0 - 2), iy1 = Mathf.CeilToInt(y1 + 2);
        for (int y = iy0; y <= iy1; y++)
        for (int x = ix0; x <= ix1; x++)
        {
            float qx = Mathf.Abs(x + 0.5f - cx) - (hx - r), qy = Mathf.Abs(y + 0.5f - cy) - (hy - r);
            float d = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
            float t = Mathf.Clamp01((y - y0) / Mathf.Max(1f, y1 - y0));
            Color f = Color.Lerp(top, bottom, t);
            if (f.a > 0f) Over(x, y, A(f, Cov(d)));
            if (sw > 0f && stroke.a > 0f) Over(x, y, A(stroke, Cov(Mathf.Abs(d + sw * 0.5f) - sw * 0.5f)));
        }
    }

    public void Poly(Vector2[] pts, Color fill, Color stroke, float sw)
    {
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (Vector2 p in pts) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y); }
        for (int y = Mathf.FloorToInt(minY - 3); y <= Mathf.CeilToInt(maxY + 3); y++)
        for (int x = Mathf.FloorToInt(minX - 3); x <= Mathf.CeilToInt(maxX + 3); x++)
        {
            Vector2 q = new Vector2(x + 0.5f, y + 0.5f);
            bool inside = false;
            float dmin = float.MaxValue;
            for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
            {
                Vector2 a = pts[i], b = pts[j];
                if ((a.y > q.y) != (b.y > q.y) && q.x < (b.x - a.x) * (q.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
                Vector2 ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(q - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f));
                dmin = Mathf.Min(dmin, (q - (a + ab * t)).magnitude);
            }
            float sd = inside ? -dmin : dmin;
            if (fill.a > 0f) Over(x, y, A(fill, Cov(sd)));
            if (sw > 0f && stroke.a > 0f) Over(x, y, A(stroke, Cov(Mathf.Abs(sd) - sw * 0.5f)));
        }
    }

    public Texture2D ToTexture()
    {
        Texture2D t = new Texture2D(W, H, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[W * H];
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
            px[(H - 1 - y) * W + x] = P[y * W + x];
        t.SetPixels32(px);
        t.Apply();
        return t;
    }
}
