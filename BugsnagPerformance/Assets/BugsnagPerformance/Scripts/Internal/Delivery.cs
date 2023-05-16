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
    internal class Delivery
    {

        private static PerformanceConfiguration _configuration => BugsnagPerformance.Configuration;

        private static bool _flushingCache;

        public static void Deliver(List<Span> batch)
        {
            var payload = new TracePayload(batch);
            MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload));
        }

        static IEnumerator PushToServer(TracePayload payload)
        {
            // This data must be initialised on the main thread due to unity api usage
            ResourceModel.InitResourceDataOnMainThread();
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

            using (var req = new UnityWebRequest(_configuration.Endpoint))
            {

                req.SetRequestHeader("Bugsnag-Api-Key", _configuration.ApiKey);
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Bugsnag-Integrity", "sha1 " + Hash(body));
                req.SetRequestHeader("Bugsnag-Sent-At", DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture));

                //Temporary hardcoded header until sampling is completed
                req.SetRequestHeader("Bugsnag-Span-Sampling", string.Format("1:{0}", payload.BatchSize));

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
                    CacheManager.CacheBatch(payload);
                }
                else
                {
                    // sending failed with an unacceptable status code or network error
                    // do nothing
                }
            }
        }

        public static void FlushCache()
        {
            if (_flushingCache)
            {
                return;
            }
            _flushingCache = true;
            MainThreadDispatchBehaviour.Instance().Enqueue(DoFlushCache());
        }

        private static IEnumerator DoFlushCache()
        {
            var payloads = CacheManager.GetCachedBatchesForDelivery();
            foreach (var payload in payloads)
            {
                //Process one batch at a time to save on performance costs of web requests
                yield return PushToServer(payload);
            }
            _flushingCache = false;
        }

        private static void PayloadSendSuccess(string id)
        {
            CacheManager.RemoveCachedBatch(id);
        }

        private static string Hash(byte[] input)
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
