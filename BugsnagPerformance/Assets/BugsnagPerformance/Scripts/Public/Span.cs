using System;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class Span : ISpanContext
    {
        
        public string Name { get; internal set; }
        public SpanKind Kind { get; }
        public string SpanId { get; }
        public string TraceId { get; }
        internal string ParentSpanId { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset EndTime { get; internal set; }
        internal List<AttributeModel> Attributes = new List<AttributeModel>();
        private bool _ended;
        private object _endLock = new object();

        internal Span(string name, SpanKind kind, string id, string traceId, string parentSpanId, DateTimeOffset startTime, bool? isFirstClass)
        {
            Name = name;
            Kind = kind;
            SpanId = id;
            TraceId = traceId;
            StartTime = startTime;
            ParentSpanId = parentSpanId;
            if (isFirstClass != null)
            {
                SetAttribute("bugsnag.span.first_class",isFirstClass.Value);
            }
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

        internal void SetAttribute(string key, bool value)
        {
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void EndSceneLoadSpan(string sceneName)
        {
            // no need for thread safe checks as all scene load events happen on the main thread.
            _ended = true;
            EndTime = DateTimeOffset.Now;
            Name = "[ViewLoad/Scene]" + sceneName;
            SetAttribute("bugsnag.span_category", "view_load");
            SetAttribute("bugsnag.view.type", "scene");
            SetAttribute("bugsnag.view.name", sceneName);
            Tracer.OnSpanEnd(this);
        }

    }
}
