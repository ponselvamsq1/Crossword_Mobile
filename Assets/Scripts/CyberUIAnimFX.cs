using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extra polish for the UI character animations (start page, pause, level complete, try again, final, idle).
/// The Animator frame animation and the GameManager's enable/disable of the object are untouched: this only reacts to
/// the object being enabled, adds a glow/hologram plate, and plays tweens + particle effects around the character.
/// UI version of CyberAnimFX: sizes are authored in "character units" (the same numbers the world-space version
/// used) and converted to canvas pixels / world units from the character's rect.
/// </summary>
[RequireComponent(typeof(Image))]
public class CyberUIAnimFX : MonoBehaviour
{
    public enum Kind { StartPage, Pause, LevelComplete, TryAgain, Final, Idle }

    public Kind kind = Kind.Idle;

    [Header("Sprites / material (assigned by the reskin tool)")]
    public Sprite glowSprite, ringSprite, sparkSprite, squareSprite, plateSprite, raysSprite;
    public Material particleMaterial;

    [Header("Layout (character units)")]
    public float plateOffsetY = -2.15f;
    public float plateScale = 0.85f;
    [Tooltip("Scale the original world-space character had on this screen; effect sizes are relative to it.")]
    public float referenceScale = 1f;
    [Tooltip("Scales the whole effect (rings, glow, particles) without changing the character.")]
    public float fxScale = 1f;

    Image img, outline;
    RectTransform rt, back, front;
    Vector3 baseScale;
    Vector2 basePos;
    Vector3 baseCenterLocal;
    float unitPx, worldPerUnit;
    int sortLayer, sortOrder;
    bool captured;

    static readonly Color Cyan = new Color(0.10f, 0.90f, 1f, 1f);
    static readonly Color Magenta = new Color(1f, 0.25f, 0.72f, 1f);
    static readonly Color Lime = new Color(0.45f, 1f, 0.55f, 1f);
    static readonly Color Gold = new Color(1f, 0.82f, 0.25f, 1f);

    void Capture()
    {
        if (captured) return;
        captured = true;
        img = GetComponent<Image>();
        rt = (RectTransform)transform;
        baseScale = rt.localScale;
        basePos = rt.anchoredPosition;
    }

    void OnEnable()
    {
        Capture();
        Cleanup();
        // the canvas scaler has not settled on the first frame after enabling: build the effects one frame later
        if (kind != Kind.Idle) { Color c = img.color; c.a = 0f; img.color = c; }
        StartCoroutine(Build());
    }

    System.Collections.IEnumerator Build()
    {
        yield return null;

        Canvas canvas = GetComponentInParent<Canvas>();
        Canvas root = canvas != null ? canvas.rootCanvas : null;
        float canvasScale = root != null ? root.transform.lossyScale.x : 1f;
        sortLayer = root != null ? root.sortingLayerID : 0;
        sortOrder = root != null ? root.sortingOrder + 1 : 1;

        // 1 character unit = (sprite width / 5) / referenceScale pixels (the mascot sprite is 5 world units wide at scale 1)
        float size = Mathf.Min(rt.rect.width, rt.rect.height);
        unitPx = size / (5f * Mathf.Max(referenceScale, 0.01f)) * fxScale;
        worldPerUnit = unitPx * canvasScale;

        RectTransform parent = (RectTransform)transform.parent;
        baseCenterLocal = parent.InverseTransformPoint(rt.TransformPoint(rt.rect.center));

        int idx = rt.GetSiblingIndex();
        back = NewRoot(name + "_FX", parent);
        back.SetSiblingIndex(idx);
        front = NewRoot(name + "_FXTop", parent);
        front.SetSiblingIndex(rt.GetSiblingIndex() + 1);

        BuildOutline();
        BuildPlate();
        switch (kind)
        {
            case Kind.StartPage:     PlayStart(); break;
            case Kind.Pause:         PlayPause(); break;
            case Kind.LevelComplete: PlayComplete(); break;
            case Kind.TryAgain:      PlayTryAgain(); break;
            case Kind.Final:         PlayFinal(); break;
        }
    }

    RectTransform NewRoot(string n, RectTransform parent)
    {
        GameObject go = new GameObject(n, typeof(RectTransform));
        go.layer = gameObject.layer;
        RectTransform r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = Vector2.zero;
        r.localPosition = baseCenterLocal;
        return r;
    }

