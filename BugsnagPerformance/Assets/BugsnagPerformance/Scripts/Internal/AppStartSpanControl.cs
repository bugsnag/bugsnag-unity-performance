using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class AppStartSpanControl : IAppStartSpanControl
    {
        private readonly Span _span;

        public AppStartSpanControl(Span span)
        {
            _span = span;
        }

        public void SetType(string type)
        {
            if (_span != null && !_span.Ended)
            {
                _span.Name = "[AppStart/UnityRuntime]" + type;
                _span.SetAttributeInternal("bugsnag.app_start.name", type);
            }
        }

        public void ClearType()
        {
            if (_span != null && !_span.Ended)
            {
                _span.Name = "[AppStart/UnityRuntime]";
                _span.SetAttributeInternal("bugsnag.app_start.name", (string)null);
            }
        }
    }
}