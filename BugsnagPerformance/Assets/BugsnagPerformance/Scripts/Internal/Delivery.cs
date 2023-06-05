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
    internal class Delivery : IPhasedStartup
    {
        private string _endpoint;
        private string _apiKey;

        private bool _flushingCache;

        private ResourceModel _resourceModel;
        private CacheManager _cacheManager;

        private const int MAX_PAYLOAD_BYTES = 1000000;


        public Delivery(ResourceModel resourceModel, CacheManager cacheManager)
        {
            _resourceModel = resourceModel;
            _cacheManager = cacheManager;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _endpoint = config.Endpoint;
            _apiKey = config.ApiKey;
        }

        public void Start()
        {
            FlushCache();
        }

        public void Deliver(List<Span> batch)
        {
            var payload = new TracePayload(_resourceModel, batch);
            MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload));
        }

        private IEnumerator PushToServer(TracePayload payload)
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
                new Thread(() => {
                    body = Encoding.ASCII.GetBytes(payload.GetJsonBody());
                    bodyReady = true;
                }).Start();
                yield return new WaitUntil(() => bodyReady);
            }

            if (body == null)
            {
                yield break;
            }

            using (var req = new UnityWebRequest(_endpoint))
            {
                foreach (var header in payload.Headers)
                {
                    req.SetRequestHeader(header.Key, header.Value);
                }
                req.SetRequestHeader("Bugsnag-Api-Key", _apiKey);
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Bugsnag-Integrity", "sha1 " + Hash(body));
                req.SetRequestHeader("Bugsnag-Sent-At", DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                req.SetRequestHeader("Bugsnag-Uncompressed-Content-Length", body.Length.ToString());

                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.method = UnityWebRequest.kHttpVerbPOST;

                yield return req.SendWebRequest();

                var code = req.responseCode;
                if (code == 200 || code == 202)
                {
                    // success!
                    PayloadSendSuccess(payload.PayloadId);
                    FlushCache();
                }
                else if (code == 0 || code == 408 || code == 429 || code >= 500)
                {
                    // sending failed with retryable error, cache for later retry
                    if (body.Length <= MAX_PAYLOAD_BYTES)
                    {
                        _cacheManager.CacheBatch(payload);
                    }
                }
                else
                {
                    // sending failed with an unacceptable status code or network error
                    // do nothing
                }
            }
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
                yield return PushToServer(payload);
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
