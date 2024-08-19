using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BugsnagUnityPerformance;
using System;
using System.Text.RegularExpressions;

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

        [Test]
        public void TracePropagationUrlsTest()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);

            Regex pattern1 = new Regex("https://example.com/.*");
            Regex pattern2 = new Regex("https://anotherexample.com/.*");

            config.TracePropagationUrls = new[] { pattern1, pattern2 };

            Assert.AreEqual(2, config.TracePropagationUrls.Length);
            Assert.AreEqual(pattern1.ToString(), config.TracePropagationUrls[0].ToString());
            Assert.AreEqual(pattern2.ToString(), config.TracePropagationUrls[1].ToString());

            string matchingUrl1 = "https://example.com/path/to/resource";
            string nonMatchingUrl1 = "https://notexample.com/path";
            string matchingUrl2 = "https://anotherexample.com/another/path";
            string nonMatchingUrl2 = "https://example.com/not/anotherexample";

            Assert.IsTrue(config.TracePropagationUrls[0].IsMatch(matchingUrl1));
            Assert.IsFalse(config.TracePropagationUrls[0].IsMatch(nonMatchingUrl1));
            Assert.IsTrue(config.TracePropagationUrls[1].IsMatch(matchingUrl2));
            Assert.IsFalse(config.TracePropagationUrls[1].IsMatch(nonMatchingUrl2));
        }

        [Test]
        public void CustomSamplingValue()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            Assert.IsFalse(config.IsSamplingProbabilityOverriden);
            config.SamplingProbability = 0.5;
            Assert.IsTrue(config.IsSamplingProbabilityOverriden);
        }

    }
}
