using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class Tracer : IPhasedStartup
    {
        private int _maxBatchSize = 100;

        private float _maxBatchAgeSeconds = 30f;

        private List<WeakReference<Span>> _spanQueue = new List<WeakReference<Span>>();

        private List<WeakReference<Span>> _preStartSpans = new List<WeakReference<Span>>();

        private object _queueLock = new object();

        private object _prestartLock = new object();

        private WaitForSeconds _workerPollFrequency = new WaitForSeconds(1);

        private DateTimeOffset _lastBatchSendTime = DateTimeOffset.UtcNow;

        private Sampler _sampler;

        private Delivery _delivery;

        private bool _started;

        private static AutoInstrumentAppStartSetting _appStartSetting;

        private List<Func<Span, bool>> _onSpanEndCallbacks;


        public Tracer(Sampler sampler, Delivery delivery)
        {
            _sampler = sampler;
            _delivery = delivery;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _onSpanEndCallbacks = config.GetOnSpanEndCallbacks();
            _maxBatchSize = config.MaxBatchSize;
            _maxBatchAgeSeconds = config.MaxBatchAgeSeconds;
            _appStartSetting = config.AutoInstrumentAppStart;
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
                MainThreadDispatchBehaviour.Instance().Enqueue(Worker());
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
                    if (span.IsAppStartSpan && _appStartSetting == AutoInstrumentAppStartSetting.OFF)
                    {
                        continue;
                    }
                    Sample(span);
                }
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
            Debug.Log("OnSpanEnd: " + span.Name);
            var weakSpan = new WeakReference<Span>(span);

            if (!_started)
            {
                lock (_prestartLock)
                {
                    _preStartSpans.Add(weakSpan);
                }
                return;
            }
            else
            {
                Sample(span);
            }
        }

        public void RunOnEndCallbacks(Span span)
        {
            if (!span.WasDiscarded && _onSpanEndCallbacks != null && _onSpanEndCallbacks.Count > 0)
            {
                var startTime = DateTimeOffset.UtcNow;
                foreach (var callback in _onSpanEndCallbacks)
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
                        Debug.LogError("Error running OnSpanEndCallback: " + e.Message);
                    }
                }
                var duration = DateTimeOffset.UtcNow - startTime;
                span.SetAttributeInternal("bugsnag.span.callbacks_duration", duration.Ticks * 100);
            }
            span.SetCallbackComplete();
        }

        private void Sample(Span span)
        {
            Debug.Log("Sampling span: " + span.Name);
            if (_sampler.Sampled(span))
            {
                Debug.Log("Running OnEndCallbacks");

                RunOnEndCallbacks(span);
                                Debug.Log("OnEndCallbacks complete");

                if (!span.WasDiscarded)
                {
                                    Debug.Log("Adding span to queue");

                    AddSpanToQueue(span);
                }
            }
        }

        private void AddSpanToQueue(Span span)
        {
            var deliverBatch = false;
            lock (_queueLock)
            {
                _spanQueue.Add(new WeakReference<Span>(span));
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
                foreach (var weakRef in _spanQueue)
                {
                    if (weakRef.TryGetTarget(out var span))
                    {
                        batch.Add(span);
                    }
                }
                _spanQueue.Clear();


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
                        foreach (var weakRef in _spanQueue)
                        {
                            if (weakRef.TryGetTarget(out var span))
                            {
                                batch.Add(span);
                            }
                        }
                        _spanQueue.Clear();
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
            return _spanQueue.Count >= _maxBatchSize;
        }

        private bool BatchDue()
        {
            return (DateTimeOffset.UtcNow - _lastBatchSendTime).TotalSeconds > _maxBatchAgeSeconds;
        }

    }
}


