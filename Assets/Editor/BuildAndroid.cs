using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MindTheCrack.EditorTools
{
    /// <summary>
    /// Phase 0 APK. The gate is "does it hold someone for five minutes on a
    /// phone", so the slice has to reach a phone before anything else.
    ///
    ///   Unity.exe -batchmode -quit -projectPath . \
    ///     -executeMethod MindTheCrack.EditorTools.BuildAndroid.Run
    ///
    /// The scene is empty on purpose: Bootstrap builds the whole slice at
    /// runtime, so there is nothing to author yet.
    /// </summary>
    public static class BuildAndroid
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string OutputDir = "Builds";
        const string ApkName = "mindthecrack-phase0.apk";

        [MenuItem("Mind the Crack/Build Android APK")]
        public static void Run()
        {
            EnsureScene();
            Configure();

            Directory.CreateDirectory(OutputDir);
            string output = Path.Combine(OutputDir, ApkName);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"BUILD OK  {output}  {summary.totalSize / (1024 * 1024)} MB  " +
                          $"{summary.totalTime.TotalSeconds:0} s");
            else
                Debug.LogError($"BUILD FAILED  {summary.result}  errors={summary.totalErrors}");
        }

        static void EnsureScene()
        {
            if (File.Exists(ScenePath)) return;

            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
        }

        static void Configure()
        {
            PlayerSettings.companyName = "Yilk Games";
            PlayerSettings.productName = "Mind the Crack";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android, "com.yilkgames.mindthecrack");

            // docs/market-research.md 10: IL2CPP + ARM64, stripping on.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);

            // Engine code stripping removes built-in resources nothing in a
            // scene references - the default GUI skin among them, which made
            // IMGUI throw before reaching our HUD. Phase 0 builds everything at
            // runtime, so there is nothing to hold those references.
            // Re-enable this once the HUD is real UI and the scene is authored;
            // shipping with stripping off costs build size.
            PlayerSettings.stripEngineCode = false;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

            // Rhythm game: never leave audio on the default buffer.
            var audio = AudioSettings.GetConfiguration();
            audio.dspBufferSize = 256;
            AudioSettings.Reset(audio);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
