using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnityPerformance
{
    internal class PValueUpdater : IPhasedStartup
    {
        private PerformanceConfiguration _config;
        private Delivery _delivery;
        private Sampler _sampler;
        private DateTimeOffset _pValueTimeout;
        public bool IsConfigured { get; private set; }

        public PValueUpdater(Delivery delivery, Sampler sampler)
        {
            _delivery = delivery;
            _sampler = sampler;
            _pValueTimeout = DateTimeOffset.UtcNow;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _config = config;
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
                if (DateTimeOffset.UtcNow.CompareTo(_pValueTimeout) >= 0)
                {
                    _delivery.DeliverPValueRequest(OnPValueRequestCompleted);
                }

                yield return new WaitForSeconds(_config.PValueCheckIntervalSeconds);
            }
        }

        private void markPValueUpdated()
        {
            _pValueTimeout = DateTimeOffset.UtcNow.AddSeconds(_config.PValueTimeoutSeconds);
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
