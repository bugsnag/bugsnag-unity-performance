using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BugsnagUnityPerformance
{
    public class BugsnagSceneManager
    {

        public static SceneEvent OnSeceneLoad = new SceneEvent();

        public static int sceneCount => SceneManager.sceneCount;

        public static int sceneCountInBuildSettings => SceneManager.sceneCountInBuildSettings;


        public static Scene CreateScene(string sceneName) => SceneManager.CreateScene(sceneName);

        public static Scene CreateScene(string sceneName, CreateSceneParameters parameters) => SceneManager.CreateScene(sceneName, parameters);

        public static Scene GetActiveScene() => SceneManager.GetActiveScene();

        public static Scene GetSceneAt(int index) => SceneManager.GetSceneAt(index);

        public static Scene GetSceneByBuildIndex(int buildIndex) => SceneManager.GetSceneByBuildIndex(buildIndex);

        public static Scene GetSceneByName(string name) => SceneManager.GetSceneByName(name);

        public static Scene GetSceneByPath(string scenePath) => SceneManager.GetSceneByPath(scenePath);

        public static void MergeScenes(Scene sourceScene, Scene destinationScene) => SceneManager.MergeScenes(sourceScene,destinationScene);

        public static void MoveGameObjectToScene(GameObject go, Scene scene) => SceneManager.MoveGameObjectToScene(go,scene);

        public static bool SetActiveScene(Scene scene) => SceneManager.SetActiveScene(scene);

        public static AsyncOperation UnloadSceneAsync(int sceneBuildIndex) => SceneManager.UnloadSceneAsync(sceneBuildIndex);

        public static AsyncOperation UnloadSceneAsync(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);

        public static AsyncOperation UnloadSceneAsync(Scene scene) => SceneManager.UnloadSceneAsync(scene);

        public static AsyncOperation UnloadSceneAsync(int sceneBuildIndex, UnloadSceneOptions options) => SceneManager.UnloadSceneAsync(sceneBuildIndex, options);

        public static AsyncOperation UnloadSceneAsync(string sceneName, UnloadSceneOptions options) => SceneManager.UnloadSceneAsync(sceneName, options);

        public static AsyncOperation UnloadSceneAsync(Scene scene, UnloadSceneOptions options) => SceneManager.UnloadSceneAsync(scene, options);


        public static void LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
        {
            OnSeceneLoad.Invoke(sceneBuildIndex);
            SceneManager.LoadScene(sceneBuildIndex, mode);
        }

        public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            OnSeceneLoad.Invoke(sceneName);
            SceneManager.LoadScene(sceneName, mode);
        }

        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            OnSeceneLoad.Invoke(sceneName);
            return SceneManager.LoadSceneAsync(sceneName, mode);
        }

        public static AsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
        {
            OnSeceneLoad.Invoke(sceneBuildIndex);
            return SceneManager.LoadSceneAsync(sceneBuildIndex, mode);
        }

        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneParameters parameters)
        {
            OnSeceneLoad.Invoke(sceneName);
            return SceneManager.LoadSceneAsync(sceneName, parameters);
        }

        public static AsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneParameters parameters)
        {
            OnSeceneLoad.Invoke(sceneBuildIndex);
            return SceneManager.LoadSceneAsync(sceneBuildIndex, parameters);
        }

    }

    [System.Serializable]
    public class SceneEvent : UnityEvent<object>
    {

    }
}