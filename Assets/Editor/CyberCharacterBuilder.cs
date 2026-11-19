using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Draws the "Byte" mascot (a crossword-screen robot) frame by frame and builds the sprite animations + controllers
/// that replace the old character. Menu: Cyber > Build Character. Deterministic and safe to re-run.
/// </summary>
public static class CyberCharacterBuilder
{
    public const string Folder = "Assets/UI_Cyber/Character";
    const int Size = 500;
    const int Frames = 21;
    const float Fps = 12f;

    static readonly Color Cyan = new Color(0.10f, 0.90f, 1f, 1f);
    static readonly Color Magenta = new Color(1f, 0.25f, 0.72f, 1f);
    static readonly Color Gold = new Color(1f, 0.82f, 0.25f, 1f);
    static readonly Color Lime = new Color(0.45f, 1f, 0.55f, 1f);
    static readonly Color Case0 = new Color(0.20f, 0.31f, 0.45f, 1f);
    static readonly Color Case1 = new Color(0.09f, 0.15f, 0.26f, 1f);
    static readonly Color Screen = new Color(0.015f, 0.04f, 0.08f, 1f);

    public enum Eyes { Open, Blink, Happy, Sad, Star, Side, Alarm, Cross }
    public enum Mouth { Smile, Open, Frown, Flat }

    public class Pose
    {
        public float bob, headDx, headDy, ant, legL, legR;       // px
        public float lookX;                                      // pupil offset for Eyes.Side
        public float[] armL = { 12f, 8f }, armR = { 12f, 8f };   // upper / lower angle, 0 = down, 90 = sideways, 180 = up
        public Eyes eyes = Eyes.Open;
        public Mouth mouth = Mouth.Smile;
        public int dots;                                         // thinking dots
        public float tear = -1f;                                 // 0..1 falling tear, <0 none
        public float sparkle = -1f;                              // >=0 enables sparkles with this phase
        public bool coreBlink;
        public float warn;                                       // red alert overlay on the screen, 0..1
    }

    public static readonly string[] Names = { "idle", "start", "think", "dance", "sad", "final" };

    [MenuItem("Cyber/Build Character")]
    public static void Build()
    {
        Directory.CreateDirectory(Folder);
        List<string> pngs = new List<string>();

        AssetDatabase.StartAssetEditing();
        foreach (string name in Names)
        {
            Directory.CreateDirectory(Folder + "/" + name);
            for (int i = 0; i < Frames; i++)
            {
                CyberImg img = Draw(PoseFor(name, i / (float)Frames));
                string path = Folder + "/" + name + "/" + name + "_" + i.ToString("00") + ".png";
                Texture2D t = img.ToTexture();
                File.WriteAllBytes(path, t.EncodeToPNG());
                Object.DestroyImmediate(t);
                pngs.Add(path);
            }
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        AssetDatabase.StartAssetEditing();
        foreach (string path in pngs)
        {
            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        foreach (string name in Names) BuildAnimation(name);
        AssetDatabase.SaveAssets();
        Debug.Log("Cyber character built.");
    }

    // ---- animation assets --------------------------------------------------------------------

    static void BuildAnimation(string name)
    {
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[Frames];
        for (int i = 0; i < Frames; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / Fps,
                value = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "/" + name + "/" + name + "_" + i.ToString("00") + ".png")
            };
        }

        string clipPath = Folder + "/" + name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
        clip.frameRate = Fps;
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = true;
        s.stopTime = Frames / Fps;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        EditorUtility.SetDirty(clip);

        string ctrlPath = Folder + "/" + name + ".controller";
        AssetDatabase.DeleteAsset(ctrlPath);
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        AnimatorState state = ctrl.layers[0].stateMachine.AddState(name);
        state.motion = clip;
        ctrl.layers[0].stateMachine.defaultState = state;
        EditorUtility.SetDirty(ctrl);
    }

    // ---- poses -------------------------------------------------------------------------------

