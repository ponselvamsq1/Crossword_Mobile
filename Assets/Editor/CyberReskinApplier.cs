using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Applies the cybersecurity reskin to the open Crossword scene: swaps sprites, recolours text and panels,
/// adds the card labels / title, and attaches the animation and UI effect components.
/// Menu: Cyber > Apply Reskin (run Build UI Sprites first). Idempotent. Questions, answers and logic are not touched.
/// </summary>
public static class CyberReskinApplier
{
    const string UI = "Assets/UI_Cyber/";
    static readonly Color TextLight = new Color(0.88f, 0.97f, 1f, 1f);
    static readonly Color Cyan = new Color(0.10f, 0.90f, 1f, 1f);
    static readonly Color Magenta = new Color(1f, 0.25f, 0.72f, 1f);
    static readonly Color Lime = new Color(0.45f, 1f, 0.55f, 1f);
    static readonly Color Gold = new Color(1f, 0.82f, 0.25f, 1f);

    static Sprite S(string n) { return AssetDatabase.LoadAssetAtPath<Sprite>(UI + n + ".png"); }

    static Scene scene;
    static StringBuilder log;

    [MenuItem("Cyber/Apply Reskin")]
    public static void Apply()
    {
        scene = SceneManager.GetActiveScene();
        log = new StringBuilder();

        RemoveBrokenFxComponents();
        SwapSprites();
        RecolourShapes();
        RecolourText();
        DimOverlays();
        LevelCards();
        StartPage();
        Characters();
        PanelAndButtonFx();
        StarsOnGameManager();
        CentreStars();
        Camera();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Cyber reskin applied.\n" + log);
    }

    // Components left behind by an earlier version of the FX scripts (classes that shared one file) resolve to
    // "missing script"; they are ours, so clear them before the components are added again.
    static void RemoveBrokenFxComponents()
    {
        int n = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                n += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        log.AppendLine("broken components removed: " + n);
    }

    // ---- helpers -----------------------------------------------------------------------------

