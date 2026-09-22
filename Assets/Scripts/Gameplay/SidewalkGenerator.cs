using System;
using System.Collections.Generic;
using UnityEngine;

namespace MindTheCrack.Gameplay
{
    /// <summary>
    /// Reverse generation (docs/game-design.md 3.2).
    ///
    /// Stones are never laid down first and checked for solvability
    /// afterwards. Instead a valid step sequence is chosen, and the cracks are
    /// placed around the resulting landing points. Every sidewalk therefore
    /// has at least one valid solution by construction.
    ///
    /// Solvable is not enough on its own: the intended path also has to be
    /// needed. Cracks therefore sit at every whole stride after a landing the
    /// player is meant to step over, so standing still and walking on runs
    /// into one. Only a plain normal step gets a safe midpoint crack.
    ///
    /// Crack placement also keeps each landing near the centre of its stone,
    /// otherwise a landing on the intended path could not score a perfect.
    /// Sides end up 0.5 or 1.0 units, which stays inside the centre fraction.
    /// </summary>
    public sealed class SidewalkGenerator
    {
        const float NormalStepGap = 1.0f;
        const float ShortSide = 0.5f;
        // Kept small: a wider jitter pushes a landing far enough off its stone's
        // centre that a perfect becomes unreachable there.
        const float MidJitterFraction = 0.04f;

        readonly List<float> _cracks = new();
        readonly List<float> _landings = new();
        readonly System.Random _rng;
        readonly float _normal, _long, _jump;

        float _lastLanding;

        public IReadOnlyList<float> Cracks => _cracks;

        /// <summary>The intended solution: one landing per step, in order.
        /// Kept so the generator can be checked against its own promise.</summary>
        public IReadOnlyList<float> Landings => _landings;
        public float GeneratedTo => _lastLanding;

        public SidewalkGenerator(int seed, float normalStep, float longStep, float jumpStep)
        {
            _rng = new System.Random(seed);
            _normal = normalStep;
            _long = longStep;
            _jump = jumpStep;

            // The player starts at 0, in the middle of the opening stone.
            _lastLanding = 0f;
            _cracks.Add(-ShortSide);
            _landings.Add(0f);
        }

        /// <summary>Metres of pure normal steps at the start of a run.</summary>
        public const float SafeOpening = 8f;

        /// <summary>0 = opening difficulty, 1 = hardest. Drives step mix.</summary>
        ///
        /// The opening is deliberately flat. With the curve starting at 0 m,
        /// a fifth of the very first steps already demanded a long step, so a
        /// player who had not been taught anything yet walked into a crack
        /// within about four steps. docs/onboarding.md asks for the first
        /// seconds to be unloseable and input-free; this is where that starts.
        public static float DifficultyAt(float distance) =>
            Mathf.Clamp01((distance - SafeOpening) / 400f);

        public void EnsureAhead(float x, float lookahead = 20f)
        {
            while (_lastLanding < x + lookahead)
                AppendStep(DifficultyAt(_lastLanding));
        }

        void AppendStep(float difficulty)
        {
            float step = PickStep(difficulty);
            float next = _lastLanding + step;
            PlaceCracks(_lastLanding, next);
            _lastLanding = next;
            _landings.Add(next);
        }

        float PickStep(float difficulty)
        {
            if (difficulty <= 0f) return _normal;

            // Long-step share 20% -> 55%, jump share 5% -> 25% across the curve.
            double longShare = Mathf.Lerp(0.20f, 0.55f, difficulty);
            double jumpShare = Mathf.Lerp(0.05f, 0.25f, difficulty);
            double roll = _rng.NextDouble();

            if (roll < jumpShare) return _jump;
            if (roll < jumpShare + longShare) return _long;
            return _normal;
        }

        void PlaceCracks(float from, float to)
        {
            float gap = to - from;

            if (gap <= NormalStepGap + 0.01f)
            {
                // Normal step: one crack midway, so walking on is safe here.
                float jitter = (float)(_rng.NextDouble() * 2 - 1) * gap * MidJitterFraction;
                Add(from + gap * 0.5f + jitter);
                return;
            }

            // Longer step: put a crack at every whole stride from the previous
            // landing, so a player who does nothing walks straight into one.
            //
            // The first version placed cracks a fixed distance from each landing
            // and filled the middle evenly. That guaranteed the intended path
            // was solvable but not that it was needed: a constant 1.0 stride
            // drifted across the filler stones and survived indefinitely. On
            // device the character walked 1000 metres and scored 1600 points
            // with no input at all. A sidewalk that does not punish doing
            // nothing has no decision in it.
            for (float offset = NormalStepGap; offset < gap - 0.05f; offset += NormalStepGap)
                Add(from + offset);
        }

        void Add(float x)
        {
            // Guard against degenerate spacing from jitter or rounding.
            if (_cracks.Count > 0 && x - _cracks[^1] < 0.25f) return;
            _cracks.Add(x);
        }

        /// <summary>Stone containing x, as the two cracks bounding it.</summary>
        public bool StoneAt(float x, out float left, out float right)
        {
            left = right = 0f;
            if (_cracks.Count < 2) return false;

            int i = _cracks.BinarySearch(x);
            if (i < 0) i = ~i;

            if (i <= 0 || i >= _cracks.Count) return false;
            left = _cracks[i - 1];
            right = _cracks[i];
            return true;
        }

        public float DistanceToNearestCrack(float x)
        {
            if (!StoneAt(x, out float left, out float right)) return float.MaxValue;
            return Mathf.Min(x - left, right - x);
        }

        /// <summary>Drops cracks the camera has left behind.</summary>
        public void TrimBehind(float x)
        {
            int keep = 0;
            while (keep < _cracks.Count && _cracks[keep] < x) keep++;
            keep = Math.Max(0, keep - 2);
            if (keep > 0) _cracks.RemoveRange(0, keep);

            int drop = 0;
            while (drop < _landings.Count && _landings[drop] < x) drop++;
            if (drop > 1) _landings.RemoveRange(0, drop - 1);
        }
    }
}