    static Pose PoseFor(string name, float t)
    {
        float tau = t * Mathf.PI * 2f;
        Pose p = new Pose();
        switch (name)
        {
            case "idle":
                p.bob = Mathf.Sin(tau) * 3f;
                p.ant = Mathf.Sin(tau) * 6f;
                p.armL = new[] { 14f + Mathf.Sin(tau) * 3f, 8f };
                p.armR = new[] { 14f + Mathf.Sin(tau) * 3f, 8f };
                if (t > 0.84f && t < 0.93f) p.eyes = Eyes.Blink;
                p.coreBlink = t > 0.5f;
                break;
            case "start": // friendly wave
                p.bob = Mathf.Sin(tau) * 4f;
                p.ant = Mathf.Sin(tau * 2f) * 7f;
                p.armL = new[] { 14f, 8f };
                p.armR = new[] { 128f, 168f + Mathf.Sin(tau * 3f) * 26f };
                p.eyes = (t > 0.45f && t < 0.52f) ? Eyes.Blink : Eyes.Open;
                p.sparkle = t;
                break;
            case "think":
                p.bob = Mathf.Sin(tau) * 2f;
                p.headDx = Mathf.Sin(tau) * 3f;
                p.ant = Mathf.Sin(tau) * 4f;
                p.armL = new[] { 12f, 8f };
                p.armR = new[] { 56f, 170f };
                p.eyes = Eyes.Side;
                p.lookX = Mathf.Sin(tau) * 8f;
                p.mouth = Mathf.Sin(tau) > 0.3f ? Mouth.Flat : Mouth.Smile;
                p.dots = 1 + Mathf.FloorToInt(t * 3f) % 3;
                break;
            case "dance": // level complete
            {
                float hop = Mathf.Abs(Mathf.Sin(tau * 2f));
                p.bob = -hop * 20f;
                p.ant = Mathf.Sin(tau * 2f) * 9f;
                float sw = Mathf.Sin(tau * 2f) * 24f;
                p.armL = new[] { 104f - sw, 150f - sw };
                p.armR = new[] { 104f + sw, 150f + sw };
                p.legL = Mathf.Sin(tau * 2f) * 8f; p.legR = -Mathf.Sin(tau * 2f) * 8f;
                p.eyes = Eyes.Happy; p.mouth = Mouth.Open;
                p.sparkle = t;
                break;
            }
            case "sad":
                p.headDy = 14f;
                p.bob = Mathf.Sin(tau) * 2f + 6f;
                p.ant = -16f + Mathf.Sin(tau) * 3f;
                p.armL = new[] { 6f + Mathf.Sin(tau) * 2f, 2f };
                p.armR = new[] { 6f + Mathf.Sin(tau) * 2f, 2f };
                p.eyes = Eyes.Sad; p.mouth = Mouth.Frown;
                p.tear = Mathf.Repeat(t * 1.0f, 1f);
                break;
            case "final":
            {
                float jump = (1f - Mathf.Cos(tau * 2f)) * 0.5f;
                p.bob = -jump * 30f;
                p.ant = Mathf.Sin(tau * 4f) * 10f;
                float wig = Mathf.Sin(tau * 4f) * 12f;
                p.armL = new[] { 110f + wig, 158f - wig };
                p.armR = new[] { 110f - wig, 158f + wig };
                p.legL = jump * 12f; p.legR = -jump * 12f;
                p.eyes = Eyes.Star; p.mouth = Mouth.Open;
                p.sparkle = t * 2f;
                p.coreBlink = ((int)(t * 8f)) % 2 == 0;
                break;
            }
        }
        return p;
    }

    // ---- drawing -----------------------------------------------------------------------------

    static Vector2 Dir(float deg, bool left)
    {
        float a = deg * Mathf.Deg2Rad;
        return new Vector2((left ? -1f : 1f) * Mathf.Sin(a), Mathf.Cos(a));
    }