    static IEnumerable<T> All<T>() where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (T c in root.GetComponentsInChildren<T>(true)) yield return c;
    }

    static Transform Find(string path)
    {
        string[] parts = path.Split('/');
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != parts[0]) continue;
            if (parts.Length == 1) return root.transform;
            Transform t = root.transform.Find(string.Join("/", parts, 1, parts.Length - 1));
            if (t != null) return t;
        }
        return null;
    }

    static bool IsProjectSprite(Sprite s) { return s != null && AssetDatabase.GetAssetPath(s).StartsWith("Assets/"); }

    // ---- sprites -----------------------------------------------------------------------------

    static void SwapSprites()
    {
        Dictionary<string, string> map = new Dictionary<string, string>
        {
            { "BG with Texture", "bg_game" }, { "Levels Screen", "bg_game" }, { "Intro screen 2", "bg_intro" },
            { "wame grass", "floor_grid" }, { "web", "web_cyber" }, { "bug tree", "tree_cyber" }, { "Icon", "icon_shield" },
            { "Panel", "panel_glass" }, { "Panel 1", "panel_glass" }, { "Submit box", "button_primary" }, { "Timer box", "timer_plate" },
            { "Hint button", "icon_hint" }, { "Pause button", "icon_pause" }, { "Levels button", "icon_levels" }, { "Exit button", "icon_exit" },
            { "Next button", "icon_next" }, { "Replay button", "icon_replay" }, { "Resume button (1)_0", "icon_resume" },
            { "Empty Star", "star_empty" }, { "Filled Star", "star_filled" }, { "Background", "star_bar" },
            { "ChatGPT Image Jun 16, 2026, 03_23_23 PM", "button_primary" }, { "Retry button box", "button_primary" },
            { "Easy", "card_easy" }, { "Medium", "card_medium" }, { "Difficult", "card_hard" },
        };
        HashSet<string> sliced = new HashSet<string> { "panel_glass", "button_primary" };

        int n = 0;
        foreach (Image img in All<Image>())
        {
            Sprite cur = img.sprite;
            string key;
            if (!IsProjectSprite(cur) || !map.TryGetValue(cur.name, out key)) continue;
            Sprite repl = S(key);
            if (repl == null) { log.AppendLine("missing sprite " + key); continue; }
            img.sprite = repl;
            img.color = Color.white;
            if (sliced.Contains(key)) { img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = 1f; }
            else if (img.type == Image.Type.Sliced) img.type = Image.Type.Simple;
            EditorUtility.SetDirty(img);
            n++;
        }

        foreach (SpriteRenderer sr in All<SpriteRenderer>())
        {
            Sprite cur = sr.sprite;
            string key;
            if (!IsProjectSprite(cur) || !map.TryGetValue(cur.name, out key)) continue;
            if (key != "web_cyber" && key != "tree_cyber") continue;
            sr.sprite = S(key);
            EditorUtility.SetDirty(sr);
            n++;
        }

        // level selection: dark backdrop behind the frame (the cream tint would otherwise multiply the new art)
        Transform ls = Find("Canvas/Levelselection");
        if (ls != null)
        {
            Image root = ls.GetComponent<Image>();
            root.sprite = S("bg_game"); root.color = Color.white;
            Transform frame = ls.Find("Image");
            if (frame != null) frame.GetComponent<Image>().color = Color.white;
        }

        // the Final panel dimmer becomes the game backdrop
        Transform fin = Find("MenuCanvas/Final");
        if (fin != null) { Image i = fin.GetComponent<Image>(); i.sprite = S("bg_game"); i.color = Color.white; }
        log.AppendLine("sprites swapped: " + n);
    }

    // ---- Shapes2D panels ---------------------------------------------------------------------

    static void RecolourShapes()
    {
        foreach (Shapes2D.Shape sh in All<Shapes2D.Shape>())
        {
            Color f = sh.settings.fillColor;
            // Shapes2D feeds its colours to the shader without the sRGB conversion, so give it linear values
            sh.settings.fillColor = new Color(0.045f, 0.09f, 0.155f, f.a).linear;
            sh.settings.fillColor2 = new Color(0.025f, 0.055f, 0.105f, f.a).linear;
            sh.settings.outlineColor = new Color(0.10f, 0.90f, 1f, 0.9f).linear;
            if (sh.settings.outlineSize < 0.5f) sh.settings.outlineSize = Mathf.Max(sh.settings.outlineSize, 2f);
            EditorUtility.SetDirty(sh);
            if (sh.name == "Handle") { sh.settings.fillColor = new Color(0.10f, 0.90f, 1f, 1f).linear; sh.settings.fillColor2 = new Color(0.05f, 0.6f, 0.8f, 1f).linear; }
            log.AppendLine("shape recoloured: " + sh.transform.parent.name + "/" + sh.name);
        }
    }

    // ---- text --------------------------------------------------------------------------------

    static void RecolourText()
    {
        int n = 0;
        foreach (TMP_Text t in All<TMP_Text>())
        {
            Color c = t.color;
            float lum = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            if (lum > 0.45f) continue;
            t.color = new Color(TextLight.r, TextLight.g, TextLight.b, c.a);
            EditorUtility.SetDirty(t);
            n++;
        }
        log.AppendLine("texts recoloured: " + n);
    }

    static void DimOverlays()
    {
        // the grid backing shows between and around the cells: make it dark instead of light grey
        foreach (string p in new[] { "Canvas/EasyLevel/GridParent", "Canvas/EasyLevel/GridParent/Grid", "Canvas/MediumLevel/Grid", "Canvas/HardLevel/Grid" })
        {
            Transform g = Find(p);
            if (g == null) continue;
            Image gi = g.GetComponent<Image>();
            if (gi != null) { gi.color = new Color(0.03f, 0.06f, 0.10f, 1f); EditorUtility.SetDirty(gi); }
        }

        foreach (string p in new[] { "Pause", "Quit", "Levelcomplete", "Tryagain", "TimeupPanel" })
        {
            Transform t = Find("MenuCanvas/" + p);
            if (t == null) continue;
            Image i = t.GetComponent<Image>();
            if (i == null) continue;
            i.color = new Color(0.01f, 0.03f, 0.07f, 0.86f);
            EditorUtility.SetDirty(i);
        }
    }

    // ---- level selection cards ---------------------------------------------------------------

    static void LevelCards()
    {
        AddCardText("Canvas/Levelselection/Image/Panel/Easy",      "Easy",      Lime,    "No. of words: 8\nGrid size: 12x12\nTime limit: 5 min");
        AddCardText("Canvas/Levelselection/Image/Panel/Medium",    "Medium",    Gold,    "No. of words: 12\nGrid size: 15x15\nTime limit: 8 min");
        AddCardText("Canvas/Levelselection/Image/Panel/Difficult", "Difficult", Magenta, "No. of words: 18\nGrid size: 18x18\nTime limit: 10 min");
    }

    static TMP_FontAsset Font()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/ObjektivMk1_Trial_XBd SDF.asset");
    }

    static TextMeshProUGUI MakeText(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align)
    {
        Transform ex = parent.Find(name);
        GameObject go = ex != null ? ex.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        t.font = Font();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        EditorUtility.SetDirty(t);
        return t;
    }

    static void AddCardText(string path, string title, Color accent, string details)
    {
        Transform card = Find(path);
        if (card == null) { log.AppendLine("card not found " + path); return; }

        // the cards already carry their own title / detail labels: only tint the title, never touch the wording
        Transform mine;
        while ((mine = card.Find("CardTitle")) != null) Object.DestroyImmediate(mine.gameObject);
        while ((mine = card.Find("CardDetails")) != null) Object.DestroyImmediate(mine.gameObject);

        Transform t = card.Find("Text (TMP) (1)");
        if (t != null)
        {
            TMP_Text tt = t.GetComponent<TMP_Text>();
            tt.color = accent;
            EditorUtility.SetDirty(tt);
        }
    }

    // ---- start page --------------------------------------------------------------------------

    static void StartPage()
    {
        Transform sp = Find("Canvas/StartPage");
        if (sp == null) return;

        TextMeshProUGUI cyber = MakeText(sp, "TitleCyber", "CYBER", 78f, Cyan, TextAlignmentOptions.Left);
        cyber.fontStyle = FontStyles.Bold;
        cyber.characterSpacing = 42f;
        Place(cyber.rectTransform, new Vector2(1010f - 960f, 150f), new Vector2(900f, 110f));

        TextMeshProUGUI word = MakeText(sp, "TitleCrossword", "CROSSWORD", 118f, Color.white, TextAlignmentOptions.Left);
        word.fontStyle = FontStyles.Bold;
        word.characterSpacing = 6f;
        word.enableAutoSizing = true;       // always fits the space left of the screen edge
        word.fontSizeMin = 40f; word.fontSizeMax = 118f;
        Place(word.rectTransform, new Vector2(1010f - 960f, 28f), new Vector2(860f, 160f));

        TextMeshProUGUI tag = MakeText(sp, "TitleTagline", "SOLVE  •  DECODE  •  DEFEND", 34f, new Color(1f, 0.25f, 0.72f, 1f), TextAlignmentOptions.Left);
        tag.characterSpacing = 18f;
        Place(tag.rectTransform, new Vector2(1010f - 960f, -85f), new Vector2(900f, 60f));

        Transform start = sp.Find("Start");
        if (start != null)
        {
            RectTransform r = (RectTransform)start;
            r.anchoredPosition = new Vector2(1010f - 960f + r.sizeDelta.x * 0.5f, r.anchoredPosition.y);
            Ensure<CyberButtonFX>(start.gameObject).idlePulse = true;
        }

        Transform icon = sp.Find("Image"); if (icon == null) icon = sp.Find("world_animation");
        if (icon != null) ((RectTransform)icon).anchoredPosition = new Vector2(640f, 175f);

        // slow HUD rings around the shield in the background art
        MakeRing(sp, "HudRingA", new Vector2(520f - 960f, 540f - 560f + 20f), 940f, Cyan, 5f, 0.35f);
        MakeRing(sp, "HudRingB", new Vector2(520f - 960f, 540f - 560f + 20f), 700f, Magenta, -8f, 0.45f);
        Transform bg = sp.Find("Bg");
        if (bg != null)
        {
            foreach (string n in new[] { "HudRingA", "HudRingB" })
            {
                Transform ring = sp.Find(n);
                if (ring != null) ring.SetSiblingIndex(bg.GetSiblingIndex() + 1);
            }
        }

        Ensure<CyberPanelPop>(cyber.gameObject).slideFrom = new Vector2(-120f, 0f);
        CyberPanelPop wp = Ensure<CyberPanelPop>(word.gameObject); wp.slideFrom = new Vector2(120f, 0f); wp.delay = 0.12f;
        CyberPanelPop tp = Ensure<CyberPanelPop>(tag.gameObject); tp.fadeOnly = true; tp.delay = 0.45f;
    }

    static void Place(RectTransform r, Vector2 pos, Vector2 size)
    {
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0f, 0.5f);
        r.sizeDelta = size;
        r.anchoredPosition = pos;
    }

    static void MakeRing(Transform parent, string name, Vector2 pos, float size, Color c, float speed, float alpha)
    {
        Transform ex = parent.Find(name);
        GameObject go = ex != null ? ex.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        Image i = go.GetComponent<Image>();
        i.sprite = S("hud_ring");
        i.color = new Color(c.r, c.g, c.b, alpha);
        i.raycastTarget = false;
        RectTransform r = (RectTransform)go.transform;
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(size, size);
        r.anchoredPosition = pos;
        Ensure<CyberSpin>(go).degreesPerSecond = speed;
        EditorUtility.SetDirty(i);
    }

    static T Ensure<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    // ---- characters --------------------------------------------------------------------------

    static Material ParticleMaterial()
    {
        string path = UI + "fx_particles.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    static void Characters()
    {
        Material pm = ParticleMaterial();

        // the mobile scene draws every character as a UI Image driven by an Animator: same objects and
        // enable/disable triggers, new mascot frames (sprite curves rebuilt for the Image component)
        Mascot("Canvas/StartPage/Character_animatoin",                   "start", CyberUIAnimFX.Kind.StartPage,     0.98f, pm, 400f, new Vector2(-70f, 20f), 0.8f);
        Mascot("MenuCanvas/Pause/Popup/PauseThinkAnimation",             "think", CyberUIAnimFX.Kind.Pause,         0.85f, pm, 300f);
        Mascot("MenuCanvas/Quit/Popup/PauseThinkAnimation",              "think", CyberUIAnimFX.Kind.Pause,         0.85f, pm, 300f);
        Mascot("MenuCanvas/Levelcomplete/LevelCompleteAnimation ",       "dance", CyberUIAnimFX.Kind.LevelComplete, 0.82f, pm);
        Mascot("MenuCanvas/Tryagain/LeveInCompletedAnimation",           "sad",   CyberUIAnimFX.Kind.TryAgain,      0.66f, pm);
        Mascot("MenuCanvas/TimeupPanel/LeveInCompletedAnimation (1)",    "sad",   CyberUIAnimFX.Kind.TryAgain,      0.66f, pm);
        Mascot("MenuCanvas/Final/FinalDance",                            "final", CyberUIAnimFX.Kind.Final,         0.85f, pm);
        Mascot("MenuCanvas/CompleteAllLevel/FinalDance",                 "final", CyberUIAnimFX.Kind.Final,         0.85f, pm);

        // the idle character shown during play only gets the outline glow and plate
        foreach (string level in new[] { "EasyLevel", "MediumLevel", "HardLevel" })
            Mascot("Canvas/" + level + "/CharacterAnimation", "idle", CyberUIAnimFX.Kind.Idle, 0.8f, pm);
    }

    static AnimationClip UiClip(string anim)
    {
        string uiPath = CyberCharacterBuilder.Folder + "/" + anim + "_ui.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(uiPath);
        if (clip != null) return clip;

        AnimationClip src = AssetDatabase.LoadAssetAtPath<AnimationClip>(CyberCharacterBuilder.Folder + "/" + anim + ".anim");
        if (src == null) return null;
        EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(src);
        if (bindings.Length == 0) return null;
        ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(src, bindings[0]);

        clip = new AnimationClip { frameRate = src.frameRate };
        EditorCurveBinding ui = new EditorCurveBinding { type = typeof(Image), path = "", propertyName = "m_Sprite" };
        AnimationUtility.SetObjectReferenceCurve(clip, ui, keys);
        AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(src);
        AnimationUtility.SetAnimationClipSettings(clip, s);
        AssetDatabase.CreateAsset(clip, uiPath);
        return clip;
    }

    static RuntimeAnimatorController UiController(string anim)
    {
        string path = CyberCharacterBuilder.Folder + "/" + anim + "_ui.controller";
        RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
        if (ctrl != null) return ctrl;
        AnimationClip clip = UiClip(anim);
        if (clip == null) return null;
        UnityEditor.Animations.AnimatorController c = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
        UnityEditor.Animations.AnimatorState st = c.layers[0].stateMachine.AddState(anim);
        st.motion = clip;
        c.layers[0].stateMachine.defaultState = st;
        return c;
    }

    static void Mascot(string path, string anim, CyberUIAnimFX.Kind kind, float referenceScale, Material pm, float size = 0f, Vector2? anchoredPos = null, float fxScale = 1f)
    {
        Transform t = Find(path);
        if (t == null) { log.AppendLine("character not found " + path); return; }
        RuntimeAnimatorController ctrl = UiController(anim);
        Sprite first = AssetDatabase.LoadAssetAtPath<Sprite>(CyberCharacterBuilder.Folder + "/" + anim + "/" + anim + "_00.png");
        if (ctrl == null || first == null) { log.AppendLine("mascot assets missing for " + anim + " (run Cyber > Build Character)"); return; }

        t.GetComponent<Animator>().runtimeAnimatorController = ctrl;
        Image img = t.GetComponent<Image>();
        img.sprite = first;
        img.color = Color.white;
        img.preserveAspect = true;
        Vector3 s = t.localScale;
        t.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);   // the old character was drawn mirrored; the mascot faces front

        RectTransform rect = (RectTransform)t;
        if (size > 0f) rect.sizeDelta = new Vector2(size, size);
        if (anchoredPos.HasValue) rect.anchoredPosition = anchoredPos.Value;

        CyberUIAnimFX fx = Ensure<CyberUIAnimFX>(t.gameObject);
        fx.kind = kind;
        fx.fxScale = fxScale;
        fx.referenceScale = referenceScale;
        fx.glowSprite = S("fx_glow"); fx.ringSprite = S("fx_ring"); fx.sparkSprite = S("fx_spark");
        fx.squareSprite = S("fx_square"); fx.plateSprite = S("fx_plate"); fx.raysSprite = S("fx_rays");
        fx.particleMaterial = pm;
        // smaller, higher ground plate so it stays inside the popup box
        if (kind == CyberUIAnimFX.Kind.Pause || kind == CyberUIAnimFX.Kind.TryAgain || kind == CyberUIAnimFX.Kind.Final)
        {
            fx.plateScale = 0.62f;
            fx.plateOffsetY = -1.86f;
        }
        EditorUtility.SetDirty(fx);
        EditorUtility.SetDirty(img);
        EditorUtility.SetDirty(t.gameObject);
    }

    // ---- panel transitions and button feedback -----------------------------------------------

    static void PanelAndButtonFx()
    {
        foreach (string p in new[] { "MenuCanvas/Pause/Popup", "MenuCanvas/Quit/Popup", "MenuCanvas/CompleteAllLevel/Image", "MenuCanvas/Levelcomplete/Image", "MenuCanvas/Tryagain/Image", "MenuCanvas/TimeupPanel/Image", "MenuCanvas/Final/Image", "Canvas/Levelselection/Image" })
        {
            Transform t = Find(p);
            if (t != null) Ensure<CyberPanelPop>(t.gameObject);
        }
        foreach (string p in new[] { "MenuCanvas/Pause", "MenuCanvas/Quit", "MenuCanvas/Levelcomplete", "MenuCanvas/Tryagain", "MenuCanvas/TimeupPanel", "MenuCanvas/Final", "MenuCanvas/CompleteAllLevel" })
        {
            Transform t = Find(p);
            if (t == null) continue;
            Ensure<CyberPanelPop>(t.gameObject).fadeOnly = true;
            if (p != "MenuCanvas/Final" && p != "MenuCanvas/CompleteAllLevel") Ensure<CyberOverlayCover>(t.gameObject);
        }

        foreach (Button b in All<Button>())
        {
            if (b.GetComponent<CyberButtonFX>() == null) Undo.AddComponent<CyberButtonFX>(b.gameObject);
            ColorBlock cb = b.colors;
            cb.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            cb.pressedColor = new Color(0.8f, 0.9f, 1f, 1f);
            cb.selectedColor = Color.white;
            b.colors = cb;
        }
    }

    // ---- stars -------------------------------------------------------------------------------

    static void StarsOnGameManager()
    {
        GameManager gm = null;
        foreach (GameManager g in All<GameManager>()) gm = g;
        if (gm == null) return;
        SerializedObject so = new SerializedObject(gm);
        so.FindProperty("emptystar").objectReferenceValue = S("star_empty");
        so.FindProperty("filledstar").objectReferenceValue = S("star_filled");
        so.ApplyModifiedProperties();
    }

    // the three stars sit centred over the "LEVEL COMPLETED" text
    static void CentreStars()
    {
        Transform label = Find("MenuCanvas/Levelcomplete/Image/TimeUpTxt");
        if (label == null) return;
        float x = ((RectTransform)label).anchoredPosition.x;
        foreach (string p in new[] { "MenuCanvas/Levelcomplete/Image/EmptyStarPanel", "MenuCanvas/Levelcomplete/Image/FilledStarPanel (1)" })
        {
            Transform t = Find(p);
            if (t == null) continue;
            RectTransform r = (RectTransform)t;
            r.anchoredPosition = new Vector2(x, r.anchoredPosition.y);
            EditorUtility.SetDirty(r);
        }
    }

    static void Camera()
    {
        Camera cam = UnityEngine.Camera.main;
        if (cam == null) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.05f, 0.09f, 1f);
        EditorUtility.SetDirty(cam);
    }
}
