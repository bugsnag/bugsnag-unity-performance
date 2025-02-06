using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BugsnagUnityPerformance
{
    internal class FrameMetricsCollector : IPhasedStartup
    {
        private int TotalFrames = 0;
        private int SlowFrames = 0;
        private int FrozenFrames = 0;
        private const float FROZEN_FRAME_THRESHOLD = 0.7f; // 700 ms
        private const float DEFAULT_SLOW_FRAME_TOLERANCE = 0.05f;
        private PlayerLoopSystem _playerUpdateCallback;
        private bool _isEnabled = true;
        private bool _callbackActive = false;
        private List<float> _frozenFrameDurations = new List<float>();

        public FrameMetricsCollector()
        {
            BeginCollectingMetrics();
        }
        private void BeginCollectingMetrics()
        {
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
            TotalFrames++;
            float frameTime = Time.unscaledDeltaTime;
            if (frameTime >= FROZEN_FRAME_THRESHOLD)
            {
                _frozenFrameDurations.Add(frameTime);
                FrozenFrames++;
                return;
            }

            // this cannot be a cached value as target frame rate can change at any time
            var slowFrameThreshold = 1.0f / (Application.targetFrameRate > 0 ? Application.targetFrameRate : 60.0f);
            // apply tolerance to allow for some variance in frame time
            slowFrameThreshold *= 1.0f + DEFAULT_SLOW_FRAME_TOLERANCE;

            if (frameTime > slowFrameThreshold)
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
                FrozenFrameDurations = _frozenFrameDurations.ToList(),
                TotalFrames = TotalFrames,
                SlowFrames = SlowFrames,
                FrozenFrames = FrozenFrames
            };
        }

        public void Configure(PerformanceConfiguration config)
        {
            _isEnabled = config.AutoInstrumentRendering;
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
        public List<float> FrozenFrameDurations;
        public int TotalFrames;
        public int SlowFrames;
        public int FrozenFrames;
    }
}