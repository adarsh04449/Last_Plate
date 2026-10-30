using UnityEngine;

/// <summary>Mixes gentle effects, conversational reactions, and a quiet background loop.</summary>
public class AudioManager : MonoBehaviour
{
    [Header("Mix")]
    [Range(0f, 1f)] public float effectsVolume = 0.24f;
    [Range(0f, 1f)] public float voiceVolume = 0.5f;
    [Range(0f, 1f)] public float backgroundVolume = 0.1f;
    public bool MusicEnabled { get; private set; }

    private AudioSource source;
    private AudioSource voiceSource;
    private AudioSource backgroundSource;
    private bool playbackPaused;
    private AudioClip catchClip, goldenClip, tiltClip, wrongClip, completeClip, lostClip, levelClip, finishClip;
    private AudioClip[] damageVoices, goldenVoices, completeVoices;
    private int damageIndex, goldenIndex, completeIndex;
    private int activeVoicePriority = -1;
    private float nextVoiceTime;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.volume = effectsVolume;
        voiceSource = gameObject.AddComponent<AudioSource>(); voiceSource.playOnAwake = false; voiceSource.volume = voiceVolume;
        voiceSource.spatialBlend = 0f;
        voiceSource.pitch = 1f;
        backgroundSource = gameObject.AddComponent<AudioSource>();
        backgroundSource.playOnAwake = false;
        backgroundSource.loop = true;
        backgroundSource.spatialBlend = 0f;
        backgroundSource.volume = 0f;
        backgroundSource.clip = Resources.Load<AudioClip>("Audio/peaceful_kitchen");
        MusicEnabled = PlayerPrefs.GetInt("LastPlate.MusicEnabled", 1) != 0;
        catchClip = MakeSequence("Catch", new[] { 620f, 820f }, 0.055f);
        goldenClip = MakeSequence("Golden", new[] { 880f, 1180f, 1480f }, 0.06f);
        tiltClip = MakeSequence("Tilt", new[] { 340f, 260f }, 0.045f);
        wrongClip = MakeSequence("Wrong Catch", new[] { 190f, 145f, 105f }, 0.065f);
        completeClip = MakeSequence("Order Complete", new[] { 520f, 660f, 880f, 1050f }, 0.085f);
        lostClip = MakeSequence("Life Lost", new[] { 310f, 230f, 150f }, 0.1f);
        levelClip = MakeSequence("Rush Level", new[] { 440f, 660f, 440f, 880f }, 0.07f);
        finishClip = MakeSequence("Round Finished", new[] { 720f, 620f, 520f, 390f }, 0.12f);
        damageVoices = LoadVoicePair("chef_damage");
        goldenVoices = LoadVoicePair("chef_golden");
        completeVoices = LoadVoicePair("chef_complete");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MusicEnabled = !MusicEnabled;
            PlayerPrefs.SetInt("LastPlate.MusicEnabled", MusicEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        if (playbackPaused) return;

        // Give the actor room in the mix while preserving audible gameplay feedback.
        float targetVolume = effectsVolume * (voiceSource.isPlaying ? 0.45f : 1f);
        source.volume = Mathf.MoveTowards(source.volume, targetVolume, Time.unscaledDeltaTime * 3f);
        voiceSource.volume = voiceVolume;

        bool playing = GameManager.Instance != null && GameManager.Instance.State == GameState.Playing;
        float targetBackground = playing && MusicEnabled ? backgroundVolume * (voiceSource.isPlaying ? 0.35f : 1f) : 0f;
        if (targetBackground > 0f && !backgroundSource.isPlaying && backgroundSource.clip != null)
            backgroundSource.Play();
        // Fade using real time so the round-end fade still works when gameplay is frozen.
        backgroundSource.volume = Mathf.MoveTowards(backgroundSource.volume, targetBackground, Time.unscaledDeltaTime * 0.15f);
        if (targetBackground == 0f && backgroundSource.volume == 0f && backgroundSource.isPlaying)
            backgroundSource.Stop();
    }

    public void PlayCatch(bool golden)
    {
        source.PlayOneShot(golden ? goldenClip : catchClip);
        if (golden) PlayVoice(goldenVoices, ref goldenIndex, 6f, 0);
    }
    public void PlayTilt() { source.PlayOneShot(tiltClip, 0.45f); }
    public void PlayOrderComplete() { source.PlayOneShot(completeClip, 0.7f); PlayVoice(completeVoices, ref completeIndex, 4f, 1); }
    public void PlayLifeLost() { source.PlayOneShot(lostClip, 0.65f); PlayVoice(damageVoices, ref damageIndex, 3f, 2); }
    public void PlayWrongIngredient() { source.PlayOneShot(wrongClip, 0.8f); }
    public void PlayLevelUp() { source.PlayOneShot(levelClip); }
    public void PlayRoundFinished() { source.PlayOneShot(finishClip); }

    public void ResetPlayback()
    {
        source.Stop();
        voiceSource.Stop();
        backgroundSource.Stop();
        backgroundSource.volume = 0f;
        playbackPaused = false;
        source.volume = effectsVolume;
        nextVoiceTime = 0f;
        activeVoicePriority = -1;
    }

    public void PausePlayback()
    {
        playbackPaused = true;
        source.Pause();
        voiceSource.Pause();
        backgroundSource.Pause();
    }

    public void ResumePlayback()
    {
        playbackPaused = false;
        source.UnPause();
        voiceSource.UnPause();
        backgroundSource.UnPause();
    }

    private static AudioClip[] LoadVoicePair(string name)
    {
        return new[] {
            Resources.Load<AudioClip>("Audio/" + name + "_01"),
            Resources.Load<AudioClip>("Audio/" + name + "_02")
        };
    }

    private void PlayVoice(AudioClip[] clips, ref int nextIndex, float cooldown, int priority)
    {
        // Let a conversational reaction finish naturally; never cut one off mid-sentence.
        if (playbackPaused || voiceSource.isPlaying) return;
        if (Time.unscaledTime < nextVoiceTime && priority <= activeVoicePriority) return;
        AudioClip clip = clips[nextIndex];
        if (clip == null) return;
        nextIndex = (nextIndex + 1) % clips.Length;
        voiceSource.Stop();
        voiceSource.clip = clip;
        activeVoicePriority = priority;
        source.volume = effectsVolume * 0.45f;
        voiceSource.Play();
        nextVoiceTime = Time.unscaledTime + cooldown;
    }

    private static AudioClip MakeSequence(string name, float[] notes, float noteLength)
    {
        const int sampleRate = 44100;
        int noteSamples = Mathf.CeilToInt(sampleRate * noteLength);
        float[] samples = new float[noteSamples * notes.Length];
        for (int note = 0; note < notes.Length; note++)
        for (int i = 0; i < noteSamples; i++)
        {
            float envelope = Mathf.Clamp01(i / (sampleRate * 0.012f)) * Mathf.Pow(1f - i / (float)noteSamples, 2f);
            samples[note * noteSamples + i] = Mathf.Sin(2f * Mathf.PI * notes[note] * i / sampleRate) * envelope * 0.55f;
        }
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false); clip.SetData(samples, 0); return clip;
    }
}
