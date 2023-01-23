using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnityPerformance
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
                req.SetRequestHeader("Bugsnag-Api-Key", _configuration.ApiKey);
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Bugsnag-Integrity", "sha1 " + Hash(body));
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
                }
                else if (code == 0 || code == 408 || code == 429 || code >= 500)
                {
                    // sending failed with no network or retryable error
                }
                else
                {
                    // sending failed with an unacceptable status code or network error
                }
            }
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
