using System;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class Span
    {
        
        public string Name { get; }
        public SpanKind Kind { get; }
        public string Id { get; }
        public string TraceId { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset EndTime { get; private set; }
        internal List<AttributeModel> Attributes = new List<AttributeModel>();
        private bool _ended;
        private object _endLock = new object();

        internal Span(string name, SpanKind kind, string id, string traceId, DateTimeOffset startTime)
        {
            Name = name;
            Kind = kind;
            Id = id;
            TraceId = traceId;
            StartTime = startTime;
        }

        public void End()
        {
            lock (_endLock)
            {
                if (_ended)
                {
                    return;
                }
                _ended = true;
            }
            EndTime = DateTimeOffset.Now;
            Tracer.OnSpanEnd(this);
        }

        internal void EndNetworkSpan(BugsnagUnityWebRequest request)
        {
            lock (_endLock)
            {
                if (_ended)
                {
                    return;
                }
                _ended = true;
            }

            EndTime = DateTimeOffset.Now;

            SetAttribute("http.status_code", request.responseCode.ToString());

            if (request.uploadHandler != null && request.uploadHandler.data != null)
            {
                SetAttribute("http.request_content_length", request.uploadHandler.data.Length.ToString());
            }

            if (request.downloadHandler != null && request.downloadHandler.data != null)
            {
                SetAttribute("http.response_content_length", request.downloadHandler.data.Length.ToString());
            }

            Tracer.OnSpanEnd(this);
        }

        internal void SetAttribute(string key, string value)
        {
            Attributes.Add(new AttributeModel(key, value));
        }



    }
}
