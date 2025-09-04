namespace BugsnagUnityPerformance
{
    internal class SpanControlRegistry
    {
        private readonly AppStartHandler _appStartHandler;

        public SpanControlRegistry(AppStartHandler handler)
        {
            _appStartHandler = handler;
        }

        public T GetSpanControl<T>(ISpanQuery<T> query) where T : class
        {
            if (query is SpanType.AppStartQuery)
            {
                var span = _appStartHandler.GetAppStartSpan();
                return span != null ? new AppStartSpanControl(span) as T : null;
            }

            return null;
        }
    }
}