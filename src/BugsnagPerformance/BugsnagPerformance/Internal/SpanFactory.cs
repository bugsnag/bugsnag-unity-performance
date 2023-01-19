using System;
namespace BugsnagPerformance
{
    public class SpanFactory
    {

        private static Random _rand = new Random();


        private static string GetNewTraceId()
        {
            return new Guid().ToString();
        }

        private static long GetNewSpanId()
        {
            return _rand.Next() + _rand.Next();
        }

        internal static Span StartCustomSpan(string name, DateTimeOffset startTime)
        {
            return new Span(name,SpanKind.SPAN_KIND_INTERNAL,GetNewSpanId(),GetNewTraceId(), startTime);
        }
    }
}
