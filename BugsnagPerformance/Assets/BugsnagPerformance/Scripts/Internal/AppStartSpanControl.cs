namespace BugsnagUnityPerformance
{
    internal class AppStartSpanControl : IAppStartSpanControl
    {
        private const string APP_START_SPAN_PREFIX = "[AppStart/UnityRuntime]";
        private const string APP_START_NAME_ATTRIBUTE = "bugsnag.app_start.name";
        private readonly Span _span;

        public AppStartSpanControl(Span span)
        {
            _span = span;
        }

        public void SetType(string type)
        {
            if (_span != null && !_span.Ended)
            {
                _span.Name = APP_START_SPAN_PREFIX + type;
                _span.SetAttributeInternal(APP_START_NAME_ATTRIBUTE, type);
            }
        }

        public void ClearType()
        {
            if (_span != null && !_span.Ended)
            {
                _span.Name = APP_START_SPAN_PREFIX;
                _span.SetAttributeInternal(APP_START_NAME_ATTRIBUTE, (string)null);
            }
        }
    }
}