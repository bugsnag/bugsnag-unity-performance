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
        private const string LEGACY_DEFAULT_ENDPOINT = "https://otlp.bugsnag.com/v1/traces";
        private const string DEFAULT_ENDPOINT_FORMAT = "https://{0}.otlp.bugsnag.com/v1/traces";

        private void OnSpanEnd(Span span)
        {
            // Nothing to do
        }

        [Test]
        public void TestEndpointWhenUnset()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            var endpoint = config.GetEndpoint();
            var expectedEndpoint = string.Format(DEFAULT_ENDPOINT_FORMAT, VALID_API_KEY);
            Assert.AreEqual(expectedEndpoint, endpoint);
        }

        [Test]
        public void TestEndpointWhenSetToLegacyDefault()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.Endpoint = LEGACY_DEFAULT_ENDPOINT;
            var endpoint = config.GetEndpoint();
            var expectedEndpoint = string.Format(DEFAULT_ENDPOINT_FORMAT, VALID_API_KEY);
            Assert.AreEqual(expectedEndpoint, endpoint);
        }

        [Test]
        public void TestEndpointWhenSetToCustomValue()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            string customEndpoint = "https://custom.endpoint.com/traces";
            config.Endpoint = customEndpoint;
            var endpoint = config.GetEndpoint();
            Assert.AreEqual(customEndpoint, endpoint);
        }

        [Test]
        public void TestEndpointWithNullApiKey()
        {
            var config = new PerformanceConfiguration(null);
            var endpoint = config.GetEndpoint();
            var expectedEndpoint = string.Format(DEFAULT_ENDPOINT_FORMAT, string.Empty);
            Assert.AreEqual(expectedEndpoint, endpoint);
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
                OnSpanEnd,
                128,
                null);
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
            Assert.IsFalse(config.IsFixedSamplingProbability);
            config.SamplingProbability = 0.5;
            Assert.IsTrue(config.IsFixedSamplingProbability);
        }

        [Test]
        public void TestAttributeStringValueLimit()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);

            Assert.AreEqual(PerformanceConfiguration.DEFAULT_ATTRIBUTE_STRING_VALUE_LIMIT, config.AttributeStringValueLimit);

            config.AttributeStringValueLimit = 5000;
            Assert.AreEqual(5000, config.AttributeStringValueLimit);

            config.AttributeStringValueLimit = 10000;
            Assert.AreEqual(10000, config.AttributeStringValueLimit);

            config.AttributeStringValueLimit = -1;
            Assert.AreEqual(10000, config.AttributeStringValueLimit, "Value should not change if it's less than 0");

            config.AttributeStringValueLimit = 0;
            Assert.AreEqual(10000, config.AttributeStringValueLimit, "Value should not change if it's 0");

            config.AttributeStringValueLimit = 15000;
            Assert.AreEqual(10000, config.AttributeStringValueLimit, "Value should not exceed the maximum of 10000");
        }

        [Test]
        public void TestAttributeArrayLengthLimit()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);

            Assert.AreEqual(PerformanceConfiguration.DEFAULT_ATTRIBUTE_ARRAY_LENGTH_LIMIT, config.AttributeArrayLengthLimit);

            config.AttributeArrayLengthLimit = 500;
            Assert.AreEqual(500, config.AttributeArrayLengthLimit);

            config.AttributeArrayLengthLimit = 10000;
            Assert.AreEqual(10000, config.AttributeArrayLengthLimit);

            config.AttributeArrayLengthLimit = 0;
            Assert.AreEqual(10000, config.AttributeArrayLengthLimit, "Value should not change if it's 0");

            config.AttributeArrayLengthLimit = -1;
            Assert.AreEqual(10000, config.AttributeArrayLengthLimit, "Value should not change if it's less than 0");

            config.AttributeArrayLengthLimit = 15000;
            Assert.AreEqual(10000, config.AttributeArrayLengthLimit, "Value should not exceed the maximum of 10000");
        }

        [Test]
        public void TestAttributeCountLimit()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);

            Assert.AreEqual(PerformanceConfiguration.DEFAULT_ATTRIBUTE_COUNT_LIMIT, config.AttributeCountLimit);

            config.AttributeCountLimit = 500;
            Assert.AreEqual(500, config.AttributeCountLimit);

            config.AttributeCountLimit = 1000;
            Assert.AreEqual(1000, config.AttributeCountLimit);

            config.AttributeCountLimit = -1;
            Assert.AreEqual(1000, config.AttributeCountLimit, "Value should not change if it's less than 0");

            config.AttributeCountLimit = 0;
            Assert.AreEqual(1000, config.AttributeCountLimit, "Value should not change if it's 0");

            config.AttributeCountLimit = 1500;
            Assert.AreEqual(1000, config.AttributeCountLimit, "Value should not exceed the maximum of 1000");
        }



    }
}
