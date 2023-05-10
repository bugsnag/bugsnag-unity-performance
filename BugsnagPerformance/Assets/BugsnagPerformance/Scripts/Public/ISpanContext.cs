namespace BugsnagUnityPerformance
{
    public interface ISpanContext
    {
        string SpanId { get; }
        string TraceId { get; }
    }
}