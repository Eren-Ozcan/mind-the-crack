using MindTheCrack.Gameplay;
using MindTheCrack.Rhythm;
using UnityEngine;

namespace MindTheCrack.DebugTools
{
    /// <summary>
    /// Phase 0 instrumentation. The numbers that matter while tuning feel are
    /// the input deviation in milliseconds and the multiplier - everything
    /// else is noise at this stage.
    /// </summary>
    public sealed class DebugHud : MonoBehaviour
    {
        public RunController Run;
        public Conductor Conductor;
        public StepInputReader Input;

        /// <summary>How long the press flash stays up, seconds.</summary>
        const double FlashDuration = 0.12;

        GUIStyle _big, _small, _result;
        Texture2D _flash;

        void OnGUI()
        {
            _big ??= new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold };
            _small ??= new GUIStyle(GUI.skin.label) { fontSize = 22 };
            _result ??= new GUIStyle(GUI.skin.label) { fontSize = 46, fontStyle = FontStyle.Bold };

            DrawPressFlash();

            float pad = 18f;
            GUILayout.BeginArea(new Rect(pad, pad, Screen.width - pad * 2, Screen.height - pad * 2));

            GUILayout.Label($"{Run.Distance:0.0} m", _big);
            GUILayout.Label($"skor {Run.Score}   x{Run.Multiplier:0.0}   seri {Run.PerfectStreak}", _small);

            double ms = Run.LastDeviation * 1000.0;
            string dev = Run.LastInputIgnored
                ? $"{(ms < 0 ? "COK ERKEN" : "COK GEC")} ({ms:+0;-0} ms)"
                : $"sapma {ms:+0;-0} ms";
            GUILayout.Label($"{dev}   adim {Run.LastKind}   bpm {Conductor.Bpm:0}", _small);
            GUILayout.Label($"offset {Conductor.CalibrationOffset * 1000.0:0} ms", _small);

            GUILayout.FlexibleSpace();

            var (text, colour) = Run.LastResult switch
            {
                StepResult.Perfect => ("PERFECT", new Color(0.3f, 1f, 0.4f)),
                StepResult.Good => ("iyi", new Color(0.85f, 0.85f, 0.85f)),
                StepResult.Stumble => ("MISS", new Color(1f, 0.55f, 0.2f)),
                _ => ("OLDUN", new Color(1f, 0.3f, 0.3f)),
            };
            var prev = GUI.color;
            GUI.color = colour;
            GUILayout.Label(text, _result);
            GUI.color = prev;

            GUILayout.Label("dokunma = 2 blok   tap = 3 blok   yukari swipe = 5 blok", _small);

            GUILayout.EndArea();

            // Out of the way in the corner: in the middle of the screen it ate
            // taps meant for the beat, which reads as a dead spot.
            if (GUI.Button(new Rect(Screen.width - 150f - pad, pad, 150f, 44f), "Yeniden"))
                Run.StartRun(Random.Range(1, 999999));
        }

        /// <summary>
        /// Border flash the moment a press is read, anywhere on screen. It says
        /// "the tap arrived" before the beat has decided what to do with it, so
        /// bad timing stops reading as a broken touch area.
        /// </summary>
        void DrawPressFlash()
        {
            if (Input == null || !Conductor.Running) return;

            double age = Conductor.SongTime - Input.LastPressSongTime;
            if (age < 0 || age > FlashDuration) return;

            _flash ??= Texture2D.whiteTexture;

            float alpha = (float)(1.0 - age / FlashDuration) * 0.5f;
            var prev = GUI.color;
            GUI.color = new Color(0.6f, 0.85f, 1f, alpha);

            const float w = 14f;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, w), _flash);
            GUI.DrawTexture(new Rect(0, Screen.height - w, Screen.width, w), _flash);
            GUI.DrawTexture(new Rect(0, 0, w, Screen.height), _flash);
            GUI.DrawTexture(new Rect(Screen.width - w, 0, w, Screen.height), _flash);

            GUI.color = prev;
        }
    }
}
