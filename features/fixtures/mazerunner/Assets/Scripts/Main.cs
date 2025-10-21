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
    public string uuid;
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
    private string _commandUuidFileName = "/command_uuid.txt";
    private static string LastCommandUuid;
    public static string MazeHost;

    public ScenarioRunner ScenarioRunner;

    private void Awake()
    {
        _instance = this;
    }

    public IEnumerator Start()
    {
        Log("Maze Runner app started");
        GetLastCommandUuid();

        yield return GetFixtureConfig();

        InvokeRepeating("DoRunNextMazeCommand", 0, 1);
    }

    private void GetLastCommandUuid()
    {
        var uuidFilePath = Application.persistentDataPath + _commandUuidFileName;
        if (File.Exists(uuidFilePath))
        {
            LastCommandUuid = File.ReadAllText(uuidFilePath);
        }
        else
        {
            LastCommandUuid = "";
        }
        Log("Last command UUID is: " + LastCommandUuid);
    }

    private void SetLastCommandUuid(String uuid) 
    {
        Log("Setting last command UUID: " + uuid);
        var uuidFilePath = Application.persistentDataPath + _commandUuidFileName;
        File.WriteAllText(uuidFilePath, uuid);
        LastCommandUuid = uuid;
        Log("Command UUID is now: " + LastCommandUuid);
    }

    private IEnumerator GetFixtureConfig()
    {
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var numTries = 0;
            var timeOut = 15;
            while (numTries < timeOut)
            {
                var configPath = Application.persistentDataPath + _fixtureConfigFileName;
                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    Log("Mazerunner got fixture config json: " + configJson);
                    var config = JsonUtility.FromJson<FixtureConfig>(configJson);
                    MazeHost = "http://" + config.maze_address;
                    break;
                }
                else
                {
                    Log("Mazerunner no fixture config found at path: " + configPath);
                    numTries++;
                    if (numTries == timeOut)
                    {
                        Log("Timedout looking for config file!");
                    }
                    else
                    {
                        yield return new WaitForSeconds(1);
                    }
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
        Log("Mazerunner host set to: " + MazeHost);
    }

    private void DoRunNextMazeCommand()
    {
        StartCoroutine(RunNextMazeCommand());
    }

    IEnumerator RunNextMazeCommand()
    {
        var url = MazeHost + "/idem-command?after=" + LastCommandUuid;
        Log("Requesting Maze  Runner command from: " + url);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            var result = request != null && request.result == UnityWebRequest.Result.Success;

            if (result)
            {
                var response = request.downloadHandler?.text;
                if (response == null || response == "null")
                {
                    Log("No Maze Runner command to process at present");
                }
                else
                {
                    var command = JsonUtility.FromJson<Command>(response);
                    if (command != null)
                    {
                        Log("Received Maze Runner command:\n" + response);

                        switch(command.action)
                        {
                            case "noop":
                                break;
                            case "reset_uuid":
                                SetLastCommandUuid("");
                                break;
                            case "clear_cache":
                                ClearUnityCache();
                                SetLastCommandUuid(command.uuid);
                                break;
                            case "run_scenario":
                                SetLastCommandUuid(command.uuid);
                                ScenarioRunner.RunScenario(command.scenarioName, API_KEY, MazeHost);
                                break;
                        }
                    }
                }
            }
            else
            {
                Log("Getting next Maze Runner command failed: " + request.error);

            }
        }
    }

    private void ClearUnityCache()
    {
        if (Directory.Exists(Application.persistentDataPath + "/bugsnag-performance"))
        {
            Directory.Delete(Application.persistentDataPath + "/bugsnag-performance", true);
        }
        if (Directory.Exists(Application.persistentDataPath + "/Bugsnag"))
        {
            Directory.Delete(Application.persistentDataPath + "/Bugsnag", true);
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ClearIOSData();
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
        try
        {
            _instance.DebugText.text += Environment.NewLine + msg;
            Debug.Log(msg);
        }
        catch { }

    }
}