    void OnDisable()
    {
        if (!captured) return;
        Cleanup();
    }

    void Cleanup()
    {
        DOTween.Kill(rt);
        DOTween.Kill(img);
        rt.localScale = baseScale;
        rt.anchoredPosition = basePos;
        rt.localRotation = Quaternion.identity;
        if (img != null) { Color c = img.color; c.a = 1f; img.color = c; }
        KillRoot(ref back);
        KillRoot(ref front);
        outline = null;
    }

    static void KillRoot(ref RectTransform r)
    {
        if (r == null) return;
        DOTween.Kill(r);
        foreach (Transform t in r) DOTween.Kill(t);
        foreach (Image i in r.GetComponentsInChildren<Image>(true)) DOTween.Kill(i);
        Destroy(r.gameObject);
        r = null;
    }

    void LateUpdate()
    {
        if (back != null)
        {
            // the effects stay on the character's resting centre (they ignore hop tweens)
            RectTransform parent = (RectTransform)transform.parent;
            Vector2 hop = rt.anchoredPosition - basePos;
            Vector3 centre = rt.TransformPoint(rt.rect.center) - parent.TransformVector(new Vector3(hop.x, hop.y, 0f));
            back.position = centre;
            if (front != null) front.position = centre;
        }
        if (outline != null)
        {
            outline.sprite = img.sprite;
            RectTransform o = outline.rectTransform;
            o.position = rt.TransformPoint(rt.rect.center);
            o.localScale = rt.localScale * 1.05f;
            o.rotation = rt.rotation;
            o.sizeDelta = rt.rect.size;
        }
    }

    // ---- building blocks ---------------------------------------------------------------------

    float Px(float units) { return units * unitPx; }

