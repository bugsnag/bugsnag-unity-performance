namespace BugsnagUnityPerformance
{
    public interface IAppStartSpanControl
    {
        void SetType(string type);
        void ClearType();
    }

    public interface ISpanQuery<T> { }

    public static class SpanType
    {
        public static readonly ISpanQuery<IAppStartSpanControl> AppStart = new AppStartQuery();
        internal sealed class AppStartQuery : ISpanQuery<IAppStartSpanControl> { }
    }
}