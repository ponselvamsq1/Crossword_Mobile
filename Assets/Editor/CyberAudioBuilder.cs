using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Synthesises the UI sound effects and the ambient background loop as WAV files.
/// Run from the menu: Cyber > Build Audio. Output goes to Assets/Resources/CyberAudio (loaded by CyberAudio at runtime).
/// </summary>
public static class CyberAudioBuilder
{
    const string Folder = "Assets/Resources/CyberAudio";
    const int Rate = 32000;

    [MenuItem("Cyber/Build Audio")]
    public static void Build()
    {
        Directory.CreateDirectory(Folder);

        Save("sfx_click",   Click(),   false);
        Save("sfx_confirm", Confirm(), false);
        Save("sfx_back",    Back(),    false);
        Save("sfx_pause",   Sweep(900f, 380f, 0.24f, 0.45f), false);
        Save("sfx_resume",  Sweep(380f, 900f, 0.24f, 0.45f), false);
        Save("sfx_hint",    Sparkle(new[] { 1318.5f, 1568f, 2093f }, 0.07f, 0.55f), false);
        Save("sfx_submit",  Submit(),  false);
        Save("sfx_win",     Arpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.11f, 1.5f, true), false);
        Save("sfx_fail",    Arpeggio(new[] { 392f, 329.63f }, 0.2f, 0.75f, false), false);
        Save("sfx_final",   Fanfare(), false);
        Save("sfx_timeup",  Arpeggio(new[] { 440f, 349.23f, 293.66f }, 0.28f, 1.1f, false), false);
        Save("music_ambient", Ambient(), true);

