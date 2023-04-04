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




        // Listener related wrappers
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


        // Expose Instance Methods

        public void Dispose()
        {
            if (UnityWebRequest != null)
            {
                UnityWebRequest.Dispose();
            }
        }

        public bool IsDone
        {
            get { return UnityWebRequest.isDone; }
        }

        public bool IsNetworkError
        {
            get { return UnityWebRequest.isNetworkError; }
        }

        public bool IsHttpError
        {
            get { return UnityWebRequest.isHttpError; }
        }

        public string Error
        {
            get { return UnityWebRequest.error; }
        }

        public bool IsSuccess
        {
            get { return !IsNetworkError && !IsHttpError && IsDone; }
        }

        public long ResponseCode
        {
            get { return UnityWebRequest.responseCode; }
        }

        public string DownloadHandlerText
        {
            get { return UnityWebRequest.downloadHandler.text; }
        }

        public byte[] DownloadHandlerData
        {
            get { return UnityWebRequest.downloadHandler.data; }
        }

        public float DownloadProgress
        {
            get { return UnityWebRequest.downloadProgress; }
        }

        public float UploadProgress
        {
            get { return UnityWebRequest.uploadProgress; }
        }

    }

}