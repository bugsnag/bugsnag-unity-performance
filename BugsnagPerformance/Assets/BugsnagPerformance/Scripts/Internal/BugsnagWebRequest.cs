using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagNetworking
{

    public class BugsnagWebRequest : IDisposable
    {

        private static List<BugsnagNetworkListener> _listeners = new List<BugsnagNetworkListener>();

        public UnityWebRequest UnityWebRequest;


        public static void AddNetworkListener(BugsnagNetworkListener listener)
        {
            _listeners.Add(listener);
        }

        // Constructors
        public BugsnagWebRequest()
        {
            UnityWebRequest = new UnityWebRequest();
        }

        public BugsnagWebRequest(UnityWebRequest unityWebRequest)
        {
            UnityWebRequest = unityWebRequest;
        }

        public BugsnagWebRequest(string url)
        {
            UnityWebRequest = new UnityWebRequest(url);
        }

        public BugsnagWebRequest(Uri uri)
        {
            UnityWebRequest = new UnityWebRequest(uri);
        }

        public BugsnagWebRequest(string url, string method)
        {
            UnityWebRequest = new UnityWebRequest(url, method);
        }

        public BugsnagWebRequest(Uri uri, string method)
        {
            UnityWebRequest = new UnityWebRequest(uri, method);
        }

        // Static Constructors

        // Get
        public static BugsnagWebRequest Get(string uri)
        {
            return new BugsnagWebRequest(UnityWebRequest.Get(uri));
        }

        public static BugsnagWebRequest Get(Uri uri)
        {
            return new BugsnagWebRequest(UnityWebRequest.Get(uri));
        }

        // Post
        public static BugsnagWebRequest Post(string uri, string postData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, postData));
        }

        public static BugsnagWebRequest Post(string uri, WWWForm formData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, formData));
        }

        public static BugsnagWebRequest Post(string uri, List<IMultipartFormSection> multipartFormSections)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, multipartFormSections));
        }

        public static BugsnagWebRequest Post(string uri, Dictionary<string, string> formFields)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, formFields));
        }

        public static BugsnagWebRequest Post(Uri uri, string postData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, postData));
        }

        public static BugsnagWebRequest Post(Uri uri, WWWForm formData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, formData));
        }

        public static BugsnagWebRequest Post(Uri uri, List<IMultipartFormSection> multipartFormSections)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, multipartFormSections));
        }

        public static BugsnagWebRequest Post(Uri uri, Dictionary<string, string> formFields)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, formFields));
        }

        public static BugsnagWebRequest Post(string uri, List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, multipartFormSections, boundary));
        }

        public static BugsnagWebRequest Post(Uri uri, List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            return new BugsnagWebRequest(UnityWebRequest.Post(uri, multipartFormSections, boundary));
        }

        // Put
        public static BugsnagWebRequest Put(string uri, byte[] bodyData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        public static BugsnagWebRequest Put(string uri, string bodyData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        public static BugsnagWebRequest Put(Uri uri, byte[] bodyData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        public static BugsnagWebRequest Put(Uri uri, string bodyData)
        {
            return new BugsnagWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        // Head
        public static BugsnagWebRequest Head(string uri)
        {
            return new BugsnagWebRequest(UnityWebRequest.Head(uri));
        }

        public static BugsnagWebRequest Head(Uri uri)
        {
            return new BugsnagWebRequest(UnityWebRequest.Head(uri));
        }

        // Delete
        public static BugsnagWebRequest Delete(string uri)
        {
            return new BugsnagWebRequest(UnityWebRequest.Delete(uri));
        }

        public static BugsnagWebRequest Delete(Uri uri)
        {
            return new BugsnagWebRequest(UnityWebRequest.Delete(uri));
        }

        // Static Methods

        public static void ClearCookieCache()
        {
            UnityWebRequest.ClearCookieCache();
        }

        public static string EscapeURL(string s)
        {
            return UnityWebRequest.EscapeURL(s);
        }

        public static string EscapeURL(string s, Encoding e)
        {
            return UnityWebRequest.EscapeURL(s, e);
        }

        public static string UnEscapeURL(string s)
        {
            return UnityWebRequest.UnEscapeURL(s);
        }

        public static string UnEscapeURL(string s, Encoding e)
        {
            return UnityWebRequest.UnEscapeURL(s, e);
        }

        public static byte[] GenerateBoundary()
        {
            return UnityWebRequest.GenerateBoundary();
        }

        public static byte[] SerializeFormSections(List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            return UnityWebRequest.SerializeFormSections(multipartFormSections, boundary);
        }

        public static byte[] SerializeSimpleForm(Dictionary<string, string> formFields)
        {
            return UnityWebRequest.SerializeSimpleForm(formFields);
        }




        // Public methods

        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            foreach (var listener in _listeners)
            {
                listener.OnSend(UnityWebRequest);
            }
            var asyncAction = UnityWebRequest.SendWebRequest();
            asyncAction.completed += RequestCompleted;
            return asyncAction;
        }

        private void RequestCompleted(AsyncOperation obj)
        {
            foreach (var listener in _listeners)
            {
                listener.OnComplete(UnityWebRequest);
            }
        }

        public void Abort()
        {
            foreach (var listener in _listeners)
            {
                listener.OnAbort(UnityWebRequest);
            }
            UnityWebRequest.Abort();
        }

        public string GetRequestHeader(string name) => UnityWebRequest.GetRequestHeader(name);

        public string GetResponseHeader(string name) => UnityWebRequest.GetResponseHeader(name);

        public Dictionary<string, string> GetResponseHeaders() => UnityWebRequest.GetResponseHeaders();

        public void SetRequestHeader(string name, string value) => UnityWebRequest.SetRequestHeader(name, value);

        public void Dispose() => UnityWebRequest.Dispose();

        // Static Properties

        public static string kHttpVerbCREATE => UnityWebRequest.kHttpVerbCREATE;

        public static string kHttpVerbDELETE => UnityWebRequest.kHttpVerbDELETE;

        public static string kHttpVerbGET => UnityWebRequest.kHttpVerbGET;

        public static string kHttpVerbHEAD => UnityWebRequest.kHttpVerbHEAD;

        public static string kHttpVerbPOST => UnityWebRequest.kHttpVerbPOST;

        public static string kHttpVerbPUT => UnityWebRequest.kHttpVerbPUT;

        // Properties

        public UnityEngine.Networking.CertificateHandler certificateHandler
        {
            get { return UnityWebRequest.certificateHandler; }
            set { UnityWebRequest.certificateHandler = value; }
        }

        public bool disposeCertificateHandlerOnDispose
        {
            get { return UnityWebRequest.disposeCertificateHandlerOnDispose; }
            set { UnityWebRequest.disposeCertificateHandlerOnDispose = value; }
        }

        public bool disposeDownloadHandlerOnDispose
        {
            get { return UnityWebRequest.disposeDownloadHandlerOnDispose; }
            set { UnityWebRequest.disposeDownloadHandlerOnDispose = value; }
        }

        public bool disposeUploadHandlerOnDispose
        {
            get { return UnityWebRequest.disposeUploadHandlerOnDispose; }
            set { UnityWebRequest.disposeUploadHandlerOnDispose = value; }
        }

        public ulong downloadedBytes => UnityWebRequest.downloadedBytes;


        public UnityEngine.Networking.DownloadHandler downloadHandler
        {
            get { return UnityWebRequest.downloadHandler; }
            set { UnityWebRequest.downloadHandler = value; }
        }

        public float downloadProgress => UnityWebRequest.downloadProgress;

        public string error => UnityWebRequest.error;

        public bool isModifiable => UnityWebRequest.isModifiable;

        public string method
        {
            get { return UnityWebRequest.method; }
            set { UnityWebRequest.method = value; }
        }

        public int redirectLimit
        {
            get { return UnityWebRequest.redirectLimit; }
            set { UnityWebRequest.redirectLimit = value; }
        }

        public int timeout
        {
            get { return UnityWebRequest.timeout; }
            set { UnityWebRequest.timeout = value; }
        }

        public ulong uploadedBytes => UnityWebRequest.uploadedBytes;

        public Uri uri
        {
            get { return UnityWebRequest.uri; }
            set { UnityWebRequest.uri = value; }
        }

        public string url
        {
            get { return UnityWebRequest.url; }
            set { UnityWebRequest.url = value; }
        }

        public bool useHttpContinue
        {
            get { return UnityWebRequest.useHttpContinue; }
            set { UnityWebRequest.useHttpContinue = value; }
        }


        public bool isDone => UnityWebRequest.isDone;

        public bool isNetworkError => UnityWebRequest.isNetworkError;

        public bool isHttpError => UnityWebRequest.isHttpError;

        public long responseCode => UnityWebRequest.responseCode;

        public float uploadProgress => UnityWebRequest.uploadProgress;
        

    }

}