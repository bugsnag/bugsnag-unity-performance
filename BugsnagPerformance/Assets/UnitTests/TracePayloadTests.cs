using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BugsnagUnityPerformance;
using System;
using System.IO;

namespace Tests
{
    public class TracePayloadTests
    {

        private const string VALID_API_KEY = "227df1042bc7772c321dbde3b31a03c2";

        [Test]
        public void TestPersistenceNoHeaders()
        {
            var dir = CreateTempDir("TestPersistence");
            var payload = PayloadWithPValues(new List<double> { 1.0 });
            var filePath = dir + Path.DirectorySeparatorChar + "payload.bin";
            var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            payload.Serialize(stream);
            stream.Close();

            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var loadedPayload = TracePayload.Deserialize(payload.PayloadId, stream);
            stream.Close();

            Assert.IsTrue(payload.PayloadsAreEqual(loadedPayload));
            Assert.AreEqual("1:1", loadedPayload.Headers["Bugsnag-Span-Sampling"]);

            Directory.Delete(dir, true);
        }

        [Test]
        public void TestPersistenceOneHeader()
        {
            var dir = CreateTempDir("TestPersistence");
            var payload = PayloadWithPValues(new List<double> { 1.0 });
            payload.Headers["BugsnagSomething"] = "blah";
            var filePath = dir + Path.DirectorySeparatorChar + "payload.bin";
            var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            payload.Serialize(stream);
            stream.Close();

            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var loadedPayload = TracePayload.Deserialize(payload.PayloadId, stream);
            stream.Close();

            Assert.IsTrue(payload.PayloadsAreEqual(loadedPayload));
            Assert.AreEqual("1:1", loadedPayload.Headers["Bugsnag-Span-Sampling"]);

            Directory.Delete(dir, true);
        }

        [Test]
        public void TestPersistenceMultipleHeaders()
        {
            var dir = CreateTempDir("TestPersistence");
            var payload = PayloadWithPValues(new List<double> { 1.0 });
            payload.Headers["BugsnagSomething"] = "blah";
            payload.Headers["BugsnagSomethingElse"] = "foo";
            payload.Headers["BugsnagSomethingCompletelyDifferent"] = "bar";
            var filePath = dir + Path.DirectorySeparatorChar + "payload.bin";
            var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            payload.Serialize(stream);
            stream.Close();

            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var loadedPayload = TracePayload.Deserialize(payload.PayloadId, stream);
            stream.Close();

            Assert.IsTrue(payload.PayloadsAreEqual(loadedPayload));
            Assert.AreEqual("1:1", loadedPayload.Headers["Bugsnag-Span-Sampling"]);

            Directory.Delete(dir, true);
        }

        [Test]
        public void TestHistogram()
        {
            AssertSpanSamplingHistogram(new List<double> { }, "1:0");
            AssertSpanSamplingHistogram(new List<double> { 1.0 }, "1:1");
            AssertSpanSamplingHistogram(new List<double> { 1.0, 1.0 }, "1:2");
            AssertSpanSamplingHistogram(new List<double> { 1.0, 0.5, 1.0 }, "0.5:1;1:2");
            AssertSpanSamplingHistogram(new List<double> { 1.0, 0.1, 0.5, 1.0, 0.5, 0.2, 1.0, 0.5 }, "0.1:1;0.2:1;0.5:3;1:3");
        }

        private TracePayload PayloadWithPValues(List<double> pValues)
        {
            var cacheManager = new CacheManager(Application.temporaryCachePath);
            var resourceModel = new ResourceModel(cacheManager);
            return new TracePayload(resourceModel, SpansWithProbabilities(pValues),false,1,1);
        }

        private void AssertSpanSamplingHistogram(List<double> pValues, string expectedHistogram)
        {
            var payload = PayloadWithPValues(pValues);
            if (expectedHistogram.Length > 0)
            {
                Assert.AreEqual(expectedHistogram, payload.Headers["Bugsnag-Span-Sampling"]);
            } else
            {
                Assert.IsFalse(payload.Headers.ContainsKey("Bugsnag-Span-Sampling"));
            }
        }

        private List<Span> SpansWithProbabilities(List<double> pValues)
        {
            var result = new List<Span>();
            foreach (double p in pValues)
            {
                result.Add(SpanWithProbability(p));
            }
            return result;
        }
        private Span SpanWithProbability(double probability)
        {
            var span = new Span("test",
                            SpanKind.SPAN_KIND_INTERNAL,
                            "f000000000000000",
                            "00000000000000010000000000000000",
                            "",
                            DateTimeOffset.Now,
                            true,
                            OnSpanEnd,
                            128,
                null);
            span.UpdateSamplingProbability(probability);
            return span;
        }

        private void OnSpanEnd(Span span)
        {
            // Nothing to do
        }

        private string CreateTempDir(string name)
        {
            var dir = Application.persistentDataPath + Path.DirectorySeparatorChar + name;
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
                // Ignored
            }
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
