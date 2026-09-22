using System;
using MindTheCrack.Config;
using MindTheCrack.Rhythm;
using UnityEngine;

namespace MindTheCrack.Gameplay
{
    public enum StepResult { Perfect, Good, Stumble, Dead }

    /// <summary>
    /// Phase 0 vertical slice: walk on the beat, choose the step, land on the
    /// stone. No meta, no economy, no ads, no art - the slice only has to
    /// answer whether the core is fun (docs/roadmap.md, Phase 0 gate).
    ///
    /// Death is disabled here on purpose. The gate is measured by whether
    /// people keep playing for five minutes, and an unforgiving slice
    /// measures frustration instead.
    /// </summary>
    public sealed class RunController : MonoBehaviour
    {
        public Conductor Conductor;
        public StepInputReader Input;   // polls itself, ahead of the conductor
        public StepConfig Config;
        public SidewalkView View;
        public Transform Player;
        public AudioSource Music;
        public Transform[] PreviewMarkers = Array.Empty<Transform>();

        public bool DeathEnabled;

        public SidewalkGenerator Generator { get; private set; }

        // --- run state -------------------------------------------------------
        public float Distance => _x;
        public int Score { get; private set; }
        public int PerfectStreak { get; private set; }
        public float Multiplier => Config.MultiplierFor(PerfectStreak);
        public StepResult LastResult { get; private set; } = StepResult.Good;
        public StepKind LastKind { get; private set; } = StepKind.Normal;
        public double LastDeviation { get; private set; }
        public bool LastInputIgnored { get; private set; }
        public int StepCount { get; private set; }

        float _x;
        float _stepFrom;
        float _moveFrom, _moveTo;
        double _moveStart, _moveDuration;
        float _arcHeight, _arcTarget;

        // Subscribed in Start, not OnEnable: OnEnable fires inside
        // AddComponent, before the bootstrap has assigned Conductor, so an
        // OnEnable subscription silently never happens and the run never
        // takes a step.
        void Start()
        {
            if (Conductor == null) return;
            Conductor.BeatStarted += OnBeatStart;
            Conductor.BeatCommitted += OnBeat;
        }

        void OnDestroy()
        {
            if (Conductor == null) return;
            Conductor.BeatStarted -= OnBeatStart;
            Conductor.BeatCommitted -= OnBeat;
        }

        public void StartRun(int seed)
        {
            Generator = new SidewalkGenerator(seed, Config.NormalStep, Config.LongStep, Config.JumpStep);
            Generator.EnsureAhead(0f);
            _x = 0f;
            _stepFrom = 0f;
            _moveFrom = _moveTo = 0f;
            _arcHeight = _arcTarget = LowArc;
            Score = 0;
            PerfectStreak = 0;
            StepCount = 0;
            LastResult = StepResult.Good;
            View.Rebuild(Generator);
        }

        /// <summary>
        /// The step leaves on the beat, assuming the player will do nothing.
        /// The commit an instant later either confirms that or stretches the
        /// stride mid-air, which is where the input actually gets read.
        /// </summary>
        void OnBeatStart(int beat)
        {
            if (Generator == null) return;

            _stepFrom = _x;

            float target = _x + Config.Distance(StepKind.Normal);
            Generator.EnsureAhead(target);

            _moveFrom = _x;
            _moveTo = target;
            _moveStart = Conductor.TimeOfBeat(beat);
            _moveDuration = Conductor.SecPerBeat;
            _arcTarget = LowArc;
        }

        void OnBeat(int beat)
        {
            if (Generator == null) return;

            var pending = Input.Consume();
            var kind = StepKind.Normal;
            bool hadInput = false;
            LastInputIgnored = false;
            LastDeviation = 0;

            if (pending.HasValue)
            {
                double dev = pending.SongTime - Conductor.TimeOfBeat(beat);
                double early = Config.EarlyWindow;
                double late = Config.LateWindow;

                // Fed in whether or not the window accepted it. Learning only
                // from accepted inputs meant the estimate could never see the
                // error it existed to correct: a player 140 ms late has every
                // such press rejected, so only their accidental early ones
                // reached the average and the offset settled far too low.
                Conductor.ObserveDeviation(dev);

                if (dev >= -early && dev <= late)
                {
                    kind = pending.Kind;
                    hadInput = true;
                    LastDeviation = dev;
                }
                else
                {
                    // Outside the window the input does not exist: the character
                    // takes a normal step, which is usually fatal. That is the
                    // punishment for bad timing, not a separate "miss" state.
                    LastInputIgnored = true;
                    LastDeviation = dev;
                }
            }

            float target = _stepFrom + Config.Distance(kind);
            Generator.EnsureAhead(target);

            LastResult = Evaluate(target, hadInput, LastDeviation);
            LastKind = kind;
            StepCount++;

            switch (LastResult)
            {
                case StepResult.Perfect:
                    PerfectStreak++;
                    Score += Mathf.RoundToInt(Config.PerfectScore * Multiplier);
                    break;
                case StepResult.Good:
                    PerfectStreak = 0;
                    Score += Config.GoodScore;
                    break;
                case StepResult.Stumble:
                case StepResult.Dead:
                    PerfectStreak = 0;
                    break;
            }

            Retarget(target, kind == StepKind.Jump);
            View.Advance(Generator, _moveTo);

            UnityEngine.Debug.Log(
                $"[MTC] beat={beat} kind={kind} dev={LastDeviation * 1000.0:+0;-0}ms " +
                $"ignored={LastInputIgnored} result={LastResult} x={_x:0.00} " +
                $"streak={PerfectStreak} offset={Conductor.CalibrationOffset * 1000.0:0}ms " +
                $"bpm={Conductor.Bpm:0}");
        }

