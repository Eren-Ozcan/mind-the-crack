using System;
using UnityEngine;

namespace MindTheCrack.Rhythm
{
    /// <summary>
    /// Beat clock. Everything rhythmic in the game reads time from here.
    ///
    /// Rules (docs/game-design.md 2.1):
    ///   - Time comes from AudioSettings.dspTime, never Time.time.
    ///   - The track is started with AudioSource.PlayScheduled.
    ///   - dspTime only advances when the audio thread fills a buffer, so it
    ///     steps rather than flows. Between updates we extrapolate with
    ///     realtime, otherwise input deviation quantises to the buffer size.
    ///
    /// Commit delay: a step for beat N resolves at N * secPerBeat + CommitDelay
    /// rather than exactly on the beat, so an input that lands slightly late
    /// still counts. Without it the late half of the timing window is
    /// unreachable - the step would already have been taken.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class Conductor : MonoBehaviour
    {
        [SerializeField] float _bpm = 100f;

        const string CalibrationKey = "mtc.calibration";

        /// <summary>Device audio latency, seconds. Positive = player hears late.</summary>
        public double CalibrationOffset;

        /// <summary>
        /// How long after a beat its step resolves.
        ///
        /// This is what makes the late half of the timing window reachable, so
        /// it must be at least the late window or inputs past it arrive after
        /// the step was already taken and get judged against the next beat.
        /// The character still lands exactly on the following beat (the move
        /// is shortened by the same amount), so the delay costs departure
        /// crispness, not sync.
        /// </summary>
        public double CommitDelay = 0.12;

        /// <summary>
        /// Stage 1 calibration from docs/game-design.md 2.1: the first inputs
        /// of a run are averaged silently and become the offset. A player
        /// whose device is 100 ms late taps 100 ms late on every beat, which
        /// without this is indistinguishable from being bad at the game.
        /// </summary>
        /// <summary>Deviations per correction. Small on purpose - see ObserveDeviation.</summary>
        public int CalibrationSamples = 5;

        readonly System.Collections.Generic.List<double> _calibrationSamples = new();
        int _corrections;

        /// <summary>Raised on the beat itself. The step leaves here.</summary>
        public event Action<int> BeatStarted;

        /// <summary>Raised once per beat, CommitDelay after the beat itself.</summary>
        public event Action<int> BeatCommitted;

        AudioSource _source;
        double _dspSongStart;
        double _cachedDsp;
        float _cachedRealtime;
        int _lastStartedBeat = -1;
        int _lastCommittedBeat = -1;
        bool _running;

        public float Bpm => _bpm;
        public double SecPerBeat => 60.0 / _bpm;
        public bool Running => _running;

        /// <summary>Beat index currently being walked into.</summary>
        public int CurrentBeat => _lastCommittedBeat;

        /// <summary>Seconds since the first beat, calibration applied.</summary>
        public double SongTime => DspNow - _dspSongStart - CalibrationOffset;

        double DspNow
        {
            get
            {
                double dsp = AudioSettings.dspTime;
                if (dsp != _cachedDsp)
                {
                    _cachedDsp = dsp;
                    _cachedRealtime = Time.realtimeSinceStartup;
                }
                return _cachedDsp + (Time.realtimeSinceStartup - _cachedRealtime);
            }
        }

        public double TimeOfBeat(int beat) => beat * SecPerBeat;

        /// <summary>Signed distance from a song time to its nearest beat, seconds.</summary>
        public double DeviationToNearestBeat(double songTime)
        {
            double spb = SecPerBeat;
            double beats = songTime / spb;
            double nearest = Math.Round(beats);
            return (beats - nearest) * spb;
        }

        /// <summary>
        /// Feeds one measured deviation into the silent estimate. Ignored once
        /// enough samples are in; the explicit calibration screen takes over.
        /// </summary>
        public void ObserveDeviation(double deviation)
        {
            // A press meant for a different beat than the one that read it is
            // noise, not latency.
            if (Math.Abs(deviation) > SecPerBeat * 0.5) return;

            _calibrationSamples.Add(deviation);
            if (_calibrationSamples.Count < CalibrationSamples) return;

            // Median, not mean: one fumbled tap should not drag the offset the
            // player then has to live with.
            _calibrationSamples.Sort();
            int mid = _calibrationSamples.Count / 2;
            double median = _calibrationSamples.Count % 2 == 1
                ? _calibrationSamples[mid]
                : (_calibrationSamples[mid - 1] + _calibrationSamples[mid]) * 0.5;

            // Corrects every few inputs instead of once per run. Waiting for a
            // long batch meant a player who gives an input on a tenth of the
            // beats spent two minutes fighting the latency the estimate was
            // there to remove - and by then had mostly stopped playing.
            //
            // The step shrinks as 1/(n+1): the first correction lands whole,
            // then a half, a third, a quarter. A fixed half-step never settled
            // - the offset bounced between 83 and 122 ms on a real device and
            // the bouncing showed up as timing scatter of its own.
            CalibrationOffset += median / (_corrections + 1);
            _corrections++;

            // Later batches are longer: once the offset is roughly right, the
            // remaining signal is smaller than the noise in five taps.
            if (_corrections >= 2) CalibrationSamples = 9;

            // The samples measured the old offset, so they are spent.
            _calibrationSamples.Clear();
            PlayerPrefs.SetFloat(CalibrationKey, (float)CalibrationOffset);
        }

        /// <summary>
        /// Starting offset: a stored calibration if the player has one, else
        /// the driver's own reported latency, which on Android is anywhere
        /// between 10 and 200 ms and is never zero.
        /// </summary>
        public void LoadCalibration()
        {
            if (PlayerPrefs.HasKey(CalibrationKey))
            {
                // A stored offset is a starting point, not a final answer: the
                // estimate keeps refining it, just in half steps from here.
                CalibrationOffset = PlayerPrefs.GetFloat(CalibrationKey);
                _corrections = 1;
                return;
            }

            var cfg = AudioSettings.GetConfiguration();
            CalibrationOffset = cfg.sampleRate > 0
                ? (double)cfg.dspBufferSize / cfg.sampleRate * 2.0
                : 0.0;
        }

        public void Begin(AudioSource source, float bpm, double leadIn = 0.5)
        {
            _source = source;
            _bpm = bpm;
            _source.loop = true;
            _dspSongStart = AudioSettings.dspTime + leadIn;
            _source.PlayScheduled(_dspSongStart);
            _lastStartedBeat = -1;
            _lastCommittedBeat = -1;
            _running = true;
        }

        public void Stop()
        {
            _running = false;
            if (_source != null) _source.Stop();
        }

        void Update()
        {
            if (!_running) return;

            // Started first: the step leaves on the beat with a default in
            // mind, and the commit an instant later only decides how far it
            // reaches. Moving on the commit instead left the character parked
            // for CommitDelay every beat and then lunging through the
            // remainder, which reads as the music and the legs disagreeing.
            int startDue = (int)Math.Floor(SongTime / SecPerBeat);
            while (_lastStartedBeat < startDue)
            {
                _lastStartedBeat++;
                BeatStarted?.Invoke(_lastStartedBeat);
            }

            int beatDue = (int)Math.Floor((SongTime - CommitDelay) / SecPerBeat);

            // Catch up one beat at a time: a hitch must not silently eat steps.
            while (_lastCommittedBeat < beatDue)
            {
                _lastCommittedBeat++;
                BeatCommitted?.Invoke(_lastCommittedBeat);
            }
        }
    }
}
