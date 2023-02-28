using NUnit.Framework;
using System;

namespace BugsnagUnityPerformance
{
    [TestFixture]
    public class ConfigurationTests
    {

        private const string VALID_API_KEY = "227df1042bc7772c321dbde3b31a03c2";

        [Test]
        public void SetInvalidApiKey()
        {

            Assert.Throws(typeof(Exception), () =>
            {
                new PerformanceConfiguration("test");
            });
        }

        [Test]
        public void SetValidApiKey()
        {
            new PerformanceConfiguration(VALID_API_KEY);
        }

        [Test]
        public void ReleaseStage()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            Assert.AreEqual("production", config.ReleaseStage);

            config.ReleaseStage = "test";
            Assert.AreEqual("test", config.ReleaseStage);
        }

        [Test]
        public void CreateSimpleSpan()
        {
            var span = BugsnagPerformance.StartSpan("test");
            Assert.AreEqual("test", span.Name);
            Assert.AreEqual(SpanKind.SPAN_KIND_INTERNAL, span.Kind);
            Assert.AreEqual(36, span.TraceId.Length);
        }
    }
}
