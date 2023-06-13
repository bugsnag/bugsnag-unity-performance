using System.Collections;
using System.Collections.Generic;
using System.IO;
using BugsnagUnity;
using BugsnagUnity.Editor;
using BugsnagUnityPerformance;
using UnityEditor;
using UnityEngine;

public class BugsnagPerformanceEditor : EditorWindow
{

    public Texture DarkIcon, LightIcon;

    private static string GetFullSettingsPath()
    {
        return Application.dataPath + "/Resources/Bugsnag/BugsnagPerformanceSettingsObject.asset";
    }

    [MenuItem("Window/Bugsnag/Performance Configuration", false, 0)]
    public static void ShowWindow()
    {
        CheckForSettingsCreation();
        GetWindow(typeof(BugsnagPerformanceEditor));
    }

    private static bool SettingsFileFound()
    {
        return File.Exists(GetFullSettingsPath());
    }

    private static void CheckForSettingsCreation()
    {
        if (!SettingsFileFound())
        {
            CreateNewSettingsFile();
        }
    }

    private BugsnagPerformanceSettingsObject GetSettingsObject()
    {
        return Resources.Load<BugsnagPerformanceSettingsObject>("Bugsnag/BugsnagPerformanceSettingsObject");
    }

    private static void CreateNewSettingsFile()
    {
        var resPath = Application.dataPath + "/Resources/Bugsnag";
        if (!Directory.Exists(resPath))
        {
            Directory.CreateDirectory(resPath);
        }
        var asset = CreateInstance<BugsnagPerformanceSettingsObject>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/Bugsnag/BugsnagPerformanceSettingsObject.asset");
        AssetDatabase.SaveAssets();
    }

    private void OnGUI()
    {
        DrawIcon();
        if (SettingsFileFound())
        {
            DrawSettingsEditorWindow();
        }
    }

    private void DrawIcon()
    {
        titleContent.image = EditorGUIUtility.isProSkin ? LightIcon : DarkIcon;
    }

    private void DrawSettingsEditorWindow()
    {

        var settings = GetSettingsObject();
        var so = new SerializedObject(settings);
        EditorGUI.indentLevel++;

        EditorGUIUtility.labelWidth = 70;
        EditorGUILayout.PropertyField(so.FindProperty("ApiKey"));

        EditorGUIUtility.labelWidth = 280;
        settings.StartAutomaticallyAtLaunch = EditorGUILayout.Toggle("Start Automatically (requires API key to be set)", settings.StartAutomaticallyAtLaunch);

        EditorGUIUtility.labelWidth = 200;
        EditorGUILayout.PropertyField(so.FindProperty("AutoInstrumentAppStart"));
        EditorGUILayout.PropertyField(so.FindProperty("ReleaseStage"));
        EditorGUILayout.PropertyField(so.FindProperty("SamplingProbability"));
        EditorGUILayout.PropertyField(so.FindProperty("Endpoint"));

        EditorGUI.indentLevel--;


        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);
    }
}
