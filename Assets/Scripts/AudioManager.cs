using UnityEngine;

/// <summary>Mixes arcade effects with human-recorded chef reactions. There is no music loop.</summary>
public class AudioManager : MonoBehaviour
{
    [Header("Mix")]
    [Range(0f, 1f)] public float effectsVolume = 0.35f;
    [Range(0f, 1f)] public float voiceVolume = 0.8f;

    private AudioSource source;
    private AudioSource voiceSource;
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
        // Give the actor room in the mix while preserving audible gameplay feedback.
        float targetVolume = effectsVolume * (voiceSource.isPlaying ? 0.35f : 1f);
        source.volume = Mathf.MoveTowards(source.volume, targetVolume, Time.unscaledDeltaTime * 3f);
        voiceSource.volume = voiceVolume;
    }

    public void PlayCatch(bool golden)
    {
        source.PlayOneShot(golden ? goldenClip : catchClip);
        if (golden) PlayVoice(goldenVoices, ref goldenIndex, 2.5f, 0);
    }
    public void PlayTilt() { source.PlayOneShot(tiltClip, 0.45f); }
    public void PlayOrderComplete() { source.PlayOneShot(completeClip); PlayVoice(completeVoices, ref completeIndex, 1.6f, 1); }
    public void PlayLifeLost() { source.PlayOneShot(lostClip); PlayVoice(damageVoices, ref damageIndex, 1.2f, 2); }
    public void PlayWrongIngredient() { source.PlayOneShot(wrongClip, 0.8f); }
    public void PlayLevelUp() { source.PlayOneShot(levelClip); }
    public void PlayRoundFinished() { source.PlayOneShot(finishClip); }

    public void ResetPlayback()
    {
        source.Stop();
        voiceSource.Stop();
        source.volume = effectsVolume;
        nextVoiceTime = 0f;
        activeVoicePriority = -1;
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
        // Damage can interrupt a cheer; repeated damage cannot constantly restart the voice.
        if ((voiceSource.isPlaying || Time.unscaledTime < nextVoiceTime) && priority <= activeVoicePriority) return;
        AudioClip clip = clips[nextIndex];
        if (clip == null) return;
        nextIndex = (nextIndex + 1) % clips.Length;
        voiceSource.Stop();
        voiceSource.clip = clip;
        activeVoicePriority = priority;
        source.volume = effectsVolume * 0.35f;
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
            float envelope = 1f - i / (float)noteSamples;
            samples[note * noteSamples + i] = Mathf.Sin(2f * Mathf.PI * notes[note] * i / sampleRate) * envelope * 0.55f;
        }
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false); clip.SetData(samples, 0); return clip;
    }
}
