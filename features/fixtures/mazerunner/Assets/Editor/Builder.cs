using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

public class Builder : MonoBehaviour
{

    static void BuildStandalone(string folder, BuildTarget target, bool dev)
    {
        BuildPlayerOptions opts = new BuildPlayerOptions();
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        opts.scenes = scenes;
        opts.locationPathName = folder;
        opts.target = target;
        opts.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(opts);
    }

    public static void MacOSRelease()
    {
        BuildMacOS(false);
    }

    public static void MacOSDev()
    {
        BuildMacOS(true);
    }
    static void BuildMacOS(bool dev)
    {
        BuildStandalone(dev ? "mazerunner_macos_dev" : "mazerunner_macos", BuildTarget.StandaloneOSX, dev);
    }

    public static void WindowsRelease()
    {
        BuildWindows(false);
    }

    public static void WindowsDev()
    {
        BuildWindows(false);
    }
    static void BuildWindows(bool dev)
    {
        BuildStandalone(dev ? "build/Windows/mazerunner_windows_dev.exe" : "build/Windows/mazerunner_windows.exe", BuildTarget.StandaloneWindows64, dev);
    }

    public static void WebGLRelease()
    {
        BuildWebGL(false);
    }

    public static void WebGLDev()
    {
        BuildWebGL(true);
    }

    static void BuildWebGL(bool dev)
    {
        BuildStandalone(dev ? "mazerunner_webgl_dev" : "mazerunner_webgl", BuildTarget.WebGL,dev);
    }

    // Generates the Mazerunner APK
    public static void AndroidRelease()
    {
        BuildAndroid(false);
    }

    public static void AndroidDev()
    {
        BuildAndroid(true);
    }
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



    // Generates the Mazerunner IPA
    
    public static void IosRelease(){
        BuildIos(false);
    }
    public static void IosDev(){
        BuildIos(true);
    }
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

    private static BuildPlayerOptions CommonMobileBuildOptions(string outputFile, bool dev)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = scenes;
        opts.locationPathName = Application.dataPath + "/../" + outputFile;
        opts.options = dev ? BuildOptions.Development : BuildOptions.None;

        return opts;
    }
}
#endif
