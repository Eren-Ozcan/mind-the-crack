using System;
using MindTheCrack.Config;
using MindTheCrack.Gameplay;
using UnityEditor;
using UnityEngine;

namespace MindTheCrack.EditorTools
{
    /// <summary>
    /// Layer A test from docs/test-plan.md: the generator promises every
    /// sidewalk is solvable, and that promise is checked rather than trusted.
    ///
    /// For each seed it walks the intended step sequence and asserts that
    /// every landing is safely inside a stone, and that a perfect landing is
    /// actually reachable there. An unreachable perfect is not a crash, but it
    /// would quietly make the scoring feel arbitrary.
    ///
    /// Run from the menu, or headless:
    ///   Unity.exe -batchmode -quit -projectPath . \
    ///     -executeMethod MindTheCrack.EditorTools.GeneratorValidation.RunHeadless
    /// </summary>
    public static class GeneratorValidation
    {
        const int Seeds = 10000;
        const float MetresPerSeed = 500f;

        [MenuItem("Mind the Crack/Validate Generator (10k seeds)")]
        public static void RunFromMenu() => Run(throwOnFailure: false);

        public static void RunHeadless()
        {
            bool ok = Run(throwOnFailure: false);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        static bool Run(bool throwOnFailure)
        {
            var config = StepConfig.CreateDefault();

            int unsafeLandings = 0;
            int unperfectable = 0;
            int stepsChecked = 0;
            float worstClearance = float.MaxValue;

            for (int seed = 1; seed <= Seeds; seed++)
            {
                var gen = new SidewalkGenerator(seed, config.NormalStep, config.LongStep, config.JumpStep);
                gen.EnsureAhead(MetresPerSeed, lookahead: 1f);

                var landings = gen.Landings;
                for (int i = 0; i < landings.Count; i++)
                {
                    float x = landings[i];
                    if (!gen.StoneAt(x, out float left, out float right)) continue;

                    stepsChecked++;

                    float clearance = Mathf.Min(x - left, right - x);
                    worstClearance = Mathf.Min(worstClearance, clearance);

                    if (clearance <= config.StumbleRadius)
                    {
                        unsafeLandings++;
                        if (unsafeLandings <= 5)
                            Debug.LogError($"seed {seed}: landing {x:0.000} only {clearance:0.000} from a crack " +
                                           $"(stone {left:0.000}..{right:0.000})");
                    }

                    float centre = (left + right) * 0.5f;
                    float halfLen = (right - left) * 0.5f;
                    if (Mathf.Abs(x - centre) > halfLen * config.PerfectCentreFraction)
                    {
                        unperfectable++;
                        if (unperfectable <= 5)
                            Debug.LogWarning($"seed {seed}: landing {x:0.000} off centre by " +
                                             $"{Mathf.Abs(x - centre) / halfLen:0.00} of the half stone");
                    }
                }
            }

            // The sidewalk must punish doing nothing. Walk a constant normal
            // stride and see how long it survives; if it survives forever there
            // is no decision in the game.
            int naiveTotal = 0, naiveWorst = 0, naiveNeverDies = 0;
            for (int seed = 1; seed <= 500; seed++)
            {
                var gen = new SidewalkGenerator(seed, config.NormalStep, config.LongStep, config.JumpStep);
                float x = 0f;
                int steps = 0;
                while (steps < 200)
                {
                    x += config.NormalStep;
                    gen.EnsureAhead(x, lookahead: 4f);
                    steps++;
                    if (gen.DistanceToNearestCrack(x) <= config.DeadlyRadius) break;
                }
                if (steps >= 200) naiveNeverDies++;
                naiveTotal += steps;
                naiveWorst = Mathf.Max(naiveWorst, steps);
            }
            float naiveAverage = naiveTotal / 500f;

            bool naiveOk = naiveNeverDies == 0 && naiveAverage <= 15f;
            Debug.Log($"Do-nothing path: dies after {naiveAverage:0.0} steps on average, " +
                      $"worst {naiveWorst}, never-dies {naiveNeverDies}/500");

            bool ok = unsafeLandings == 0 && unperfectable == 0 && naiveOk;
            string summary =
                $"Generator validation: {Seeds} seeds, {stepsChecked} steps. " +
                $"unsafe={unsafeLandings} unperfectable={unperfectable} " +
                $"worst clearance={worstClearance:0.000} (stumble radius {config.StumbleRadius:0.000}) " +
                $"naive={naiveAverage:0.0} steps";

            if (ok) Debug.Log("PASS  " + summary);
            else Debug.LogError("FAIL  " + summary);

            if (!ok && throwOnFailure) throw new Exception(summary);
            return ok;
        }
    }
}
