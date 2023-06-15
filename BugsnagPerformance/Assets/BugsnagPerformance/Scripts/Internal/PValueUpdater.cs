using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnityPerformance
{
    internal class PValueUpdater : IPhasedStartup
    {
        private Delivery _delivery;
        private Sampler _sampler;
        private DateTime _pValueTimeout;
        private float _pValueTimeoutSeconds;
        private float _pValueCheckIntervalSeconds;

        public PValueUpdater(Delivery delivery, Sampler sampler)
        {
            _delivery = delivery;
            _sampler = sampler;
            _pValueTimeout = DateTime.Now;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _pValueTimeoutSeconds = config.PValueTimeoutSeconds;
            _pValueCheckIntervalSeconds = config.PValueCheckIntervalSeconds;
        }

        public void Start()
        {
            MainThreadDispatchBehaviour.Instance().Enqueue(CheckPValue());
        }
        
        private IEnumerator CheckPValue()
        {
            while (true)
            {
                if (DateTime.Now.CompareTo(_pValueTimeout) >= 0)
                {
                    _delivery.DeliverPValueRequest(OnPValueRequestCompleted);
                }

                yield return new WaitForSeconds(_pValueCheckIntervalSeconds);
            }
        }

        private void markPValueUpdated()
        {
            _pValueTimeout = DateTime.Now.AddSeconds(_pValueTimeoutSeconds);
        }

        private void OnPValueRequestCompleted(TracePayload payload, UnityWebRequest req, double newProbability)
        {
            if (!Double.IsNaN(newProbability))
            {
                _sampler.Probability = newProbability;
                markPValueUpdated();
            }
        }
    }
}
