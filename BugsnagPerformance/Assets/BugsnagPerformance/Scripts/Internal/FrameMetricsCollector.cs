using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BugsnagUnityPerformance
{
    internal class FrameMetricsCollector
    {
        private int TotalFrames = 0;
        private int SlowFrames = 0;
        private int FrozenFrames = 0;
        private float SlowFrameThreshold = 1.0f / Application.targetFrameRate; // ~30 FPS
        private const float FROZEN_FRAME_THRESHOLD = 0.7f; // 700 ms

        public FrameMetricsCollector()
        {
            BeginCollectingMetrics();
        }
        private void BeginCollectingMetrics()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var newSystem = new PlayerLoopSystem
            {
                updateDelegate = OnUnityUpdate
            };
            var systems = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            systems.Insert(0, newSystem);
            playerLoop.subSystemList = systems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
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
            return new FrameMetricsSnapshot
            {
                TotalFrames = TotalFrames,
                SlowFrames = SlowFrames,
                FrozenFrames = FrozenFrames
            };
        }
    }

    internal class FrameMetricsSnapshot
    {
        public int TotalFrames { get; set; }
        public int SlowFrames { get; set; }
        public int FrozenFrames { get; set; }
    }
}