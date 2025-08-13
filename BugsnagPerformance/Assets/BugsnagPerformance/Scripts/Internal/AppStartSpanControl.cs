namespace BugsnagUnityPerformance
{
    internal class AppStartSpanControl : IAppStartSpanControl
    {
        private const string Prefix = "[AppStart/UnityRuntime]";
        private readonly Span _span;

        public AppStartSpanControl(Span span) => _span = span;

        public void SetType(string type)
        {
            _span?.TryUpdateAppStartSpan(type, Prefix);
        }

        public void ClearType()
        {
            _span?.TryUpdateAppStartSpan(null, Prefix);
        }
    }
}