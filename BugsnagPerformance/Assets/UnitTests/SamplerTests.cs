using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BugsnagUnityPerformance;
using System;

namespace Tests
{
    public class SamplerTests
    {
        private const string VALID_API_KEY = "227df1042bc7772c321dbde3b31a03c2";

        private void OnSpanEnd(Span span)
        {
            // Nothing to do
        }

        [Test]
        public void TestDefaultConfig()
        {
            var sampler = new Sampler();

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "f000000000000000",
                "f0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            var config = new PerformanceConfiguration(VALID_API_KEY);
            sampler.Configure(config);
            sampler.Start();

            Assert.IsTrue(sampler.Sampled(span));
        }

        [Test]
        public void TestProbability1_0()
        {
            var sampler = new Sampler();

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "f000000000000000",
                "f0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.SamplingProbability = 1.0;
            sampler.Configure(config);
            sampler.Start();

            Assert.IsTrue(sampler.Sampled(span));
        }

        [Test]
        public void TestProbability0_0()
        {
            var sampler = new Sampler();

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "10000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.SamplingProbability = 0.0;
            sampler.Configure(config);
            sampler.Start();

            Assert.IsFalse(sampler.Sampled(span));
        }

        [Test]
        public void TestProbability0_1()
        {
            var sampler = new Sampler();

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "f0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.SamplingProbability = 0.1;
            sampler.Configure(config);
            sampler.Start();

            Assert.IsFalse(sampler.Sampled(span));

            span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "10000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);
            Assert.IsTrue(sampler.Sampled(span));

        }

        [Test]
        public void TestProbability0_9()
        {
            var sampler = new Sampler();

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "c0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.SamplingProbability = 0.9;
            sampler.Configure(config);
            sampler.Start();

            Assert.IsTrue(sampler.Sampled(span));

            span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "10000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);
            Assert.IsTrue(sampler.Sampled(span));

            span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "ffffffffffffffffffffffffffffffff",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);
            Assert.IsFalse(sampler.Sampled(span));

        }
    }
}
