﻿using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class SpanModel
    {
        public string name;
        public int kind;
        public string spanId;
        public string traceId;
        public string startTimeUnixNano;
        public string endTimeUnixNano;
        public AttributeModel[] attributes;

        public SpanModel(Span span)
        {
            name = span.Name;
            kind = (int)span.Kind;
            spanId = span.Id;
            traceId = span.TraceId.Replace("-",string.Empty);
            startTimeUnixNano = (span.StartTime.Ticks * 100).ToString();
            endTimeUnixNano = (span.EndTime.Ticks * 100).ToString();
            attributes = new AttributeModel[]
            {
                new AttributeModel()
                {
                    key = "bugsnag.span.category",
                    value = new AttributeStringValueModel("app_start")
                }
            };
        }
    }
}
