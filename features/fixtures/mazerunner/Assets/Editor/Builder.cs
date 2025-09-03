using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

public class Builder : MonoBehaviour
{
    // ---------- Shared ----------
    static void BuildStandalone(string folder, BuildTarget target, bool dev)
    {
        BuildPlayerOptions opts = new BuildPlayerOptions();
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        opts.scenes = scenes;
        opts.locationPathName = folder;
        opts.target = target;
        opts.options = dev ? BuildOptions.Development : BuildOptions.None;
        BuildPipeline.BuildPlayer(opts);
    }

    // Attempts to switch Standalone (Windows) to IL2CPP if available, returning a restore action.
    // If IL2CPP is not available, returns null and leaves settings unchanged.
    static Action PrepareWindowsIl2CppIfAvailable()
    {
        var group = BuildTargetGroup.Standalone;

        // Keep original backend to restore later
        ScriptingImplementation originalBackend = PlayerSettings.GetScriptingBackend(group);

        try
        {
            // Unity 2020+ has this API; guard just in case.
#if UNITY_2019_3_OR_NEWER
            var available = PlayerSettings.GetAvailableScriptingImplementations(group);
            bool hasIl2Cpp = available != null && available.Contains(ScriptingImplementation.IL2CPP);
#else
            bool hasIl2Cpp = true; // very old fallback; we'll attempt and catch if it fails
#endif

            if (hasIl2Cpp)
            {
                if (originalBackend != ScriptingImplementation.IL2CPP)
                {
                    PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
                    Debug.Log("[Builder] Windows: Using IL2CPP scripting backend.");
                    return () =>
                    {
                        PlayerSettings.SetScriptingBackend(group, originalBackend);
                        Debug.Log($"[Builder] Restored Standalone scripting backend to {originalBackend}.");
                    };
                }
                else
                {
                    Debug.Log("[Builder] Windows: IL2CPP already selected.");
                    return null;
                }
            }
            else
            {
                Debug.Log("[Builder] IL2CPP not available for Standalone; keeping current backend: " + originalBackend);
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Builder] Could not switch to IL2CPP automatically (will keep current backend). " + e.Message);
            return null;
        }
    }

    // ---------- macOS ----------
    public static void MacOSRelease() => BuildMacOS(false);
    public static void MacOSDev() => BuildMacOS(true);

    static void BuildMacOS(bool dev)
    {
        BuildStandalone(dev ? "mazerunner_macos_dev" : "mazerunner_macos", BuildTarget.StandaloneOSX, dev);
    }

    // ---------- Windows ----------
    public static void WindowsRelease() => BuildWindows(false);
    public static void WindowsDev() => BuildWindows(true);

    static void BuildWindows(bool dev)
    {
        // Ensure IL2CPP if available, but always restore original backend afterwards.
        var restore = PrepareWindowsIl2CppIfAvailable();
        try
        {
            BuildStandalone(
                dev ? "build/Windows/mazerunner_windows_dev.exe" : "build/Windows/mazerunner_windows.exe",
                BuildTarget.StandaloneWindows64,
                dev
            );
        }
        finally
        {
            restore?.Invoke();
        }
    }

    // ---------- WebGL ----------
    public static void WebGLRelease() => BuildWebGL(false);
    public static void WebGLDev() => BuildWebGL(true);

    static void BuildWebGL(bool dev)
    {
        BuildStandalone(dev ? "mazerunner_webgl_dev" : "mazerunner_webgl", BuildTarget.WebGL, dev);
    }

    // ---------- Android ----------
    public static void AndroidRelease() => BuildAndroid(false);
    public static void AndroidDev() => BuildAndroid(true);

    static void BuildAndroid(bool dev)
    {
        Debug.Log("Building Android app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bugsnag.fixtures.unity.performance.android");
        var opts = CommonMobileBuildOptions(dev ? "mazerunner-dev.apk" : "mazerunner.apk", dev);
        opts.target = BuildTarget.Android;
#if UNITY_2022_1_OR_NEWER
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
#endif
        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    // ---------- iOS ----------
    public static void IosRelease() => BuildIos(false);
    public static void IosDev() => BuildIos(true);

    static void BuildIos(bool dev)
    {
        Debug.Log("Building iOS app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.bugsnag.fixtures.unity.performance.ios");
        PlayerSettings.iOS.appleDeveloperTeamID = "7W9PZ27Y5F";
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.allowHTTPDownload = true;

        var opts = CommonMobileBuildOptions(dev ? "mazerunner_dev_xcode" : "mazerunner_xcode", dev);
        opts.target = BuildTarget.iOS;

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    // ---------- Switch ----------
    public static void SwitchBuild()
    {
        Debug.Log("Building Switch app...");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Switch, "com.bugsnag.mazerunner");
        var opts = CommonMobileBuildOptions("mazerunner.nspd", false);
        opts.target = BuildTarget.Switch;
        opts.options = BuildOptions.Development;

        var result = BuildPipeline.BuildPlayer(opts);
        Debug.Log("Result: " + result);
    }

    // ---------- Mobile opts ----------
    private static BuildPlayerOptions CommonMobileBuildOptions(string outputFile, bool dev)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Application.dataPath + "/../" + outputFile,
            options = dev ? BuildOptions.Development : BuildOptions.None
        };

        return opts;
    }
}
#endif