    public static CyberImg Draw(Pose p)
    {
        CyberImg img = new CyberImg(Size, Size, Color.clear);
        float by = p.bob;                    // everything above the legs moves with the bob

        // ground contact glow
        img.Glow(250f, 448f, 150f, Cyan, 0.10f);

        // legs (they stay planted; hips follow the bob)
        float foot = 440f + Mathf.Min(0f, by) * 0.85f;   // feet leave the ground when the mascot jumps
        DrawLeg(img, 224f + p.legL * 0.5f, 392f + by, 224f + p.legL, foot);
        DrawLeg(img, 276f + p.legR * 0.5f, 392f + by, 276f + p.legR, foot);

        // arms behind the torso edge
        DrawArm(img, new Vector2(194f, 326f + by), p.armL, true);
        DrawArm(img, new Vector2(306f, 326f + by), p.armR, false);

        // torso
        img.RRect(188f, 300f + by, 312f, 394f + by, 26f, Case0, Case1, CyberImg.A(Cyan, 0.7f), 3.5f);
        img.RRect(198f, 310f + by, 302f, 384f + by, 18f, Color.clear, Color.clear, CyberImg.A(Cyan, 0.18f), 1.5f);
        DrawShield(img, 250f, 346f + by, 26f, p.coreBlink);
        img.Seg(206f, 388f + by, 294f, 388f + by, 4f, CyberImg.A(Magenta, 0.8f));

        // neck
        img.RRect(232f, 288f + by, 268f, 308f + by, 4f, Case1, Case1, Color.clear, 0f);

        // antenna
        float hx = p.headDx, hy = by + p.headDy;
        float topX = 250f + hx + p.ant, topY = 36f + hy + Mathf.Abs(p.ant) * 0.3f;
        img.Seg(250f + hx, 74f + hy, topX, topY + 6f, 6f, Case0);
        Color ball = p.dots > 0 && p.dots % 2 == 0 ? Gold : Magenta;
        img.Glow(topX, topY, 34f, ball, 0.8f);
        img.Disc(topX, topY, 11f, ball);
        img.Disc(topX - 3f, topY - 3f, 4f, new Color(1f, 1f, 1f, 0.8f));

        // head (monitor)
        img.RRect(118f + hx, 72f + hy, 382f + hx, 296f + hy, 40f, Case0, Case1, CyberImg.A(Cyan, 0.95f), 5f);
        img.RRect(104f + hx, 150f + hy, 120f + hx, 214f + hy, 6f, Magenta, new Color(0.6f, 0.1f, 0.4f, 1f), Color.clear, 0f);   // ear modules
        img.RRect(380f + hx, 150f + hy, 396f + hx, 214f + hy, 6f, Magenta, new Color(0.6f, 0.1f, 0.4f, 1f), Color.clear, 0f);
        DrawScreen(img, hx, hy);
        DrawFace(img, p, hx, hy);
        if (p.warn > 0f) img.RRect(140f + hx, 94f + hy, 360f + hx, 274f + hy, 24f, new Color(1f, 0.1f, 0.15f, 0.28f * p.warn), new Color(1f, 0.1f, 0.15f, 0.28f * p.warn), Color.clear, 0f);

        if (p.dots > 0) DrawDots(img, p.dots, topX, topY);
        if (p.tear >= 0f) DrawTear(img, p.tear, hx, hy);
        if (p.sparkle >= 0f) DrawSparkles(img, p.sparkle, by);
        return img;
    }

    static void DrawLeg(CyberImg img, float hx, float hy, float fx, float fy)
    {
        img.Seg(hx, hy, fx, fy - 8f, 28f, Case1);
        img.Seg(hx, hy, fx, fy - 8f, 28f, CyberImg.A(Cyan, 0.0f));
        img.RRect(fx - 30f, fy - 12f, fx + 30f, fy + 14f, 12f, Case0, Case1, CyberImg.A(Cyan, 0.9f), 3f);
        img.Seg(fx - 20f, fy + 2f, fx + 20f, fy + 2f, 3f, CyberImg.A(Cyan, 0.7f));
    }

    static void DrawArm(CyberImg img, Vector2 shoulder, float[] ang, bool left)
    {
        Vector2 elbow = shoulder + Dir(ang[0], left) * 70f;
        Vector2 hand = elbow + Dir(ang[1], left) * 66f;
        img.Seg(shoulder.x, shoulder.y, elbow.x, elbow.y, 26f, Case1);
        img.Seg(elbow.x, elbow.y, hand.x, hand.y, 24f, Case1);
        img.Seg(shoulder.x, shoulder.y, elbow.x, elbow.y, 5f, CyberImg.A(Cyan, 0.75f));
        img.Seg(elbow.x, elbow.y, hand.x, hand.y, 5f, CyberImg.A(Cyan, 0.75f));
        img.Disc(shoulder.x, shoulder.y, 17f, Case0);
        img.Disc(elbow.x, elbow.y, 14f, Case0);
        img.Disc(hand.x, hand.y, 20f, Cyan);
        img.Disc(hand.x - 4f, hand.y - 4f, 8f, new Color(0.85f, 1f, 1f, 0.9f));
    }

    static void DrawShield(CyberImg img, float cx, float cy, float r, bool bright)
    {
        img.Glow(cx, cy, r * 2.4f, Cyan, bright ? 0.7f : 0.35f);
        Vector2[] s =
        {
            new Vector2(cx, cy - r), new Vector2(cx + r * 0.85f, cy - r * 0.6f), new Vector2(cx + r * 0.85f, cy + r * 0.1f),
            new Vector2(cx + r * 0.48f, cy + r * 0.68f), new Vector2(cx, cy + r), new Vector2(cx - r * 0.48f, cy + r * 0.68f),
            new Vector2(cx - r * 0.85f, cy + r * 0.1f), new Vector2(cx - r * 0.85f, cy - r * 0.6f)
        };
        img.Poly(s, new Color(0.03f, 0.10f, 0.17f, 1f), Cyan, 3f);
        img.RRect(cx - 9f, cy - 3f, cx + 9f, cy + 12f, 3f, Cyan, Cyan, Color.clear, 0f);
        img.Arc(cx, cy - 4f, 6f, 3f, 180f, 360f, Cyan);
    }

