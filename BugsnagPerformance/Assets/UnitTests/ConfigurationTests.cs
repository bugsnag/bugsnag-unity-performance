using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BugsnagUnityPerformance;
using System;

namespace Tests
{
    public class ConfigurationTests
    {

        private const string VALID_API_KEY = "227df1042bc7772c321dbde3b31a03c2";

        private DateTimeOffset CustomStartTime = new DateTimeOffset(1985, 1, 1, 1, 1, 1, System.TimeSpan.Zero);
        private DateTimeOffset CustomEndTime = new DateTimeOffset(1986, 1, 1, 1, 1, 1, System.TimeSpan.Zero);

        private void OnSpanEnd(Span span)
        {
            // Nothing to do
        }

        [Test]
        public void ReleaseStage()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            Assert.AreEqual("development", config.ReleaseStage);
            config.ReleaseStage = "test";
            Assert.AreEqual("test", config.ReleaseStage);
        }

        [Test]
        public void TestCustomSpanTimes()
        {
            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "f000000000000000",
                "f0000000000000000000000000000000",
                "",
                CustomStartTime,
                true,
                OnSpanEnd);
            span.End(CustomEndTime);
            Assert.AreEqual(span.StartTime, CustomStartTime);
            Assert.AreEqual(span.EndTime, CustomEndTime);
        }

    }
}
