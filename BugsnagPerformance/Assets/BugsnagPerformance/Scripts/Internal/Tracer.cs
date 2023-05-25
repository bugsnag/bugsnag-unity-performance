using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class Tracer: IPhasedStartup
    {
        private int _maxBatchSize = 100;

        private float _maxBatchAgeSeconds = 30f;

        private List<Span> _spanQueue = new List<Span>();

        private object _queueLock = new object();

        private WaitForSeconds _workerPollFrequency = new WaitForSeconds(1);

        private DateTimeOffset _lastBatchSendTime = DateTimeOffset.UtcNow;

        private Sampler _sampler;

        private Delivery _delivery;

        public Tracer(Sampler sampler, Delivery delivery)
        {
            _sampler = sampler;
            _delivery = delivery;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _maxBatchSize = config.MaxBatchSize;
            _maxBatchAgeSeconds = config.MaxBatchAgeSeconds;
        }

        public void Start()
        {
            StartTracerWorker();
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
            new Thread(()=>
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
                if (BugsnagPerformance.IsStarted)
                {
                    _lastBatchSendTime = DateTimeOffset.UtcNow;
                    _delivery.Deliver(batch);
                }
                else
                {
                    //TODO persist batch for later delivery
                }                
            }).Start();
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


