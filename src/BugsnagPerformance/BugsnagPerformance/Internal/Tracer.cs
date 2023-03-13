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
                //Not possible in unit tests
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
            lock (_queueLock)
            {
                _spanQueue.Add(span);
            }
            if (BatchSizeLimitReached())
            {
                DeliverBatch();
            }
        }   
       
        private void DeliverBatch()
        {
            new Thread(()=>
            {
                lock (_queueLock)
                {
                    var batch = _spanQueue;
                    if (BugsnagPerformance.IsStarted)
                    {
                        _lastBatchSendTime = DateTimeOffset.Now;
                        BugsnagPerformance.Delivery.Deliver(batch);
                    }
                    else
                    {
                        //TODO persist batch for later delivery
                    }
                    _spanQueue = new List<Span>();
                }
            }).Start();
        }

        private bool BatchSizeLimitReached()
        {
            lock (_queueLock)
            {
                return _spanQueue.Count >= PerformanceConfiguration.MaxBatchSize;
            }
        }

        private bool BatchDue()
        {
            return (DateTimeOffset.Now - _lastBatchSendTime).TotalSeconds > PerformanceConfiguration.MaxBatchAgeSeconds;
        }

    }
}


