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
                ProcessQueue();
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
        }   
       
        private void ProcessQueue()
        {
            new Thread(()=>
            {
                if (ShouldCreateBatch())
                {
                    var batch = GetBatch();
                    if (BugsnagPerformance.IsStarted)
                    {
                        _lastBatchSendTime = DateTimeOffset.Now;
                        BugsnagPerformance.Delivery.Deliver(batch);
                    }
                    else
                    {
                        //TODO persist for later delivery
                    }
                }
            }).Start();
        }

        private bool ShouldCreateBatch()
        {
            if (_spanQueue.Count == 0)
            {
                return false;
            }
            var batchDue = (DateTimeOffset.Now - _lastBatchSendTime).TotalSeconds > PerformanceConfiguration.MaxBatchAgeSeconds;
            var queueFull = _spanQueue.Count >= PerformanceConfiguration.MaxBatchSize;
            return batchDue || queueFull;
        }

        private List<Span> GetBatch()
        {
            var batch = new List<Span>();
            foreach (var span in _spanQueue)
            {
                batch.Add(span);
            }
            foreach (var span in batch)
            {
                lock (_queueLock)
                {
                    _spanQueue.Remove(span);
                }
            }
            return batch;
        }

    }
}


