namespace BugsnagPerformance
{
    public class BugsnagPerformance
    {

        private static PerformanceConfiguration _configuration;

        private static bool _isStarted = false;


        public static void Start(PerformanceConfiguration performanceConfiguration)
        {
            _configuration = performanceConfiguration;
            _isStarted = true;
        }
    }
}
