using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagPerformance
{
    internal class Delivery
    {

        private PerformanceConfiguration _configuration;

        internal Delivery(PerformanceConfiguration performanceConfiguration)
        {
            _configuration = performanceConfiguration;
        }

        public void Deliver(TracePayload payload)
        {
            MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload));
        }

        // Push to the server and handle the result
        IEnumerator PushToServer(TracePayload payload)
        {
            byte[] body = null;           
            // There is no threading on webgl, so we treat the payload differently
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                body = payload.GetBody();
            }
            else
            {
                var bodyReady = false;
                new Thread(() => {
                    body = payload.GetBody();
                    bodyReady = true;
                }).Start();
                while (!bodyReady)
                {
                    yield return null;
                }
            }

            using (var req = new UnityWebRequest(_configuration.Endpoint))
            {
                foreach (var header in payload.Headers)
                {
                    req.SetRequestHeader(header.Key, header.Value);
                }
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.method = UnityWebRequest.kHttpVerbPOST;
                yield return req.SendWebRequest();

                while (!req.isDone)
                {
                    yield return new WaitForEndOfFrame();
                }
                var code = req.responseCode;
                if (code == 200 || code == 202)
                {
                    // success!
                    Debug.Log("Success!");
                }
                else if (code == 0 || code == 408 || code == 429 || code >= 500)
                {
                    // sending failed with no network or retryable error, cache payload to disk
                    Debug.Log("Something wnt wrong: res code: " + code);
                }
                else
                {
                    // sending failed with an unacceptable status code, remove payload from cache and pending payloads
                    Debug.Log("Something wnt wrong: res code: " + code);
                }
            }
        }

    }
}
