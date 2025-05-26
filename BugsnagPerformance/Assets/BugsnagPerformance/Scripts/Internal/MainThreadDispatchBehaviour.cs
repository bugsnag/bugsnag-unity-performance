using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BugsnagUnityPerformance
{
    public class BugsnagPerformanceCoroutineRunner : MonoBehaviour
    {
        private static BugsnagPerformanceCoroutineRunner _instance;

        public static BugsnagPerformanceCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var runnerObject = new GameObject("BugsnagPerformanceCoroutineRunner");
                    _instance = runnerObject.AddComponent<BugsnagPerformanceCoroutineRunner>();
                    DontDestroyOnLoad(runnerObject);
                }
                return _instance;
            }
        }
    }
    public class MainThreadDispatchBehaviour
    {

        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var newSystem = new PlayerLoopSystem
            {
                updateDelegate = OnUpdate
            };

            var systems = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            systems.Insert(0, newSystem);
            playerLoop.subSystemList = systems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void OnUpdate()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public static void Enqueue(IEnumerator action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() =>
                {
                    BugsnagPerformanceCoroutineRunner.Instance.StartCoroutine(action);
                });
            }
        }

        public static void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        public static void LogWarning(string msg)
        {
            Enqueue(() =>
            {
                Debug.LogWarning(msg);
            });
        }
    }
}