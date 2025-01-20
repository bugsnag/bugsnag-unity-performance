using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BugsnagUnityPerformance
{
    internal class FrameMetricsCollector : IPhasedStartup
    {
        private int TotalFrames = 0;
        private int SlowFrames = 0;
        private int FrozenFrames = 0;
        private float SlowFrameThreshold = 1.0f / (Application.targetFrameRate > 0 ? Application.targetFrameRate : 60.0f);
        private const float FROZEN_FRAME_THRESHOLD = 0.7f; // 700 ms
        private PlayerLoopSystem _playerUpdateCallback;
        private bool _isEnabled = true;
        private bool _callbackActive = false;

        public FrameMetricsCollector()
        {
            BeginCollectingMetrics();
        }
        private void BeginCollectingMetrics()
        {
            Debug.Log(Application.targetFrameRate);
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            _playerUpdateCallback= new PlayerLoopSystem
            {
                updateDelegate = OnUnityUpdate
            };
            var systems = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            systems.Insert(0, _playerUpdateCallback);
            playerLoop.subSystemList = systems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
            _callbackActive = true;
        }

        private void OnUnityUpdate()
        {
            float frameTime = Time.unscaledDeltaTime;
            TotalFrames++;

            if (frameTime >= FROZEN_FRAME_THRESHOLD)
            {
                FrozenFrames++;
            }
            else if (frameTime > SlowFrameThreshold)
            {
                SlowFrames++;
            }
        }

        public FrameMetricsSnapshot TakeSnapshot()
        {
            if(!_isEnabled || !_callbackActive)
            {
                return null;
            }
            return new FrameMetricsSnapshot
            {
                TotalFrames = TotalFrames,
                SlowFrames = SlowFrames,
                FrozenFrames = FrozenFrames
            };
        }

        public void Configure(PerformanceConfiguration config)
        {
            _isEnabled = config.EnabledMetrics.Rendering;
            if(!_isEnabled)
            {
                RemoveUpdateCallback();
            }
        }

        private void RemoveUpdateCallback()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var systems = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            systems.Remove(_playerUpdateCallback);
            playerLoop.subSystemList = systems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
            _callbackActive = false;
        }

        public void Start()
        {
            //do nothing
        }
    }

    public class FrameMetricsSnapshot
    {
        public int TotalFrames { get; set; }
        public int SlowFrames { get; set; }
        public int FrozenFrames { get; set; }
    }
}