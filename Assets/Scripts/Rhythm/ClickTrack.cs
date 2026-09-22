using UnityEngine;

namespace MindTheCrack.Rhythm
{
    /// <summary>
    /// Procedural metronome loop. Phase 0 has no audio assets and does not
    /// need any: the vertical slice only has to answer "does stepping on the
    /// beat feel good", and a click track answers that without waiting on the
    /// music sourcing decision (docs/roadmap.md).
    ///
    /// The clip is an exact whole number of beats so the loop point sits on
    /// the grid. The beat grid itself comes from Conductor, not from clip
    /// position, so a drifting loop would be audible but not unfair.
    /// </summary>
    public static class ClickTrack
    {
        public static AudioClip Create(float bpm, int beatsPerLoop = 8, int sampleRate = 44100)
        {
            double secPerBeat = 60.0 / bpm;
            int samplesPerBeat = Mathf.RoundToInt((float)(secPerBeat * sampleRate));
            int total = samplesPerBeat * beatsPerLoop;

            var data = new float[total];

            for (int beat = 0; beat < beatsPerLoop; beat++)
            {
                bool accent = beat % 4 == 0;
                float freq = accent ? 1400f : 900f;
                float gain = accent ? 0.55f : 0.35f;
                int blipSamples = Mathf.Min(samplesPerBeat, (int)(0.035f * sampleRate));
                int start = beat * samplesPerBeat;

                for (int i = 0; i < blipSamples; i++)
                {
                    float t = i / (float)sampleRate;
                    // Exponential decay keeps the transient sharp, which is what
                    // the player actually times against.
                    float env = Mathf.Exp(-t * 90f);
                    data[start + i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * gain;
                }
            }

            var clip = AudioClip.Create($"click_{bpm:0}bpm", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
