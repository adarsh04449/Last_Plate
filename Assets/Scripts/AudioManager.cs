using UnityEngine;

/// <summary>Creates a small original arcade sound set at runtime. There is deliberately no music loop.</summary>
public class AudioManager : MonoBehaviour
{
    private AudioSource source;
    private AudioClip catchClip, goldenClip, tiltClip, wrongClip, completeClip, lostClip, levelClip, finishClip;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.volume = 0.45f;
        catchClip = MakeSequence("Catch", new[] { 620f, 820f }, 0.055f);
        goldenClip = MakeSequence("Golden", new[] { 880f, 1180f, 1480f }, 0.06f);
        tiltClip = MakeSequence("Tilt", new[] { 340f, 260f }, 0.045f);
        wrongClip = MakeSequence("Wrong Catch", new[] { 190f, 145f, 105f }, 0.065f);
        completeClip = MakeSequence("Order Complete", new[] { 520f, 660f, 880f, 1050f }, 0.085f);
        lostClip = MakeSequence("Life Lost", new[] { 310f, 230f, 150f }, 0.1f);
        levelClip = MakeSequence("Rush Level", new[] { 440f, 660f, 440f, 880f }, 0.07f);
        finishClip = MakeSequence("Round Finished", new[] { 720f, 620f, 520f, 390f }, 0.12f);
    }

    public void PlayCatch(bool golden) { source.PlayOneShot(golden ? goldenClip : catchClip); }
    public void PlayTilt() { source.PlayOneShot(tiltClip, 0.45f); }
    public void PlayOrderComplete() { source.PlayOneShot(completeClip); }
    public void PlayLifeLost() { source.PlayOneShot(lostClip); }
    public void PlayWrongIngredient() { source.PlayOneShot(wrongClip, 0.8f); }
    public void PlayLevelUp() { source.PlayOneShot(levelClip); }
    public void PlayRoundFinished() { source.PlayOneShot(finishClip); }

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
