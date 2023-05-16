using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class Tracer
    {

        private static List<Span> _spanQueue = new List<Span>();

        private static object _queueLock = new object();

        private static WaitForSeconds _workerPollFrequency = new WaitForSeconds(1);

        private static DateTimeOffset _lastBatchSendTime = DateTimeOffset.UtcNow;        


        public static void  StartTracerWorker()
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

        private static IEnumerator Worker()
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

        public static void OnSpanEnd(Span span)
        {
            //TODO check sampling logic to see if span should be ignored
            AddSpanToQueue(span);
        }

        private static void AddSpanToQueue(Span span)
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
       
        private static void DeliverBatch()
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
                    Delivery.Deliver(batch);
                }
                else
                {
                    //TODO persist batch for later delivery
                }                
            }).Start();
        }

        private static bool BatchSizeLimitReached()
        {
            return _spanQueue.Count >= PerformanceConfiguration.MaxBatchSize;
        }

        private static bool BatchDue()
        {
            return (DateTimeOffset.UtcNow - _lastBatchSendTime).TotalSeconds > PerformanceConfiguration.MaxBatchAgeSeconds;
        }

    }
}


