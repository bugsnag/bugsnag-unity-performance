namespace BugsnagUnityPerformance
{
    internal class AppStartSpanControl : IAppStartSpanControl
    {
        private const string APP_START_SPAN_PREFIX = "[AppStart/UnityRuntime]";
        private readonly Span _span;

        public AppStartSpanControl(Span span) => _span = span;

        public void SetType(string type)
        {
            _span?.TrySetAppStartType(type, APP_START_SPAN_PREFIX);
        }

        public void ClearType()
        {
            _span?.TrySetAppStartType(null, APP_START_SPAN_PREFIX);
        }
    }
}