        AssetDatabase.Refresh();
        Debug.Log("Cyber audio built.");
    }

    // ---- helpers -----------------------------------------------------------------------------

    static float[] Mono(float seconds) { return new float[Mathf.CeilToInt(seconds * Rate)]; }

    static void Add(float[] buf, float start, float freq, float len, float amp, float attack, float decayPow, int harmonics = 1)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int n = Mathf.RoundToInt(len * Rate);
        for (int i = 0; i < n && s0 + i < buf.Length; i++)
        {
            float t = i / (float)Rate;
            float env = Mathf.Min(1f, t / Mathf.Max(attack, 1e-4f)) * Mathf.Pow(1f - i / (float)n, decayPow);
            float v = Mathf.Sin(2f * Mathf.PI * freq * t);
            for (int h = 2; h <= harmonics; h++) v += Mathf.Sin(2f * Mathf.PI * freq * h * t) * (0.35f / h);
            buf[s0 + i] += v * env * amp;
        }
    }

    static float[] Click()
    {
        float[] b = Mono(0.07f);
        Add(b, 0f, 2100f, 0.05f, 0.5f, 0.001f, 3f);
        Add(b, 0f, 1050f, 0.06f, 0.25f, 0.001f, 3f);
        return b;
    }

    static float[] Confirm()
    {
        float[] b = Mono(0.26f);
        Add(b, 0.00f, 784f,  0.12f, 0.45f, 0.004f, 2f, 3);
        Add(b, 0.07f, 1175f, 0.18f, 0.45f, 0.004f, 2.2f, 3);
        return b;
    }

    static float[] Back()
    {
        float[] b = Mono(0.2f);
        Add(b, 0.00f, 740f, 0.09f, 0.4f, 0.003f, 2f, 2);
        Add(b, 0.06f, 494f, 0.14f, 0.4f, 0.003f, 2.2f, 2);
        return b;
    }

    static float[] Sweep(float from, float to, float seconds, float amp)
    {
        float[] b = Mono(seconds);
        float phase = 0f;
        for (int i = 0; i < b.Length; i++)
        {
            float p = i / (float)b.Length;
            float f = Mathf.Lerp(from, to, p * p * (3f - 2f * p));
            phase += 2f * Mathf.PI * f / Rate;
            float env = Mathf.Min(1f, i / (0.01f * Rate)) * Mathf.Pow(1f - p, 1.6f);
            b[i] = (Mathf.Sin(phase) + 0.3f * Mathf.Sin(phase * 2f)) * env * amp;
        }
        return b;
    }

    static float[] Sparkle(float[] notes, float gap, float amp)
    {
        float[] b = Mono(gap * notes.Length + 0.5f);
        for (int i = 0; i < notes.Length; i++) Add(b, i * gap, notes[i], 0.32f, amp / (1f + i * 0.2f), 0.002f, 2.4f);
        return b;
    }

    static float[] Submit()
    {
        float[] b = Mono(0.4f);
        Add(b, 0.00f, 880f,  0.07f, 0.4f, 0.002f, 2f, 2);
        Add(b, 0.08f, 880f,  0.07f, 0.4f, 0.002f, 2f, 2);
        Add(b, 0.17f, 1320f, 0.22f, 0.45f, 0.003f, 2.4f, 3);
        return b;
    }

    static float[] Arpeggio(float[] notes, float gap, float seconds, bool shimmer)
    {
        float[] b = Mono(seconds);
        for (int i = 0; i < notes.Length; i++)
        {
            Add(b, i * gap, notes[i], seconds - i * gap, 0.38f, 0.006f, 2.2f, 3);
            if (shimmer) Add(b, i * gap + 0.03f, notes[i] * 2f, 0.5f, 0.12f, 0.004f, 3f);
        }
        return b;
    }

    static float[] Fanfare()
    {
        float[] b = Mono(3.2f);
        float[] melody = { 523.25f, 659.25f, 783.99f, 1046.5f, 783.99f, 1046.5f, 1318.5f };
        float[] when   = { 0.00f,   0.14f,   0.28f,   0.42f,   0.80f,   0.94f,   1.12f };
        for (int i = 0; i < melody.Length; i++) Add(b, when[i], melody[i], 1.9f, 0.32f, 0.006f, 2.4f, 3);
        // final chord with sparkles
        foreach (float f in new[] { 523.25f, 659.25f, 783.99f }) Add(b, 1.5f, f, 1.6f, 0.2f, 0.01f, 1.8f, 2);
        for (int i = 0; i < 9; i++) Add(b, 1.45f + i * 0.09f, 1568f * (1f + (i % 3) * 0.335f), 0.4f, 0.1f, 0.002f, 3f);
        return b;
    }

    // ---- background loop: warm lo-fi groove (D major, 92 bpm, 16 bars) -------------------------

    static float Hz(int midi) { return 440f * Mathf.Pow(2f, (midi - 69) / 12f); }

    static void Tone(float[] L, float[] R, int n, float start, float freq, float len, float amp, float pan,
                     float attack, float decayRate, float h2, float h2Decay)
    {
        float gl = Mathf.Clamp01(0.5f - pan * 0.5f) + 0.25f, gr = Mathf.Clamp01(0.5f + pan * 0.5f) + 0.25f;
        int s0 = Mathf.RoundToInt(start * Rate);
        int count = Mathf.RoundToInt(len * Rate);
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)Rate;
            float fade = Mathf.Clamp01((len - t) / 0.05f);
            float env = Mathf.Min(1f, t / attack) * Mathf.Exp(-t * decayRate) * fade;
            float v = Mathf.Sin(2f * Mathf.PI * freq * t) + h2 * Mathf.Sin(4f * Mathf.PI * freq * t) * Mathf.Exp(-t * h2Decay);
            v *= env * amp;
            int idx = (s0 + i) % n;
            L[idx] += v * gl; R[idx] += v * gr;
        }
    }

    static void Kick(float[] L, float[] R, int n, float start, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate), count = Mathf.RoundToInt(0.2f * Rate);
        float phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)Rate;
            phase += 2f * Mathf.PI * (45f + 70f * Mathf.Exp(-t * 28f)) / Rate;
            float v = Mathf.Sin(phase) * Mathf.Exp(-t * 18f) * Mathf.Min(1f, t / 0.002f) * amp;
            int idx = (s0 + i) % n;
            L[idx] += v; R[idx] += v;
        }
    }

    static void Shaker(float[] L, float[] R, int n, float start, float amp, float pan, ref uint seed, float len)
    {
        int s0 = Mathf.RoundToInt(start * Rate), count = Mathf.RoundToInt(len * Rate);
        float gl = Mathf.Clamp01(0.5f - pan * 0.5f) + 0.25f, gr = Mathf.Clamp01(0.5f + pan * 0.5f) + 0.25f;
        float prev = 0f;
        for (int i = 0; i < count; i++)
        {
            seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5;
            float nz = (seed & 0xffff) / 32768f - 1f;
            float hp = nz - prev; prev = nz;                    // crude high-pass
            float t = i / (float)Rate;
            float v = hp * Mathf.Exp(-t * (len > 0.06f ? 45f : 70f)) * Mathf.Min(1f, t / 0.002f) * amp;
            int idx = (s0 + i) % n;
            L[idx] += v * gl; R[idx] += v * gr;
        }
    }

    static float[] Ambient()
    {
        const float Bpm = 92f;
        const int Bars = 16;
        float beat = 60f / Bpm, bar = beat * 4f;
        float T = bar * Bars;
        int n = Mathf.RoundToInt(T * Rate);
        float[] L = new float[n], R = new float[n];

        int[][] chords =
        {
            new[] { 57, 61, 66, 69 },   // Dmaj7
            new[] { 57, 62, 66, 69 },   // Bm7
            new[] { 59, 62, 66, 71 },   // Gmaj7
            new[] { 57, 61, 64, 66 },   // A6
        };
        int[] roots = { 50, 47, 43, 45 };
        int[] pent = { 74, 76, 78, 81, 83, 86 };    // D major pentatonic
        string[] melody =
        {
            "0..2..3.", "5..3..2.", "..2.3.5.", "3.2.0...",
            "2..3..5.", "4..3..2.", "..0.2.3.", "2.....0.",
            "5..3..5.", "3..2..0.", "..2.3.2.", "0.......",
            "3.5.3.2.", "0.2.3...", "5..3.2.0", "2.......",
        };
        uint seed = 2463534242u;

        for (int b = 0; b < Bars; b++)
        {
            float t0 = b * bar;
            int ci = b % 4;

            // electric-piano style chord, lightly strummed
            for (int k = 0; k < chords[ci].Length; k++)
                Tone(L, R, n, t0 + k * 0.014f, Hz(chords[ci][k]), 2.4f, 0.075f, -0.45f + 0.3f * k, 0.008f, 1.5f, 0.35f, 5f);

            // bass
            Tone(L, R, n, t0, Hz(roots[ci]), 0.7f, 0.26f, 0f, 0.006f, 3.6f, 0.3f, 6f);
            Tone(L, R, n, t0 + beat * 2f, Hz(roots[ci]), 0.6f, 0.2f, 0f, 0.006f, 4f, 0.3f, 6f);
            if (b % 2 == 1) Tone(L, R, n, t0 + beat * 3.5f, Hz(roots[(ci + 1) % 4]), 0.4f, 0.14f, 0f, 0.006f, 5f, 0.3f, 6f);

            // soft kick and shaker
            Kick(L, R, n, t0, 0.2f);
            Kick(L, R, n, t0 + beat * 2f, 0.14f);
            for (int e = 0; e < 8; e++)
                Shaker(L, R, n, t0 + e * beat * 0.5f, (e % 2 == 1) ? 0.05f : 0.022f, (e % 2 == 1) ? 0.3f : -0.3f, ref seed, 0.05f);
            Shaker(L, R, n, t0 + beat, 0.06f, 0f, ref seed, 0.09f);
            Shaker(L, R, n, t0 + beat * 3f, 0.06f, 0f, ref seed, 0.09f);

            // pluck melody with dotted-eighth echo
            for (int s = 0; s < 8; s++)
            {
                char c = melody[b][s];
                if (c == '.') continue;
                float f = Hz(pent[c - '0']);
                float st = t0 + s * beat * 0.5f;
                Tone(L, R, n, st, f, 1.1f, 0.07f, -0.25f, 0.004f, 3.8f, 0.4f, 9f);
                Tone(L, R, n, st + beat * 0.75f, f, 0.9f, 0.026f, 0.5f, 0.004f, 4.2f, 0.4f, 9f);
            }
        }

        float peak = 0f;
        for (int i = 0; i < n; i++) peak = Mathf.Max(peak, Mathf.Abs(L[i]), Mathf.Abs(R[i]));
        float g = 0.5f / Mathf.Max(peak, 1e-4f);

        float[] inter = new float[n * 2];
        for (int i = 0; i < n; i++) { inter[2 * i] = L[i] * g; inter[2 * i + 1] = R[i] * g; }
        return inter;
    }

    // ---- output ------------------------------------------------------------------------------

    static void Save(string name, float[] samples, bool stereoMusic)
    {
        int channels = stereoMusic ? 2 : 1;
        float peak = 0f;
        foreach (float s in samples) peak = Mathf.Max(peak, Mathf.Abs(s));
        float norm = stereoMusic ? 1f : Mathf.Min(1f, 0.85f / Mathf.Max(peak, 1e-4f));

        string path = Folder + "/" + name + ".wav";
        using (FileStream fs = new FileStream(path, FileMode.Create))
        using (BinaryWriter w = new BinaryWriter(fs))
        {
            int dataBytes = samples.Length * 2;
            w.Write(new[] { 'R', 'I', 'F', 'F' }); w.Write(36 + dataBytes); w.Write(new[] { 'W', 'A', 'V', 'E' });
            w.Write(new[] { 'f', 'm', 't', ' ' }); w.Write(16); w.Write((short)1); w.Write((short)channels);
            w.Write(Rate); w.Write(Rate * channels * 2); w.Write((short)(channels * 2)); w.Write((short)16);
            w.Write(new[] { 'd', 'a', 't', 'a' }); w.Write(dataBytes);
            foreach (float s in samples) w.Write((short)Mathf.RoundToInt(Mathf.Clamp(s * norm, -1f, 1f) * 32000f));
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AudioImporter ai = (AudioImporter)AssetImporter.GetAtPath(path);
        AudioImporterSampleSettings ss = ai.defaultSampleSettings;
        ss.loadType = stereoMusic ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
        ss.compressionFormat = AudioCompressionFormat.Vorbis;
        ss.quality = 0.6f;
        ai.defaultSampleSettings = ss;
        ai.forceToMono = !stereoMusic;
        ai.SaveAndReimport();
    }
}
