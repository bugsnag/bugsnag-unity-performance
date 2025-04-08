using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class Tracer : IPhasedStartup
    {
        private PerformanceConfiguration _config;
        private FrameMetricsCollector _frameMetricsCollector;
        private SystemMetricsCollector _systemMetricsCollector;
        private List<Span> _finishedSpanQueue = new List<Span>();
        private List<WeakReference<Span>> _preStartSpans = new List<WeakReference<Span>>();
        private object _queueLock = new object();
        private object _prestartLock = new object();
        private WaitForSeconds _workerPollFrequency = new WaitForSeconds(1);
        private DateTimeOffset _lastBatchSendTime = DateTimeOffset.UtcNow;
        private Sampler _sampler;
        private Delivery _delivery;
        private bool _started;

        public Tracer(Sampler sampler, Delivery delivery, FrameMetricsCollector frameMetricsCollector, SystemMetricsCollector systemMetricsCollector)
        {
            _sampler = sampler;
            _delivery = delivery;
            _frameMetricsCollector = frameMetricsCollector;
            _systemMetricsCollector = systemMetricsCollector;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _config = config;
        }

        public void Start()
        {
            StartTracerWorker();
            _started = true;
            // Flush after setting _started so that no new spans are added to the prestart list during or after flushing
            FlushPreStartSpans();
        }

        private void StartTracerWorker()
        {
            try
            {
                MainThreadDispatchBehaviour.Enqueue(Worker());
            }
            catch
            {
                //Ignore this exception in unit tests, will not be an issue in a build
            }
        }

        private void FlushPreStartSpans()
        {
            foreach (var weakRef in _preStartSpans)
            {
                if (weakRef.TryGetTarget(out var span))
                {
                    RemoveDisabledMetricsFromPreStartSpan(span);
                    Sample(span);
                }
            }
        }

        private void RemoveDisabledMetricsFromPreStartSpan(Span span)
        {
            if (!_config.EnabledMetrics.Rendering)
            {
                if (span.IsFrozenFrameSpan)
                {
                    span.Discard();
                }
                else
                {
                    span.RemoveFrameRateMetrics();
                }
            }
            if (!_config.EnabledMetrics.CPU)
            {
                span.RemoveSystemCPUMetrics();
            }
            if (!_config.EnabledMetrics.Memory)
            {
                span.RemoveSystemMemoryMetrics();
            }
        }

        private IEnumerator Worker()
        {
            while (true)
            {
                if (BatchDue())
                {
                    DeliverBatch();
                }
                yield return _workerPollFrequency;
            }
        }

        public void OnSpanEnd(Span span)
        {
            ApplyFrameRateMetrics(span);
            // Delay the span end processing to allow for any additional metrics to be collected
            MainThreadDispatchBehaviour.Enqueue(DelayedOnSpanEnd(span));
        }

        private IEnumerator DelayedOnSpanEnd(Span span)
        {
            yield return new WaitForSeconds(2.0f);
            ApplySystemMetrics(span);
            if (!_started)
            {
                lock (_prestartLock)
                {
                    _preStartSpans.Add(new WeakReference<Span>(span));
                }
                yield break;
            }
            else
            {
                Sample(span);
            }
        }

        private void ApplyFrameRateMetrics(Span span)
        {
            _frameMetricsCollector.OnSpanEnd(span);
        }

        private void ApplySystemMetrics(Span span)
        {
            _systemMetricsCollector.OnSpanEnd(span);
        }

        public void RunOnEndCallbacks(Span span)
        {
            var callbacks = _config.GetOnSpanEndCallbacks();
            if (!span.WasDiscarded && callbacks != null && callbacks.Count > 0)
            {
                var startTime = DateTimeOffset.UtcNow;
                foreach (var callback in callbacks)
                {
                    try
                    {
                        if (!callback.Invoke(span))
                        {
                            span.Discard();
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        MainThreadDispatchBehaviour.LogWarning("Error running OnSpanEndCallback: " + e.Message);
                    }
                }
                var duration = DateTimeOffset.UtcNow - startTime;
                span.SetAttributeInternal("bugsnag.span.callbacks_duration", duration.Ticks * 100);
            }
            span.SetCallbackComplete();
        }

        private void Sample(Span span)
        {
            if (span.IsAppStartSpan && _config.AutoInstrumentAppStart == AutoInstrumentAppStartSetting.OFF)
            {
                return;
            }
            if (_sampler.Sampled(span))
            {
                RunOnEndCallbacks(span);
                if (!span.WasDiscarded)
                {
                    AddSpanToQueue(span);
                }
            }
        }

        private void AddSpanToQueue(Span span)
        {
            var deliverBatch = false;
            lock (_queueLock)
            {
                _finishedSpanQueue.Add(span);
                deliverBatch = BatchSizeLimitReached();
            }
            if (deliverBatch)
            {
                DeliverBatch();
            }
        }

        private void DeliverBatch()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                List<Span> batch = new List<Span>();
                foreach (var finishedSpan in _finishedSpanQueue)
                {
                    batch.Add(finishedSpan);
                }
                _finishedSpanQueue.Clear();
                if (batch.Count == 0)
                {
                    return;
                }
                _lastBatchSendTime = DateTimeOffset.UtcNow;
                _delivery.Deliver(batch);
            }
            else
            {
                new Thread(() =>
                {
                    List<Span> batch = new List<Span>();
                    lock (_queueLock)
                    {
                        batch.AddRange(_finishedSpanQueue);
                        _finishedSpanQueue.Clear();
                    }
                    if (batch.Count == 0)
                    {
                        return;
                    }
                    _lastBatchSendTime = DateTimeOffset.UtcNow;
                    _delivery.Deliver(batch);
                }).Start();
            }
        }

        private bool BatchSizeLimitReached()
        {
            return _finishedSpanQueue.Count >= _config.MaxBatchSize;
        }

        private bool BatchDue()
        {
            return (DateTimeOffset.UtcNow - _lastBatchSendTime).TotalSeconds > _config.MaxBatchAgeSeconds;
        }

    }
}


