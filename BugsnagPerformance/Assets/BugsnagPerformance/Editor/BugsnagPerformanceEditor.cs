using System.IO;
using BugsnagUnityPerformance;
using UnityEditor;
using UnityEngine;

public class BugsnagPerformanceEditor : EditorWindow
{


    public Texture DarkIcon, LightIcon;

    private void OnEnable()
    {
        titleContent.text = "BugSnag Performance";
    }

    private void Update()
    {
        CheckForSettingsCreation();
    }

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

        if (NotifierConfigAvaliable())
        {
            EditorGUIUtility.labelWidth = 280;
            settings.UseNotifierSettings = EditorGUILayout.Toggle("Use BugSnag Error Monitoring SDK Settings", settings.UseNotifierSettings);
        }
     

        if (!NotifierConfigAvaliable() || !settings.UseNotifierSettings)
        {
            DrawStandaloneSettings(so,settings);
        }

        if (NotifierConfigAvaliable() && settings.UseNotifierSettings)
        {
            DrawNotifierSettings();
        }

        EditorGUIUtility.labelWidth = 200;
        EditorGUILayout.PropertyField(so.FindProperty("AutoInstrumentAppStart"));
        EditorGUILayout.PropertyField(so.FindProperty("Endpoint"));

        EditorGUI.indentLevel--;


        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);
    }

    private void DrawStandaloneSettings(SerializedObject so, BugsnagPerformanceSettingsObject settings)
    {
        EditorGUIUtility.labelWidth = 70;
        EditorGUILayout.PropertyField(so.FindProperty("ApiKey"));

        EditorGUIUtility.labelWidth = 280;
        settings.StartAutomaticallyAtLaunch = EditorGUILayout.Toggle("Start Automatically (requires API key to be set)", settings.StartAutomaticallyAtLaunch);

        EditorGUIUtility.labelWidth = 200;
        EditorGUILayout.PropertyField(so.FindProperty("ReleaseStage"));

        EditorGUILayout.PropertyField(so.FindProperty("EnabledReleaseStages"));

        EditorGUILayout.PropertyField(so.FindProperty("AppVersion"));
        settings.VersionCode = EditorGUILayout.IntField(new GUIContent("Version Code ⓘ", "Android devices only"), settings.VersionCode);
        settings.BundleVersion = EditorGUILayout.TextField(new GUIContent("Bundle Version ⓘ", "Apple devices only"), settings.BundleVersion);
    }

    private void DrawNotifierSettings()
    {
        GUI.enabled = false;

        EditorGUILayout.LabelField("API Key: " + (string)GetValueFromNotifer("ApiKey"));
        EditorGUILayout.Toggle("Start Automatically: ", (bool)GetValueFromNotifer("StartAutomaticallyAtLaunch"));
        EditorGUILayout.LabelField("Release Stage: " + (string)GetValueFromNotifer("ReleaseStage"));

        var notifierReleaseStages = (string[])GetValueFromNotifer("EnabledReleaseStages");
        var stagesString = string.Empty;
        if (notifierReleaseStages != null)
        {
            foreach (var stage in notifierReleaseStages)
            {
                stagesString += " " + stage + ",";
            }
        }
        stagesString = stagesString.TrimEnd(',');

        EditorGUILayout.LabelField("Enabled Release Stages:" + stagesString);
        EditorGUILayout.LabelField("App Version: " + (string)GetValueFromNotifer("AppVersion"));
        EditorGUILayout.LabelField("Version Code: " + (int)GetValueFromNotifer("VersionCode"));
        EditorGUILayout.LabelField("Bundle Version: " + (string)GetValueFromNotifer("BundleVersion"));

        GUI.enabled = true;
    }

    private object GetValueFromNotifer(string key)
    {
        var notifierSettings = GetNotifierSettingsObject();
        if (notifierSettings != null)
        {
            var field = notifierSettings.GetType().GetField(key);
            if (field != null)
            {
                return field.GetValue(notifierSettings);
            }
        }
        return null;
    }

    private Object GetNotifierSettingsObject()
    {
        return Resources.Load("Bugsnag/BugsnagSettingsObject");
    }

    private bool NotifierConfigAvaliable()
    {
        return File.Exists(Application.dataPath + "/Resources/Bugsnag/BugsnagSettingsObject.asset");
    }
}