    static void DrawScreen(CyberImg img, float hx, float hy)
    {
        img.RRect(140f + hx, 94f + hy, 360f + hx, 274f + hy, 24f, Screen, Screen, CyberImg.A(Cyan, 0.5f), 2f);
        img.Glow(250f + hx, 184f + hy, 140f, new Color(0.05f, 0.5f, 0.7f, 1f), 0.18f);

        // crossword grid on the screen: 6 x 5 cells, a few lit like solved letters
        int[] lit = { 1, 4, 7, 9, 12, 14, 16, 19, 22, 25, 27 };
        float x0 = 148f + hx, y0 = 102f + hy, cw = 34.6f, ch = 33.2f;
        for (int r = 0; r < 5; r++)
        for (int c = 0; c < 6; c++)
        {
            int idx = r * 6 + c;
            bool on = System.Array.IndexOf(lit, idx) >= 0;
            float px = x0 + c * cw, py = y0 + r * ch;
            Color tint = idx % 3 == 0 ? Magenta : Cyan;
            img.RRect(px + 1.5f, py + 1.5f, px + cw - 1.5f, py + ch - 1.5f, 5f,
                CyberImg.A(tint, on ? 0.20f : 0.05f), CyberImg.A(tint, on ? 0.10f : 0.03f), CyberImg.A(Cyan, on ? 0.45f : 0.22f), 1.4f);
            if (idx % 5 == 0) img.Disc(px + 6f, py + 6f, 1.8f, new Color(1f, 1f, 1f, 0.7f));
        }
        // glass reflection
        img.Poly(new[] { new Vector2(146f + hx, 100f + hy), new Vector2(230f + hx, 100f + hy), new Vector2(146f + hx, 190f + hy) }, new Color(1f, 1f, 1f, 0.05f), Color.clear, 0f);
    }

