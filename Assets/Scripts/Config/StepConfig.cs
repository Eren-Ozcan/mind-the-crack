using UnityEngine;

namespace MindTheCrack.Config
{
    public enum StepKind { Normal, Long, Jump }

    /// <summary>
    /// Every tunable gameplay number lives here, never inline in the systems.
    /// These are the values that get played with most after a test round
    /// (docs/game-design.md 1.2).
    ///
    /// Economy numbers do NOT belong in this file - those come from
    /// docs/economy.md via their own asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Mind the Crack/Step Config")]
    public sealed class StepConfig : ScriptableObject
    {
        [Header("Step distances (units)")]
        public float NormalStep = 1.0f;
        public float LongStep = 1.5f;
        public float JumpStep = 2.5f;

        [Header("Timing windows (seconds)")]
        [Tooltip("Input earlier than this before the beat is ignored.")]
        public double EarlyWindow = 0.12;
        [Tooltip("Input later than this after the beat is ignored. The conductor's CommitDelay is set from it.")]
        public double LateWindow = 0.12;
        [Tooltip("Absolute deviation at or under this counts as perfect timing.")]
        public double PerfectWindow = 0.06;

        [Header("Tutorial (first 3 runs)")]
        [Tooltip("Windows are widened by this factor while the tutorial is active.")]
        public double TutorialWindowScale = 1.7;

        [Header("Landing")]
        [Tooltip("Foot must land inside this fraction of the stone, measured from its centre, to be perfect.")]
        public float PerfectCentreFraction = 0.4f;
        [Tooltip("Landing this close to a crack kills.")]
        public float DeadlyRadius = 0.08f;
        [Tooltip("Landing this close to a crack stumbles.")]
        public float StumbleRadius = 0.14f;

        [Header("Scoring")]
        public int PerfectScore = 3;
        public int GoodScore = 1;
        [Tooltip("Consecutive perfects needed for each multiplier step.")]
        public int[] MultiplierThresholds = { 5, 10, 20 };
        public float[] MultiplierValues = { 1f, 1.5f, 2f, 3f };

        [Header("Rhythm")]
        [Tooltip("Opening tempo. Lowered from 100: at 600 ms a step there was no time to read the pavement and decide, so most beats defaulted to no input.")]
        public float StartBpm = 88f;

        public float Distance(StepKind kind) => kind switch
        {
            StepKind.Long => LongStep,
            StepKind.Jump => JumpStep,
            _ => NormalStep,
        };

        public float MultiplierFor(int perfectStreak)
        {
            int step = 0;
            for (int i = 0; i < MultiplierThresholds.Length; i++)
                if (perfectStreak >= MultiplierThresholds[i]) step = i + 1;
            return MultiplierValues[Mathf.Min(step, MultiplierValues.Length - 1)];
        }

        public static StepConfig CreateDefault()
        {
            return CreateInstance<StepConfig>();
        }
    }
}