    Image MakeImage(RectTransform parent, string n, Sprite s, Color c, Vector2 posUnits, float widthUnits)
    {
        GameObject go = new GameObject(n, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        RectTransform r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(Px(posUnits.x), Px(posUnits.y));
        float aspect = s != null ? s.rect.height / s.rect.width : 1f;
        r.sizeDelta = new Vector2(Px(widthUnits), Px(widthUnits) * aspect);
        Image im = go.GetComponent<Image>();
        im.sprite = s;
        im.color = c;
        im.raycastTarget = false;
        return im;
    }

    void BuildOutline()
    {
        outline = MakeImage(back, "CyberOutline", img.sprite, new Color(0.10f, 0.90f, 1f, 0.55f), Vector2.zero, 1f);
        outline.rectTransform.sizeDelta = rt.rect.size;
    }

    void BuildPlate()
    {
        if (plateSprite == null) return;
        float k = referenceScale;
        // original: sprite is 5.12 units wide, scaled by plateScale * k
        float width = 5.12f * plateScale * k;
        Image plate = MakeImage(back, "Plate", plateSprite, new Color(1f, 1f, 1f, 0f), new Vector2(0f, plateOffsetY * k), width);
        plate.DOFade(0.9f, 0.5f).SetUpdate(true).SetTarget(back);
        plate.rectTransform.DOScale(1.04f, 1.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetTarget(back);
    }

    void PopIn(float from, float time)
    {
        rt.localScale = baseScale * from;
        rt.DOScale(baseScale, time).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(rt);
        Color c = img.color; c.a = 0f; img.color = c;
        img.DOFade(1f, time * 0.6f).SetUpdate(true).SetTarget(img);
    }

    void Hop(float heightUnits, float time, int loops, float delay = 0f)
    {
        rt.anchoredPosition = basePos;
        rt.DOAnchorPosY(basePos.y + Px(heightUnits), time).SetDelay(delay).SetEase(Ease.OutQuad).SetLoops(loops, LoopType.Yoyo).SetUpdate(true).SetTarget(rt);
    }

    void Glow(Color c, float diameterUnits, bool breathe, float lowAlpha, float time)
    {
        if (glowSprite == null) return;
        Image g = MakeImage(back, "Glow", glowSprite, c, new Vector2(0f, 0.2f * referenceScale), diameterUnits * 1.28f);
        if (breathe) g.DOFade(lowAlpha, time).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetTarget(back);
    }

    void PulseRing(Color c, float sizeUnits, float time, float delay)
    {
        if (ringSprite == null) return;
        Image ring = MakeImage(front, "Ring", ringSprite, new Color(c.r, c.g, c.b, 0f), new Vector2(0f, 0.2f * referenceScale), 2.56f);
        ring.rectTransform.localScale = Vector3.one * 0.05f;
        Sequence s = DOTween.Sequence().SetUpdate(true).SetTarget(front).SetDelay(delay);
        s.Append(ring.DOFade(0.9f, 0.05f));
        s.Join(ring.rectTransform.DOScale(sizeUnits / 2.56f, time).SetEase(Ease.OutCubic));
        s.Insert(0.05f, ring.DOFade(0f, time - 0.05f));
    }

    ParticleSystem MakeParticles(string n, Sprite sprite, Vector2 posUnits, int order, Vector3 dir)
    {
        GameObject go = new GameObject(n);
        go.layer = gameObject.layer;
        go.transform.SetParent(front, false);
        go.transform.localPosition = new Vector3(Px(posUnits.x), Px(posUnits.y), 0f);
        // particle shapes emit along +Z, so aim the emitter in the wanted direction (in the XY plane)
        go.transform.localRotation = Quaternion.LookRotation(new Vector3(dir.x, dir.y, 0f).normalized, Vector3.back);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer pr = go.GetComponent<ParticleSystemRenderer>();
        if (particleMaterial != null)
        {
            // one material per sprite so every effect keeps its own texture
            Material m = new Material(particleMaterial);
            if (sprite != null) m.mainTexture = sprite.texture;
            pr.sharedMaterial = m;
        }
        pr.sortingLayerID = sortLayer;
        pr.sortingOrder = sortOrder + order;

        var main = ps.main;
        main.useUnscaledTime = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Local;   // values below are given in world units
        main.playOnAwake = false;
        return ps;
    }

    static Gradient FadeOut(Color tint)
    {
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(tint, 0f), new GradientColorKey(tint, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0.9f, 0.6f), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    void Rising(string n, Color a, Color b, float rate, float size, float speed, float life, float width, Vector2 posUnits)
    {
        float w = worldPerUnit;
        ParticleSystem ps = MakeParticles(n, sparkSprite, posUnits, 2, Vector3.up);
        var main = ps.main;
        main.startLifetime = life;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.6f * w, speed * w);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f * w, size * w);
        main.startColor = new ParticleSystem.MinMaxGradient(a, b);
        main.maxParticles = 200;
        var em = ps.emission; em.rateOverTime = rate;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(width * w, 0.2f * w, 0.1f * w);
        var col = ps.colorOverLifetime; col.enabled = true; col.color = new ParticleSystem.MinMaxGradient(FadeOut(Color.white));
        var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
        ps.Play();
    }

    void Confetti(string n, Vector2 posUnits, int count, float speed, Vector3 dir, float spread, float gravity, float life)
    {
        float w = worldPerUnit;
        ParticleSystem ps = MakeParticles(n, squareSprite, posUnits, 3, dir);
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.7f, life);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.45f * w, speed * w);
        main.startSize = new ParticleSystem.MinMaxCurve(0.07f * w, 0.17f * w);
        main.gravityModifier = gravity * w;     // world gravity is 9.81 world units/s^2; the effect was tuned in character units
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 400;
        main.loop = false; // one-shot burst
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Cyan, 0f), new GradientColorKey(Magenta, 0.33f), new GradientColorKey(Gold, 0.66f), new GradientColorKey(Lime, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        main.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
        var em = ps.emission; em.rateOverTime = 0f; em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = spread; sh.radius = 0.1f * w;
        var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
        var col = ps.colorOverLifetime; col.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(fade);
        ps.Play();
    }

    // ---- the five effects --------------------------------------------------------------------

    void PlayStart()
    {
        PopIn(0.7f, 0.7f);
        rt.DOAnchorPosY(basePos.y + Px(0.08f), 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetTarget(rt);
        Glow(new Color(0.1f, 0.85f, 1f, 0.35f), 3.4f, true, 0.15f, 1.6f);
        PulseRing(Cyan, 3.4f, 1.1f, 0.25f);
        PulseRing(Magenta, 4.6f, 1.3f, 0.55f);
        Rising("Bits", Cyan, Magenta, 7f, 0.16f, 0.9f, 3.2f, 3f, new Vector2(0f, -1.8f));
    }

    void PlayPause()
    {
        // quick holographic flicker, then settle
        Sequence s = DOTween.Sequence().SetUpdate(true).SetTarget(img);
        s.Append(img.DOFade(0.15f, 0.05f)).Append(img.DOFade(1f, 0.06f)).Append(img.DOFade(0.4f, 0.05f)).Append(img.DOFade(1f, 0.08f));
        rt.localScale = baseScale * 0.92f;
        rt.DOScale(baseScale, 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(rt);
        PulseRing(Cyan, 3.0f, 1.2f, 0.1f);
        Rising("Bits", new Color(0.4f, 0.9f, 1f, 1f), Color.white, 3.5f, 0.12f, 0.45f, 3.6f, 2.4f, new Vector2(0f, -1.6f));
    }

    void PlayComplete()
    {
        PopIn(0.55f, 0.6f);
        Hop(0.35f, 0.22f, 4);
        Glow(new Color(0.45f, 1f, 0.55f, 0.28f), 3.6f, false, 0f, 0f);
        PulseRing(Lime, 4.0f, 0.9f, 0f);
        PulseRing(Cyan, 5.6f, 1.1f, 0.18f);
        PulseRing(Gold, 7.2f, 1.3f, 0.36f);
        Confetti("ConfettiL", new Vector2(-0.6f, 0.2f), 46, 7f, new Vector3(-0.35f, 1f, 0f), 38f, 1.4f, 1.8f);
        Confetti("ConfettiR", new Vector2(0.6f, 0.2f), 46, 7f, new Vector3(0.35f, 1f, 0f), 38f, 1.4f, 1.8f);
        Rising("Sparks", Lime, Gold, 10f, 0.2f, 1.3f, 2.2f, 2.8f, new Vector2(0f, -1.4f));
    }

    void PlayTryAgain()
    {
        // gentle and encouraging: soft pulse, slow wobble, warm drifting sparks
        PopIn(0.85f, 0.55f);
        rt.localRotation = Quaternion.Euler(0f, 0f, -2.5f);
        rt.DOLocalRotate(new Vector3(0f, 0f, 2.5f), 1.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetTarget(rt);
        Glow(new Color(1f, 0.75f, 0.35f, 0.22f), 3.2f, true, 0.08f, 1.8f);
        PulseRing(new Color(1f, 0.8f, 0.45f, 1f), 3.6f, 1.6f, 0.2f);
        Rising("Warm", Gold, new Color(1f, 0.6f, 0.4f, 1f), 4f, 0.15f, 0.55f, 3.2f, 2.4f, new Vector2(0f, -1.5f));
    }

    void PlayFinal()
    {
        PopIn(0.4f, 0.8f);
        Hop(0.45f, 0.26f, -1);
        if (raysSprite != null)
        {
            Image rays = MakeImage(back, "Rays", raysSprite, new Color(1f, 0.9f, 0.5f, 0.35f), new Vector2(0f, 0.3f * referenceScale), 3.2f * 5.12f);
            rays.rectTransform.DOLocalRotate(new Vector3(0f, 0f, -360f), 14f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1).SetUpdate(true).SetTarget(back);
        }
        Glow(new Color(1f, 0.85f, 0.4f, 0.3f), 3.6f, false, 0f, 0f);

        DOTween.Sequence().SetUpdate(true).SetTarget(front).SetLoops(-1)
            .AppendCallback(() =>
            {
                PulseRing(Gold, 8f, 1.3f, 0f);
                PulseRing(Cyan, 6f, 1.1f, 0.2f);
            })
            .AppendInterval(1.4f);

        Confetti("CannonL", new Vector2(-6.2f, -1.2f), 140, 15f, new Vector3(1f, 1.6f, 0f), 24f, 1.6f, 3.2f);
        Confetti("CannonR", new Vector2(5.2f, -1.2f), 140, 15f, new Vector3(-1f, 1.6f, 0f), 24f, 1.6f, 3.2f);
        DOTween.Sequence().SetUpdate(true).SetTarget(front).SetLoops(-1)
            .AppendInterval(2.6f)
            .AppendCallback(() =>
            {
                Confetti("CannonL", new Vector2(-6.2f, -1.2f), 60, 13f, new Vector3(1f, 1.6f, 0f), 24f, 1.6f, 3f);
                Confetti("CannonR", new Vector2(5.2f, -1.2f), 60, 13f, new Vector3(-1f, 1.6f, 0f), 24f, 1.6f, 3f);
            });
        Rising("Stars", Gold, Color.white, 14f, 0.24f, 1.4f, 2.6f, 3.4f, new Vector2(0f, -1.5f));
    }
}
