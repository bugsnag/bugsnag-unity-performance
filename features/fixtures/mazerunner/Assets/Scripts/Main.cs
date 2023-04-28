using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;

[Serializable]
public class Command
{
    public string action;
    public string scenarioName;
}

[Serializable]
public class FixtureConfig
{
    public string maze_address;
}

public class Main : MonoBehaviour
{

    private static Main _instance;

#if UNITY_IOS || UNITY_TVOS
    [DllImport("__Internal")]
    private static extern void ClearPersistentData();
#endif

    public TextMeshProUGUI DebugText;

    private const string API_KEY = "a35a2a72bd230ac0aa0f52715bbdc6aa";
    private string _fixtureConfigFileName = "/fixture_config.json";
    public static string MazeHost;

    public ScenarioRunner ScenarioRunner;

    private void Awake()
    {
        _instance = this;
    }

    public IEnumerator Start()
    {
        Log("Maze Runner app started");

        yield return GetFixtureConfig();

        InvokeRepeating("DoRunNextMazeCommand", 0, 1);
    }

    private IEnumerator GetFixtureConfig()
    {
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var numTries = 0;
            while (numTries < 5)
            {
                var configPath = Application.persistentDataPath + _fixtureConfigFileName;
                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    Debug.Log("Mazerunner got fixture config json: " + configJson);
                    var config = JsonUtility.FromJson<FixtureConfig>(configJson);
                    MazeHost = "http://" + config.maze_address;
                    break;
                }
                else
                {
                    Debug.Log("Mazerunner no fixture config found at path: " + configPath);
                    numTries++;
                    yield return new WaitForSeconds(1);
                }
            }
        }

        if (string.IsNullOrEmpty(MazeHost))
        {
            MazeHost = "http://localhost:9339";

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                MazeHost = "http://bs-local.com:9339";
            }
        }
        Debug.Log("Mazerunner host set to: " + MazeHost);
    }

    private void DoRunNextMazeCommand()
    {
        StartCoroutine(RunNextMazeCommand());
    }

    IEnumerator RunNextMazeCommand()
    {
        var url = MazeHost + "/command";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            var result = request != null && request.result == UnityWebRequest.Result.Success;
#else
            var result = request != null &&
                !request.isHttpError &&
                !request.isNetworkError;
#endif

            if (result)
            {
                var response = request.downloadHandler?.text;
                if (response == null || response == "null" || response == "No commands to provide" || response.Contains("noop"))
                {
                    
                }
                else
                {
                    var command = JsonUtility.FromJson<Command>(response);
                    if (command != null)
                    {
                        Log("Got Action: " + command.action + " and scenario: " + command.scenarioName);
                        if ("clear_cache".Equals(command.action))
                        {
                            ClearUnityCache();
                        }
                        else if ("run_scenario".Equals(command.action))
                        {
                            ScenarioRunner.RunScenario(command.scenarioName, API_KEY, MazeHost);
                        }
                        else if ("close_application".Equals(command.action))
                        {
                            CloseFixture();
                        }
                    }
                }
            }
        }
    }

    private void CloseFixture()
    {
        Application.Quit();
    }


    private void ClearUnityCache()
    {
        if (Directory.Exists(Application.persistentDataPath + "/Bugsnag"))
        {
            Directory.Delete(Application.persistentDataPath + "/Bugsnag", true);
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ClearIOSData();
        }
        if (Application.platform != RuntimePlatform.Android &&
            Application.platform != RuntimePlatform.IPhonePlayer)
        {
            Invoke("CloseFixture", 0.25f);
        }
    }

    public static void ClearIOSData()
    {
#if UNITY_IOS
        ClearPersistentData();
#endif
    }

    public static void Log(string msg)
    {
        _instance.DebugText.text += Environment.NewLine + msg;
        Console.WriteLine(msg);
        Debug.Log(msg);
    }

}



