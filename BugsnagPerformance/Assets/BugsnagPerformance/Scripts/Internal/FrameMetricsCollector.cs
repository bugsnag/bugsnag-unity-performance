using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BugsnagUnityPerformance
{
    public struct FrozenFrame
    {
        public DateTimeOffset StartTime;
        public DateTimeOffset EndTime;

        public FrozenFrame(DateTimeOffset start, DateTimeOffset end)
        {
            StartTime = start;
            EndTime = end;
        }
    }

    public class SpanRenderingMetrics
    {
        public int MinFrameRate = int.MaxValue;
        public int MaxFrameRate = int.MinValue;
        public FrameMetricsSnapshot BeginningMetrics;
        public FrameMetricsSnapshot EndingMetrics;

        public void UpdateFrameRate(int newFrameRate)
        {
            MinFrameRate = Mathf.Min(MinFrameRate, newFrameRate);
            MaxFrameRate = Mathf.Max(MaxFrameRate, newFrameRate);
        }
    }

    internal class FrozenFrameBuffer
    {
        private const int BUFFER_SIZE = 64;
        private FrozenFrame[] _frames;
        private int _index;
        public FrozenFrameBuffer Next { get; private set; }

        public FrozenFrameBuffer()
        {
            _frames = new FrozenFrame[BUFFER_SIZE];
            _index = 0;
        }

        public bool Add(FrozenFrame frame)
        {
            if (_index >= BUFFER_SIZE)
            {
                return false; // Buffer full
            }

            _frames[_index++] = frame;
            return true;
        }

        public FrozenFrameBuffer AppendNewBuffer()
        {
            Next = new FrozenFrameBuffer();
            return Next;
        }

        public IEnumerable<FrozenFrame> GetFrames()
        {
            for (int i = 0; i < _index; i++)
            {
                yield return _frames[i];
            }
        }

        public List<FrozenFrame> GetLastFrames(int amount)
        {
            var result = new List<FrozenFrame>(amount);
            var stack = new Stack<FrozenFrameBuffer>();

            var buffer = this;
            while (buffer != null)
            {
                stack.Push(buffer);
                buffer = buffer.Next;
            }

            while (stack.Count > 0 && result.Count < amount)
            {
                var currentBuffer = stack.Pop();

                for (int i = currentBuffer._index - 1; i >= 0 && result.Count < amount; i--)
                {
                    result.Add(currentBuffer._frames[i]);
                }
            }

            result.Reverse();
            return result;
        }
    }

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
        private FrozenFrameBuffer _frozenFrameBuffer = new FrozenFrameBuffer();
        private DateTimeOffset _lastFrameEndTime;
        private double _frameTimeSum = 0;
        private Dictionary<WeakReference<Span>, SpanRenderingMetrics> _instrumentedSpans = new Dictionary<WeakReference<Span>, SpanRenderingMetrics>();

        public FrameMetricsCollector()
        {
            BeginCollectingMetrics();
        }


        public void Configure(PerformanceConfiguration config)
        {
            _isEnabled = config.EnabledMetrics.Rendering;
            if (!_isEnabled)
            {
                RemoveUpdateCallback();
            }
        }

        public void OnSpanStart(Span span)
        {
            if (!_isEnabled)
            {
                return;
            }

            var spanRef = new WeakReference<Span>(span);
            var metrics = new SpanRenderingMetrics
            {
                BeginningMetrics = TakeSnapshot()
            };
            _instrumentedSpans[spanRef] = metrics;
        }

        public void OnSpanEnd(Span span)
        {
            SpanRenderingMetrics spanMetrics = GetSpanMetrics(span);
            if (spanMetrics == null)
            {
                return;
            }
            spanMetrics.EndingMetrics = TakeSnapshot();
            span.CalculateFrameRateMetrics(spanMetrics);
        }

        private SpanRenderingMetrics GetSpanMetrics(Span span)
        {
            SpanRenderingMetrics spanMetrics = null;
            List<WeakReference<Span>> toRemove = new List<WeakReference<Span>>();
            foreach (var pair in _instrumentedSpans)
            {
                //if a span is no longer alive, remove it from the list
                if (!pair.Key.TryGetTarget(out var spanRef))
                {
                    toRemove.Add(pair.Key);
                }
                else if (spanRef == span)
                {
                    spanMetrics = pair.Value;
                    toRemove.Add(pair.Key);
                    break;
                }
            }
            foreach (var key in toRemove)
            {
                _instrumentedSpans.Remove(key);
            }
            return spanMetrics;
        }

        private void BeginCollectingMetrics()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            _playerUpdateCallback = new PlayerLoopSystem
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
            var now = DateTimeOffset.UtcNow;

            if (_lastFrameEndTime == default)
            {
                // First frame, just record the time and return
                _lastFrameEndTime = now;
                return;
            }
            TotalFrames++;
            float frameTime = (float)(now - _lastFrameEndTime).TotalSeconds;
            if (frameTime < 0)
            {
                // Time went backwards, ignore this frame
                _lastFrameEndTime = now;
                return;
            }
            _frameTimeSum += frameTime;

            DetectSlowOrFrozenFrames(frameTime, now);
            UpdateInstrumentedSpans(frameTime);

            _lastFrameEndTime = now;
        }

        private void DetectSlowOrFrozenFrames(float frameTime, DateTimeOffset now)
        {
            if (frameTime >= FROZEN_FRAME_THRESHOLD)
            {
                var frozenFrame = new FrozenFrame(_lastFrameEndTime, now);
                if (!_frozenFrameBuffer.Add(frozenFrame))
                {
                    var newBuffer = _frozenFrameBuffer.AppendNewBuffer();
                    newBuffer.Add(frozenFrame);
                    _frozenFrameBuffer = newBuffer;
                }
                FrozenFrames++;
            }
            else
            {
                float slowFrameThreshold = 1.0f / (Application.targetFrameRate > 0
                    ? Application.targetFrameRate
                    : 60.0f);
                slowFrameThreshold *= 1.0f + DEFAULT_SLOW_FRAME_TOLERANCE;

                if (frameTime > slowFrameThreshold)
                {
                    SlowFrames++;
                }
            }
        }

        private void UpdateInstrumentedSpans(float frameTime)
        {
            // If frameTime is zero or infinity, we cannot calculate a frame rate
            // This is common on WebGL
            if (frameTime <= 0f || float.IsInfinity(frameTime))
            {
                return;
            }
            int frameRate = (int)(1.0f / frameTime);
            foreach (var pair in _instrumentedSpans)
            {
                pair.Value.UpdateFrameRate(frameRate);
            }
        }

        private FrameMetricsSnapshot TakeSnapshot()
        {
            if (!_isEnabled || !_callbackActive)
            {
                return null;
            }

            return new FrameMetricsSnapshot
            {
                FrozenFrameBuffer = _frozenFrameBuffer,
                TotalFrames = TotalFrames,
                SlowFrames = SlowFrames,
                FrozenFrames = FrozenFrames,
                FrameTimeSum = _frameTimeSum,
                TargetFrameRate = Application.targetFrameRate
            };
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
            // Do nothing
        }
    }

    public class FrameMetricsSnapshot
    {
        internal FrozenFrameBuffer FrozenFrameBuffer { get; set; }
        public int TotalFrames { get; set; }
        public int SlowFrames { get; set; }
        public int FrozenFrames { get; set; }
        public double FrameTimeSum;
        public float TargetFrameRate;

    }
}