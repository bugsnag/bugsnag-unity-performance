using NUnit.Framework;
using UnityEngine;
using BugsnagUnityPerformance;
using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Tests
{
    public class SamplerTests
    {
        private const string VALID_API_KEY = "227df1042bc7772c321dbde3b31a03c2";

        private void OnSpanEnd(Span span)
        {
            // Nothing to do
        }

        private Sampler NewSampler(PerformanceConfiguration config, bool clearSavedData)
        {
            var cacheManager = new CacheManager(Application.temporaryCachePath);
            if (clearSavedData)
            {
                // Make sure we have no persistent state for the sampler to pick up.
                cacheManager.Clear();
            }
            var persistentState = new PersistentState(cacheManager);
            var sampler = new Sampler(persistentState);

            cacheManager.Configure(config);
            persistentState.Configure(config);
            sampler.Configure(config);

            cacheManager.Start();
            persistentState.Start();
            sampler.Start();

            return sampler;
        }

        [Test]
        public void TestDefaultConfig()
        {
            var sampler = NewSampler(new PerformanceConfiguration(VALID_API_KEY), true);

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "f000000000000000",
                "f0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            Assert.AreEqual(1.0, sampler.Probability);
            Assert.IsTrue(sampler.Sampled(span));
        }

        [Test]
        public void TestPersistence()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            var sampler = NewSampler(config, true);
            Assert.AreEqual(1.0, sampler.Probability);

            sampler.Probability = 0.5;
            Assert.AreEqual(0.5, sampler.Probability);

            // Start a new Sampler that will pick up the existing saved data
            sampler = NewSampler(config, false);
            Assert.AreEqual(0.5, sampler.Probability);

            // Clear saved data and then start a new Sampler
            sampler = NewSampler(config, true);
            Assert.AreEqual(1.0, sampler.Probability);
        }

        [Test]
        public void TestProbability1_0()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            var sampler = NewSampler(config, true);

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "f000000000000000",
                "f0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            Assert.IsTrue(sampler.Sampled(span));
        }

        [Test]
        public void TestProbability0_0()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.SamplingProbability = 0.0;
            var sampler = NewSampler(config, true);

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "10000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

            Assert.IsFalse(sampler.Sampled(span));
        }

        [Test]
        public void TestProbability0_1()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.SamplingProbability = 0.1;
            var sampler = NewSampler(config, true);

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "f0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

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
            var config = new PerformanceConfiguration(VALID_API_KEY);
            config.SamplingProbability = 0.9;
            var sampler = NewSampler(config, true);

            var span = new Span("test",
                SpanKind.SPAN_KIND_INTERNAL,
                "1000000000000000",
                "c0000000000000000000000000000000",
                "",
                DateTimeOffset.Now,
                true,
                OnSpanEnd);

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
