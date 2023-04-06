using System;
using System.Collections.Generic;
using BugsnagNetworking;

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
        private Tracer _tracer;
        private bool _ended;
        private object _endLock = new object();
        private BugsnagUnityWebRequest _request;

        internal Span(string name, SpanKind kind, string id, string traceId, DateTimeOffset startTime, Tracer tracer)
        {
            Name = name;
            Kind = kind;
            Id = id;
            TraceId = traceId;
            StartTime = startTime;
            _tracer = tracer;
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
            _tracer.OnSpanEnd(this);
        }

        public void EndNetworkSpan(BugsnagUnityWebRequest request)
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
            if (request.uploadHandler != null)
            {
                SetAttribute("http.request_content_length", request.uploadHandler.data.Length.ToString());
            }
            if (request.downloadHandler != null)
            {
                SetAttribute("http.response_content_length", request.downloadHandler.data.Length.ToString());
            }
            _tracer.OnSpanEnd(this);
        }

        internal void SetAttribute(string key, string value)
        {
            Attributes.Add(new AttributeModel(key, value));
        }



    }
}
