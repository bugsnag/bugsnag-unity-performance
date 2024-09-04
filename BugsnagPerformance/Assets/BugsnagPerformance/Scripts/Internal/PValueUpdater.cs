using System;
using System.Collections;
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
        public bool IsConfigured { get; private set; }

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
            IsConfigured = true;
        }

        public void Start()
        {
            MainThreadDispatchBehaviour.Instance().Enqueue(CheckPValue());
        }
        
        private IEnumerator CheckPValue()
        {
#if BUGSNAG_DEBUG
                Logger.I("Enqueue p value update request");
#endif
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
#if BUGSNAG_DEBUG
                Logger.I("OnPValueRequestCompleted Complete, new p value: " + newProbability);
#endif
                _sampler.Probability = newProbability;
                markPValueUpdated();
            }
        }
    }
}
