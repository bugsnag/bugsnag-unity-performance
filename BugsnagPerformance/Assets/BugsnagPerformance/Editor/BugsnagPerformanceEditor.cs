using System.Collections;
using System.Collections.Generic;
using System.IO;
using BugsnagUnityPerformance;
using UnityEditor;
using UnityEngine;

public class BugsnagPerformanceEditor : EditorWindow
{


    public Texture DarkIcon, LightIcon;

    private const string SETTINGS_PATH = "/Resources/Bugsnag/BugsnagPerformanceSettingsObject.asset";

    private const string NOTIFIER_SETTINGS_PATH = "/Resources/Bugsnag/BugsnagSettingsObject.asset";

    private void OnEnable()
    {
        titleContent.text = "BugSnag Performance";
    }

    private static string GetFullSettingsPath()
    {
        return Application.dataPath + SETTINGS_PATH;
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
        AssetDatabase.CreateAsset(asset, "Assets" + SETTINGS_PATH);
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

        if (NotifierConfigAvaliable())
        {
            EditorGUIUtility.labelWidth = 200;
            settings.UseNotifierSettings = EditorGUILayout.Toggle("Use BugSnag Error Monitoring SDK Settings", settings.UseNotifierSettings);
        }
     

        if (!NotifierConfigAvaliable() || !settings.UseNotifierSettings)
        {
            EditorGUIUtility.labelWidth = 70;
            EditorGUILayout.PropertyField(so.FindProperty("ApiKey"));

            EditorGUIUtility.labelWidth = 280;
            settings.StartAutomaticallyAtLaunch = EditorGUILayout.Toggle("Start Automatically (requires API key to be set)", settings.StartAutomaticallyAtLaunch);

            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.PropertyField(so.FindProperty("ReleaseStage"));
        }

        if (NotifierConfigAvaliable() && settings.UseNotifierSettings)
        {
            GUI.enabled = false;
            EditorGUILayout.LabelField("API Key: " + GetNotifierApiKey());
            EditorGUILayout.Toggle("Start Automatically", GetNotifierAutoStart());
            EditorGUILayout.LabelField("Release Stage: " + GetNotifierReleaseStage());
            GUI.enabled = true;
        }

        EditorGUIUtility.labelWidth = 200;
        EditorGUILayout.PropertyField(so.FindProperty("AutoInstrumentAppStart"));
        EditorGUILayout.PropertyField(so.FindProperty("Endpoint"));

        EditorGUI.indentLevel--;


        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);
    }


    private string GetNotifierReleaseStage()
    {
        var notifierSettings = GetNotifierSettingsObject();
        return notifierSettings.GetType().GetField("ReleaseStage").GetValue(notifierSettings).ToString();
    }

    private string GetNotifierApiKey()
    {
        var notifierSettings = GetNotifierSettingsObject();
        return notifierSettings.GetType().GetField("ApiKey").GetValue(notifierSettings).ToString();
    }

    private bool GetNotifierAutoStart()
    {
        var notifierSettings = GetNotifierSettingsObject();
        return (bool)notifierSettings.GetType().GetField("StartAutomaticallyAtLaunch").GetValue(notifierSettings);
    }

    private Object GetNotifierSettingsObject()
    {
        return Resources.Load("Bugsnag/BugsnagSettingsObject");
    }


    private bool NotifierConfigAvaliable()
    {
        return File.Exists(Application.dataPath + NOTIFIER_SETTINGS_PATH);
    }
}