    static void DrawEye(CyberImg img, Eyes kind, float cx, float cy, bool left, float lookX)
    {
        switch (kind)
        {
            case Eyes.Blink:
                img.Seg(cx - 18f, cy, cx + 18f, cy, 7f, Cyan);
                break;
            case Eyes.Happy:
                img.Glow(cx, cy, 44f, Cyan, 0.4f);
                img.Arc(cx, cy + 12f, 20f, 9f, 205f, 335f, Cyan);
                break;
            case Eyes.Sad:
                img.Glow(cx, cy, 44f, Cyan, 0.3f);
                img.RRect(cx - 17f, cy - 14f, cx + 17f, cy + 20f, 12f, Cyan, new Color(0.05f, 0.6f, 0.8f, 1f), Color.clear, 0f);
                img.Disc(cx - 5f, cy - 4f, 5f, new Color(1f, 1f, 1f, 0.8f));
                float inner = left ? 1f : -1f;
                img.Seg(cx - inner * 22f, cy - 18f, cx + inner * 22f, cy - 32f, 6f, Cyan);   // worried brow, inner end raised
                break;
            case Eyes.Star:
            {
                Vector2[] pts = new Vector2[10];
                for (int i = 0; i < 10; i++)
                {
                    float a = (-90f + 36f * i) * Mathf.Deg2Rad, r = i % 2 == 0 ? 26f : 11f;
                    pts[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
                }
                img.Glow(cx, cy, 48f, Gold, 0.6f);
                img.Poly(pts, Gold, new Color(1f, 0.97f, 0.7f, 1f), 2f);
                break;
            }
            case Eyes.Alarm:
                img.Glow(cx, cy, 50f, new Color(1f, 0.2f, 0.25f, 1f), 0.7f);
                img.RRect(cx - 20f, cy - 30f, cx + 20f, cy + 30f, 14f, new Color(1f, 0.85f, 0.85f, 1f), new Color(1f, 0.3f, 0.35f, 1f), Color.clear, 0f);
                img.Disc(cx, cy, 6f, Screen);
                break;
            case Eyes.Cross:
                img.Glow(cx, cy, 44f, new Color(1f, 0.2f, 0.25f, 1f), 0.5f);
                img.Seg(cx - 17f, cy - 17f, cx + 17f, cy + 17f, 8f, new Color(1f, 0.3f, 0.35f, 1f));
                img.Seg(cx - 17f, cy + 17f, cx + 17f, cy - 17f, 8f, new Color(1f, 0.3f, 0.35f, 1f));
                break;
            case Eyes.Side:
                img.Glow(cx, cy, 44f, Cyan, 0.4f);
                img.RRect(cx - 19f, cy - 24f, cx + 19f, cy + 24f, 13f, new Color(0.8f, 1f, 1f, 1f), Cyan, Color.clear, 0f);
                img.Disc(cx + lookX * 0.9f, cy - 2f, 9f, Screen);
                break;
            default:
                img.Glow(cx, cy, 46f, Cyan, 0.5f);
                img.RRect(cx - 18f, cy - 25f, cx + 18f, cy + 25f, 13f, new Color(0.7f, 1f, 1f, 1f), Cyan, Color.clear, 0f);
                img.Disc(cx - 5f, cy - 9f, 5f, new Color(1f, 1f, 1f, 0.9f));
                break;
        }
    }

    static void DrawFace(CyberImg img, Pose p, float hx, float hy)
    {
        DrawEye(img, p.eyes, 205f + hx, 170f + hy, true, p.lookX);
        DrawEye(img, p.eyes, 295f + hx, 170f + hy, false, p.lookX);

        switch (p.mouth)
        {
            case Mouth.Smile: img.Arc(250f + hx, 204f + hy, 26f, 7f, 25f, 155f, Cyan); break;
            case Mouth.Frown: img.Arc(250f + hx, 244f + hy, 24f, 7f, 205f, 335f, Cyan); break;
            case Mouth.Flat:  img.Seg(228f + hx, 226f + hy, 272f + hx, 226f + hy, 7f, Cyan); break;
            case Mouth.Open:
                img.RRect(222f + hx, 206f + hy, 278f + hx, 244f + hy, 16f, Screen, new Color(0.0f, 0.12f, 0.16f, 1f), Cyan, 5f);
                img.RRect(234f + hx, 230f + hy, 266f + hx, 241f + hy, 5f, CyberImg.A(Magenta, 0.9f), CyberImg.A(Magenta, 0.9f), Color.clear, 0f);
                break;
        }
    }

    static void DrawDots(CyberImg img, int count, float ax, float ay)
    {
        for (int i = 0; i < 3; i++)
        {
            float x = ax + 52f + i * 30f, y = ay - 10f - i * 8f;
            if (i < count) { img.Glow(x, y, 22f, Cyan, 0.7f); img.Disc(x, y, 9f, Cyan); }
            else img.Disc(x, y, 7f, CyberImg.A(Cyan, 0.25f));
        }
    }

    static void DrawTear(CyberImg img, float t, float hx, float hy)
    {
        float y = 196f + hy + t * 70f;
        float a = 1f - Mathf.Clamp01((t - 0.6f) / 0.4f);
        Color c = new Color(0.55f, 0.9f, 1f, a);
        img.Disc(188f + hx, y, 7f, c);
        img.Poly(new[] { new Vector2(188f + hx, y - 20f), new Vector2(182f + hx, y - 3f), new Vector2(194f + hx, y - 3f) }, c, Color.clear, 0f);
    }

    static void DrawSparkles(CyberImg img, float phase, float by)
    {
        float[][] spots = { new[] { 70f, 120f }, new[] { 430f, 100f }, new[] { 60f, 300f }, new[] { 445f, 280f }, new[] { 120f, 40f }, new[] { 390f, 40f } };
        for (int i = 0; i < spots.Length; i++)
        {
            float k = Mathf.Repeat(phase * 2f + i * 0.17f, 1f);
            float s = Mathf.Sin(k * Mathf.PI);            // fade in and out
            if (s < 0.05f) continue;
            float x = spots[i][0], y = spots[i][1] + by * 0.5f - k * 14f, r = 6f + s * 14f;
            Color c = i % 2 == 0 ? Cyan : Gold;
            img.Glow(x, y, r * 2.2f, c, 0.5f * s);
            img.Poly(new[] { new Vector2(x, y - r), new Vector2(x + r * 0.22f, y - r * 0.22f), new Vector2(x + r, y), new Vector2(x + r * 0.22f, y + r * 0.22f),
                             new Vector2(x, y + r), new Vector2(x - r * 0.22f, y + r * 0.22f), new Vector2(x - r, y), new Vector2(x - r * 0.22f, y - r * 0.22f) },
                new Color(1f, 1f, 1f, s), Color.clear, 0f);
        }
    }
}
