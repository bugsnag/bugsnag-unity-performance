namespace BugsnagUnityPerformance
{
    public interface IAppStartSpanControl
    {
        void SetType(string type);
        void ClearType();
    }
    public static class SpanType
    {
        public static readonly AppStartQuery AppStart = new AppStartQuery();

        public class AppStartQuery : ISpanQuery<IAppStartSpanControl> { }
    }

    public interface ISpanQuery<T> { }

}