using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// UI sound effects and ambient background music. Place one in the scene; it survives scene reloads
/// (Retry / Play again / Home) so the music is never restarted or doubled.
/// Clips are loaded from Resources/CyberAudio (see Cyber > Build Audio).
/// </summary>
public class CyberAudio : MonoBehaviour
{
    public enum Cue { Click, Confirm, Back, Pause, Resume, Hint, Submit, Win, Fail, Final, TimeUp }

    public static CyberAudio Instance { get; private set; }

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.22f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    [Header("Behaviour")]
    [Tooltip("Music level while the game is paused or a jingle plays, as a fraction of Music Volume.")]
    [Range(0f, 1f)] public float duckedFraction = 0.35f;
    public float fadeSeconds = 1.2f;
    [Tooltip("Ignore the same cue if it was already played this recently (guards against double triggers).")]
    public float minRepeatSeconds = 0.08f;

    AudioSource music, sfx;
    readonly Dictionary<Cue, AudioClip> clips = new Dictionary<Cue, AudioClip>();
    readonly Dictionary<Cue, float> lastPlayed = new Dictionary<Cue, float>();
    readonly HashSet<int> hooked = new HashSet<int>();
    float duck = 1f, duckTarget = 1f, duckHold;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        music = gameObject.AddComponent<AudioSource>();
        music.clip = Resources.Load<AudioClip>("CyberAudio/music_ambient");
        music.loop = true;
        music.playOnAwake = false;
        music.volume = 0f;
        music.ignoreListenerPause = true;

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        Map(Cue.Click, "sfx_click"); Map(Cue.Confirm, "sfx_confirm"); Map(Cue.Back, "sfx_back");
        Map(Cue.Pause, "sfx_pause"); Map(Cue.Resume, "sfx_resume"); Map(Cue.Hint, "sfx_hint");
        Map(Cue.Submit, "sfx_submit"); Map(Cue.Win, "sfx_win"); Map(Cue.Fail, "sfx_fail");
        Map(Cue.Final, "sfx_final"); Map(Cue.TimeUp, "sfx_timeup");

        if (music.clip != null) music.Play();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Map(Cue cue, string file) { clips[cue] = Resources.Load<AudioClip>("CyberAudio/" + file); }

    void Start() { HookButtons(); }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        duck = duckTarget = 1f;
        HookButtons();
    }

    // ---- public API (all null-safe through the static wrappers) --------------------------------

    public static void Play(Cue cue)
    {
        if (Instance != null) Instance.PlayCue(cue);
    }

    /// <summary>Lowers the music; pass a hold time (unscaled seconds) for a jingle, or 0 to hold until Unduck.</summary>
    public static void Duck(float holdSeconds = 0f)
    {
        if (Instance == null) return;
        Instance.duckTarget = Instance.duckedFraction;
        Instance.duckHold = holdSeconds > 0f ? Time.unscaledTime + holdSeconds : float.MaxValue;
    }

    public static void Unduck()
    {
        if (Instance != null) { Instance.duckTarget = 1f; Instance.duckHold = 0f; }
    }

    public void PlayCue(Cue cue)
    {
        AudioClip clip;
        if (!clips.TryGetValue(cue, out clip) || clip == null) return;

        float last;
        if (lastPlayed.TryGetValue(cue, out last) && Time.unscaledTime - last < minRepeatSeconds) return;
        lastPlayed[cue] = Time.unscaledTime;

        sfx.PlayOneShot(clip, sfxVolume);
    }

    // ---- button hookup -------------------------------------------------------------------------

    void HookButtons()
    {
        foreach (Button b in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (!b.gameObject.scene.IsValid() || !hooked.Add(b.GetInstanceID())) continue;
            Cue cue = CueFor(b.name.ToLowerInvariant());
            b.onClick.AddListener(() => PlayCue(cue));
        }
    }

    static Cue CueFor(string n)
    {
        if (n.Contains("pause")) return Cue.Pause;
        if (n.Contains("resume")) return Cue.Resume;
        if (n.Contains("submit")) return Cue.Submit;
        if (n.Contains("hint")) return Cue.Hint;
        if (n.Contains("exit") || n.Contains("home") || n.Contains("level")) return Cue.Back;
        if (n.Contains("start") || n.Contains("retry") || n.Contains("next") || n.Contains("continue") ||
            n.Contains("easy") || n.Contains("medium") || n.Contains("difficult") || n.Contains("replay")) return Cue.Confirm;
        return Cue.Click;
    }

    // ---- music level ---------------------------------------------------------------------------

    void Update()
    {
        if (music == null || music.clip == null) return;

        if (duckHold > 0f && duckHold != float.MaxValue && Time.unscaledTime >= duckHold) { duckTarget = 1f; duckHold = 0f; }

        float dt = Time.unscaledDeltaTime;
        duck = Mathf.MoveTowards(duck, duckTarget, dt / Mathf.Max(0.05f, fadeSeconds));
        float target = musicVolume * duck;
        music.volume = Mathf.MoveTowards(music.volume, target, dt * musicVolume / Mathf.Max(0.05f, fadeSeconds));
        if (!music.isPlaying) music.Play();
    }
}
