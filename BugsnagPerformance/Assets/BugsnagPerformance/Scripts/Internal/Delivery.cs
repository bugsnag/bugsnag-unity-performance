using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnityPerformance
{
    public delegate void OnProbabilityChanged(double newProbability);

    internal class Delivery : IPhasedStartup
    {
        private OnProbabilityChanged _onProbabilityChanged;
        private bool _flushingCache;
        private ResourceModel _resourceModel;
        private CacheManager _cacheManager;
        private PerformanceConfiguration _config;

        private enum RequestResult
        {
            Success,
            RetriableFailure,
            PermanentFailure
        };

        internal delegate void OnServerResponse(TracePayload payload, UnityWebRequest req, double newProbability);

        private const int MAX_PAYLOAD_BYTES = 1000000;

        private static RequestResult GetRequestResult(UnityWebRequest req)
        {
            switch (req.responseCode)
            {
                case 200:
                case 202:
                    return RequestResult.Success;
                case 0:
                case 408:
                case 429:
                    return RequestResult.RetriableFailure;
                default:
                    return req.responseCode >= 500 ? RequestResult.RetriableFailure : RequestResult.PermanentFailure;
            }
        }

        public Delivery(ResourceModel resourceModel, CacheManager cacheManager, OnProbabilityChanged onProbabilityChanged)
        {
            _resourceModel = resourceModel;
            _cacheManager = cacheManager;
            _onProbabilityChanged = onProbabilityChanged;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _config = config;
        }

        public void Start()
        {
            FlushCache();
        }

        public void Deliver(List<Span> batch)
        {
            var payload = new TracePayload(_resourceModel, batch, _config.IsFixedSamplingProbability, _config.AttributeArrayLengthLimit, _config.AttributeStringValueLimit);
            MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload, OnTraceDeliveryCompleted));
        }

        private void OnTraceDeliveryCompleted(TracePayload payload, UnityWebRequest req, double newProbability)
        {
            switch (GetRequestResult(req))
            {
                case RequestResult.Success:
                    CheckForProbabilityUpdate(newProbability);
                    PayloadSendSuccess(payload.PayloadId);
                    FlushCache();
                    return;
                case RequestResult.RetriableFailure:
                    if (req.uploadedBytes <= MAX_PAYLOAD_BYTES)
                    {
                        _cacheManager.CacheBatch(payload);
                    }
                    return;
                case RequestResult.PermanentFailure:
                    break;
            }
        }

        public void DeliverPValueRequest(OnServerResponse onResponse = null)
        {
            if (onResponse == null)
            {
                onResponse = OnPValueRequestCompleted;
            }
            var payload = TracePayload.GetTracePayloadForPValueRequest(_resourceModel);
            MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload, onResponse));
        }

        private void OnPValueRequestCompleted(TracePayload payload, UnityWebRequest req, double newProbability)
        {
            if (GetRequestResult(req) == RequestResult.Success)
            {
                CheckForProbabilityUpdate(newProbability);
            }
        }

        private IEnumerator PushToServer(TracePayload payload, OnServerResponse onServerResponse)
        {
            byte[] body = null;
            // There is no threading on webgl, so we treat the payload differently
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                body = Encoding.ASCII.GetBytes(payload.GetJsonBody());
            }
            else
            {
                var bodyReady = false;
                new Thread(() =>
                {
                    body = Encoding.ASCII.GetBytes(payload.GetJsonBody());
                    bodyReady = true;
                }).Start();
                yield return new WaitUntil(() => bodyReady);
            }

            if (body == null)
            {
                yield break;
            }

            using (var req = new UnityWebRequest(_config.GetEndpoint()))
            {
                foreach (var header in payload.Headers)
                {
                    req.SetRequestHeader(header.Key, header.Value);
                }
                req.SetRequestHeader("Bugsnag-Api-Key", _config.ApiKey);
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Bugsnag-Integrity", "sha1 " + Hash(body));
                req.SetRequestHeader("Bugsnag-Sent-At", DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
                req.SetRequestHeader("Bugsnag-Uncompressed-Content-Length", body.Length.ToString());

                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.method = UnityWebRequest.kHttpVerbPOST;

                yield return req.SendWebRequest();

                var newProbability = GetRequestResult(req) == RequestResult.Success ? ReadResponseProbability(req) : double.NaN;
                onServerResponse(payload, req, newProbability);
            }
        }

        private void CheckForProbabilityUpdate(double newProbability)
        {
            var onProbabilityChanged = _onProbabilityChanged;
            if (onProbabilityChanged != null && !Double.IsNaN(newProbability))
            {
                onProbabilityChanged(newProbability);
            }
        }

        private static double ReadResponseProbability(UnityWebRequest req)
        {
            try
            {
                var probabilityStr = req.GetResponseHeader("Bugsnag-Sampling-Probability");
                if (probabilityStr != null)
                {
                    return double.Parse(probabilityStr, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
            }
            return double.NaN;
        }

        private void FlushCache()
        {
            if (_flushingCache)
            {
                return;
            }
            _flushingCache = true;
            MainThreadDispatchBehaviour.Instance().Enqueue(DoFlushCache());
        }

        private IEnumerator DoFlushCache()
        {
            var payloads = _cacheManager.GetCachedBatchesForDelivery();
            foreach (var payload in payloads)
            {
                //Process one batch at a time to save on performance costs of web requests
                yield return PushToServer(payload, OnTraceDeliveryCompleted);
            }
            _flushingCache = false;
        }

        private void PayloadSendSuccess(string id)
        {
            _cacheManager.RemoveCachedBatch(id);
        }

        private string Hash(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(input);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
