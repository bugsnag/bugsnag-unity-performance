using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class Tracer
    {

        private List<Span> _spanQueue = new List<Span>();

        private object _queueLock = new object();

        private WaitForSeconds _workerPollFrequency = new WaitForSeconds(1);

        private DateTimeOffset _lastBatchSendTime = DateTimeOffset.Now;        


        public Tracer()
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
            //TODO check sampling logic to see if span should be ignored
            AddSpanToQueue(span);
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
                    _lastBatchSendTime = DateTimeOffset.Now;
                    BugsnagPerformance.Delivery.Deliver(batch);
                }
                else
                {
                    //TODO persist batch for later delivery
                }                
            }).Start();
        }

        private bool BatchSizeLimitReached()
        {
            return _spanQueue.Count >= PerformanceConfiguration.MaxBatchSize;
        }

        private bool BatchDue()
        {
            return (DateTimeOffset.Now - _lastBatchSendTime).TotalSeconds > PerformanceConfiguration.MaxBatchAgeSeconds;
        }

    }
}


