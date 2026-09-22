using MindTheCrack.Config;
using MindTheCrack.DebugTools;
using MindTheCrack.Gameplay;
using MindTheCrack.Rhythm;
using UnityEngine;

namespace MindTheCrack
{
    /// <summary>
    /// Builds the whole vertical slice from code on load, so Phase 0 needs no
    /// authored scene, no prefabs and no assets. Whatever scene is open, press
    /// play and the slice runs; the Android build works the same way.
    ///
    /// This is scaffolding, not architecture. Once Phase 1 brings real art and
    /// several grounds, the scene gets authored properly and this goes away.
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            // The per-step telemetry line is the point; the stack trace under
            // each one is four more lines saying the same thing and pushed the
            // useful log off the device's ring buffer.
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            var root = new GameObject("MindTheCrack");
            Object.DontDestroyOnLoad(root);

            var config = StepConfig.CreateDefault();

            // --- audio -------------------------------------------------------
            var audioGo = new GameObject("Music");
            audioGo.transform.SetParent(root.transform, false);
            var source = audioGo.AddComponent<AudioSource>();
            source.clip = ClickTrack.Create(config.StartBpm);
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            var conductor = audioGo.AddComponent<Conductor>();
            conductor.CommitDelay = config.LateWindow;
            conductor.LoadCalibration();

            // --- world -------------------------------------------------------
            var player = Greybox.Cube("Player", root.transform,
                Greybox.NewMaterial(new Color(0.25f, 0.35f, 0.75f)));
            player.transform.localScale = new Vector3(0.36f, 0.75f, 0.36f);

            var view = root.AddComponent<SidewalkView>();

            var markers = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var m = Greybox.Quad($"preview_{i}", root.transform,
                    Greybox.NewMaterial(Color.white));
                m.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                m.transform.localScale = new Vector3(0.55f, 0.28f, 1f);
                markers[i] = m.transform;
            }

            // --- systems -----------------------------------------------------
            var input = root.AddComponent<StepInputReader>();
            input.Conductor = conductor;
            input.MaxAge = config.EarlyWindow + config.LateWindow;
            var run = root.AddComponent<RunController>();
            run.Conductor = conductor;
            run.Input = input;
            run.Config = config;
            run.View = view;
            run.Player = player.transform;
            run.Music = source;
            run.PreviewMarkers = markers;
            run.DeathEnabled = false;   // Phase 0: stumble instead of dying.

            var hud = root.AddComponent<DebugHud>();
            hud.Run = run;
            hud.Conductor = conductor;
            hud.Input = input;

            // --- camera and light --------------------------------------------
            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(root.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.backgroundColor = new Color(0.12f, 0.13f, 0.16f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 46f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraFollow>().Target = player.transform;

            var lightGo = new GameObject("Sun");
            lightGo.transform.SetParent(root.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

            run.StartRun(Random.Range(1, 999999));
            conductor.Begin(source, config.StartBpm);
        }

    }

    /// <summary>Tilted chase camera - both the character and the stones ahead
    /// of it have to stay readable (docs/game-design.md 11).</summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        // Pulled back and tipped down a little: the extra metres of pavement
        // ahead are what the player is deciding with.
        public Vector3 Offset = new(0f, 4.4f, -5.4f);
        public float Pitch = 28f;
        public float Smooth = 12f;

        void LateUpdate()
        {
            if (Target == null) return;
            var want = new Vector3(Offset.x, Offset.y, Target.position.z + Offset.z);
            transform.position = Vector3.Lerp(transform.position, want, Time.deltaTime * Smooth);
            transform.rotation = Quaternion.Euler(Pitch, 0f, 0f);
        }
    }
}
