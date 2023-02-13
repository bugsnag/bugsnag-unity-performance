﻿using NUnit.Framework;
using System;

namespace BugsnagUnityPerformance
{
    [TestFixture]
    public class ConfigurationTests
    {
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
            new PerformanceConfiguration("227df1042bc7772c321dbde3b31a03c2");
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