        StepResult Evaluate(float landing, bool hadInput, double deviation)
        {
            float toCrack = Generator.DistanceToNearestCrack(landing);

            if (toCrack <= Config.DeadlyRadius)
                return DeathEnabled ? StepResult.Dead : StepResult.Stumble;

            if (toCrack <= Config.StumbleRadius)
                return StepResult.Stumble;

            if (!Generator.StoneAt(landing, out float left, out float right))
                return StepResult.Good;

            // The design note above this asked to revisit whether doing nothing
            // was too safe once there was data. There was: of 185 steps in a
            // session, 163 took no input at all, and those free steps produced
            // most of the perfects. Standing pat is still the safe option, it
            // just no longer outscores playing.
            if (!hadInput) return StepResult.Good;

            float centre = (left + right) * 0.5f;
            float halfLen = (right - left) * 0.5f;
            bool centred = Mathf.Abs(landing - centre) <= halfLen * Config.PerfectCentreFraction;
            bool timed = Math.Abs(deviation) <= Config.PerfectWindow;

            return centred && timed ? StepResult.Perfect : StepResult.Good;
        }

        const float LowArc = 0.12f;
        const float JumpArc = 0.7f;

        /// <summary>
        /// Points the step at a new landing without moving the character.
        /// The start of the lerp is solved backwards from where the feet
        /// already are, so a stride that grows mid-air changes speed rather
        /// than teleporting.
        /// </summary>
        void Retarget(float target, bool jump)
        {
            _x = target;
            _arcTarget = jump ? JumpArc : LowArc;

            float k = Progress();
            if (k < 0.999f)
            {
                float here = Mathf.Lerp(_moveFrom, _moveTo, k);
                _moveFrom = (here - k * target) / (1f - k);
            }
            else
            {
                _moveFrom = target;
            }

            _moveTo = target;
        }

        float Progress()
        {
            double t = (Conductor.SongTime - _moveStart) / _moveDuration;
            return Mathf.Clamp01((float)t);
        }

        /// <summary>
        /// The slice used to keep walking while the phone was in a pocket: the
        /// run was started once at launch and never stopped, so a player
        /// picking the phone up landed a hundred metres in, mid difficulty
        /// curve, with a pile of stumbles they never saw. The first sixty
        /// seconds docs/onboarding.md designs were unreachable in practice.
        /// </summary>
        void OnApplicationPause(bool paused)
        {
            if (Conductor == null) return;

            if (paused)
            {
                Conductor.Stop();
                return;
            }

            StartRun(UnityEngine.Random.Range(1, 999999));
            Conductor.Begin(Music, Config.StartBpm);
        }

        void Update()
        {
            if (Conductor == null || !Conductor.Running) return;

            float k = Progress();
            float z = Mathf.Lerp(_moveFrom, _moveTo, k);

            // Eased rather than switched: the arc is raised a fifth of the way
            // into the step, and a hard swap there is a visible hop.
            _arcHeight = Mathf.MoveTowards(_arcHeight, _arcTarget, Time.deltaTime * 6f);
            float y = Mathf.Sin(k * Mathf.PI) * _arcHeight;

            Player.position = new Vector3(0f, SidewalkView.SurfaceY + 0.375f + y, z);

            UpdatePreview();
            Generator.TrimBehind(_x - 8f);
        }

        static readonly StepKind[] Kinds = { StepKind.Normal, StepKind.Long, StepKind.Jump };

        /// <summary>
        /// Shows where each of the three options would land. Reading the
        /// sidewalk is the actual skill, so the slice has to make it readable
        /// before anything else is judged.
        ///
        /// Each option is also looked at one step further: an option that is
        /// safe now but has no safe continuation is a dead end, and is drawn
        /// dimmed. One beat was not enough to notice those in time - the
        /// player was reading the sidewalk a step too late to act on it.
        /// </summary>
        void UpdatePreview()
        {
            if (PreviewMarkers.Length < 3) return;

            for (int i = 0; i < 3; i++)
            {
                float landing = _x + Config.Distance(Kinds[i]);
                Generator.EnsureAhead(landing);

                var m = PreviewMarkers[i];
                m.position = new Vector3(0f, SidewalkView.SurfaceY + 0.02f, landing);

                float toCrack = Generator.DistanceToNearestCrack(landing);
                Color c = toCrack <= Config.DeadlyRadius ? new Color(0.9f, 0.2f, 0.2f)
                    : toCrack <= Config.StumbleRadius ? new Color(0.95f, 0.7f, 0.15f)
                    : HasSafeContinuation(landing)
                        ? new Color(0.95f, 0.95f, 0.95f)
                        : new Color(0.55f, 0.5f, 0.45f);   // safe now, dead end next

                var r = m.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial.color = c;
            }
        }

        /// <summary>True if at least one of the three steps from here clears a crack.</summary>
        bool HasSafeContinuation(float from)
        {
            for (int i = 0; i < Kinds.Length; i++)
            {
                float next = from + Config.Distance(Kinds[i]);
                Generator.EnsureAhead(next);
                if (Generator.DistanceToNearestCrack(next) > Config.StumbleRadius) return true;
            }
            return false;
        }
    }
}
