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

        private List<Span> _spanQueue = new List<Span>();

        private List<Span> _preStartSpans = new List<Span>();

        private object _queueLock = new object();

        private object _prestartLock = new object();

        private WaitForSeconds _workerPollFrequency = new WaitForSeconds(1);

        private DateTimeOffset _lastBatchSendTime = DateTimeOffset.UtcNow;

        private Sampler _sampler;

        private Delivery _delivery;

        private bool _started;

        private static AutoInstrumentAppStartSetting _appStartSetting;



        public Tracer(Sampler sampler, Delivery delivery)
        {
            _sampler = sampler;
            _delivery = delivery;
        }

        public void Configure(PerformanceConfiguration config)
        {
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
            foreach (var span in _preStartSpans)
            {
                if (span.IsAppStartSpan && _appStartSetting == AutoInstrumentAppStartSetting.OFF)
                {
                    continue;
                }
                Sample(span);
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
            if (!_started)
            {
                lock (_prestartLock)
                {
                    _preStartSpans.Add(span);
                }
                return;
            }
            Sample(span);
        }

        private void Sample(Span span)
        {
            if (_sampler.Sampled(span))
            {
                AddSpanToQueue(span);
            }
        }

        private void AddSpanToQueue(Span span)
        {
            var deliverBatch = false;
            lock (_queueLock)
            {
                _spanQueue.Add(span);
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
                List<Span> batch = null;
                if (_spanQueue.Count == 0)
                {
                    return;
                }
                batch = _spanQueue.Where(span => !span.WasAborted).ToList();
                _spanQueue = new List<Span>();
                _lastBatchSendTime = DateTimeOffset.UtcNow;
                _delivery.Deliver(batch);
            }
            else
            {
                new Thread(() =>
                {
                    List<Span> batch = null;
                    lock (_queueLock)
                    {
                        if (_spanQueue.Count == 0)
                        {
                            return;
                        }
                        batch = _spanQueue;
                        _spanQueue = new List<Span>();
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


