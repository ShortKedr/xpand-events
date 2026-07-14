using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class ValidationCommands {
    private const string ScenePath = "Assets/Validation.unity";

    public static void EnableDomainReload() {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        AssetDatabase.SaveAssets();
    }

    public static void DisableDomainReload() {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        AssetDatabase.SaveAssets();
    }

    public static void BuildMacMono() {
        BuildPlayer(BuildTarget.StandaloneOSX, NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x, "mac-mono/XpandEvents.app");
    }

    public static void BuildMacIl2Cpp() {
        BuildPlayer(BuildTarget.StandaloneOSX, NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP, "mac-il2cpp/XpandEvents.app");
    }

    public static void BuildAndroidIl2Cpp() {
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        BuildPlayer(BuildTarget.Android, NamedBuildTarget.Android, ScriptingImplementation.IL2CPP, "android-il2cpp/XpandEvents.apk");
    }

    public static void BuildWebGl() {
        BuildPlayer(BuildTarget.WebGL, NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP, "webgl/XpandEvents");
    }

    private static void BuildPlayer(BuildTarget target, NamedBuildTarget namedTarget, ScriptingImplementation backend, string relativeOutput) {
        EnsureScene();
        PlayerSettings.SetScriptingBackend(namedTarget, backend);
        PlayerSettings.SetApplicationIdentifier(namedTarget, "com.xpand.events.validation");

        string output = Path.Combine(GetOutputRoot(), relativeOutput);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var options = new BuildPlayerOptions {
            scenes = new[] { ScenePath },
            locationPathName = output,
            target = target,
            options = BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded) {
            throw new BuildFailedException($"{target}/{backend} failed with {report.summary.totalErrors} errors.");
        }

        Debug.Log($"XPAND_EVENTS_BUILD_PASSED target={target} backend={backend} bytes={report.summary.totalSize}");
    }

    private static void EnsureScene() {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static string GetOutputRoot() {
        string[] arguments = Environment.GetCommandLineArgs();
        for (int i = 0; i < arguments.Length - 1; i++) {
            if (arguments[i] == "-xpandOutput") return Path.GetFullPath(arguments[i + 1]);
        }

        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Builds"));
    }
}
