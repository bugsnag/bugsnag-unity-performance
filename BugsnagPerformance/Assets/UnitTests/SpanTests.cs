using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using BugsnagUnityPerformance;

namespace Tests
{
    public class SpanTests
    {
        private const int MAX_ATTRS = 2;   // keep small to test dropping logic easily
        private Span _span;
        private bool _callbackHit;

        // ---------- helpers --------------------------------------------------

        // simple callback that records it was invoked
        private void OnSpanEnd(Span s) => _callbackHit = true;

        // create a fresh span for each test
        private Span CreateSpan(string name = "test-span",
                                SpanKind kind = SpanKind.SPAN_KIND_INTERNAL,
                                int maxAttrs = MAX_ATTRS)
        {
            _callbackHit = false;
            return new Span(
                name,
                kind,
                "a000000000000000",
                "b0000000000000000000000000000000",
                "",
                DateTimeOffset.UtcNow,
                true,
                OnSpanEnd,
                maxAttrs
            );
        }

        // assert that the only attribute still present is bugsnag.span.first_class
        private static void AssertOnlyFirstClass(Span s)
        {
            var keys = s.GetAttributes().Keys;
            Assert.That(keys, Is.EquivalentTo(new[] { "bugsnag.span.first_class" }));
        }

        #region End / EndNetworkSpan

        [Test]
        public void End_SetsEndTimeAndInvokesCallback()
        {
            _span = CreateSpan();
            DateTimeOffset customEnd = new DateTimeOffset(2025, 6, 19, 12, 0, 0, TimeSpan.Zero);

            _span.End(customEnd);

            Assert.IsTrue(_span.Ended);
            Assert.AreEqual(customEnd, _span.EndTime);
            Assert.IsTrue(_callbackHit);
        }

        [Test]
        public void End_CalledTwice_SecondCallNoEffect()
        {
            _span = CreateSpan();
            var firstEnd  = _span.StartTime.AddSeconds(1);
            var secondEnd = _span.StartTime.AddSeconds(10);

            _span.End(firstEnd);   // accepted
            _span.End(secondEnd);  // ignored

            Assert.AreEqual(firstEnd, _span.EndTime,
                "EndTime should remain the value from the first End() call");
        }

        [Test]
        public void EndNetworkSpan_SetsExpectedHttpAttributes()
        {
            _span = CreateSpan();
            const int status = 201;
            const int reqLen = 42;
            const int resLen = 256;

            _span.EndNetworkSpan(status, reqLen, resLen);

            var attrs = _span.GetAttributes();
            Assert.AreEqual(status, attrs["http.status_code"]);
            Assert.AreEqual(reqLen,  attrs["http.request_content_length"]);
            Assert.AreEqual(resLen,  attrs["http.response_content_length"]);
        }

        [Test]
        public void EndNetworkSpan_WithDefaults_DoesNotAddHttpAttributes()
        {
            _span = CreateSpan();
            _span.EndNetworkSpan();   // all defaults are -1

            AssertOnlyFirstClass(_span);
        }

        #endregion

        #region Sampling probability

        [Test]
        public void UpdateSamplingProbability_OnlyLowersValue()
        {
            _span = CreateSpan();
            _span.UpdateSamplingProbability(0.75);
            Assert.AreEqual(0.75, _span.samplingProbability);

            _span.UpdateSamplingProbability(0.9);   // higher -> ignored
            Assert.AreEqual(0.75, _span.samplingProbability);
        }

        #endregion

        #region Attribute API & limits

        [Test]
        public void SetAttribute_RespectsMaxCustomAttributesAndDrops()
        {
            _span = CreateSpan(maxAttrs: 1);   // only room for 1 custom attribute

            _span.SetAttribute("key1", "value");
            Assert.AreEqual(0, _span.DroppedAttributesCount);

            _span.SetAttribute("key2", "value");   // should be dropped
            Assert.AreEqual(1, _span.DroppedAttributesCount);
            Assert.IsFalse(_span.GetAttributes().ContainsKey("key2"));
        }

        [Test]
        public void SetAttribute_NullRemovesExistingKey()
        {
            _span = CreateSpan();

            _span.SetAttribute("removable", 123);
            Assert.IsTrue(_span.GetAttributes().ContainsKey("removable"));

            _span.SetAttribute("removable", (string)null);   // remove
            Assert.IsFalse(_span.GetAttributes().ContainsKey("removable"));
        }

        #endregion

        #region CPU metrics

        private BugsnagUnityPerformance.SystemMetricsSnapshot MakeCpuSnapshot(
            double procCpu,
            double mainCpu,
            DateTimeOffset ts)
        {
            return new BugsnagUnityPerformance.SystemMetricsSnapshot
            {
                ProcessCPUPercent     = procCpu,
                MainThreadCPUPercent  = mainCpu,
                Timestamp             = ts.Millisecond * 1_000_000,  // convert to ns
            };
        }

        [Test]
        public void ApplyCPUMetrics_ZeroSamples_NoAdditionalAttributes()
        {
            _span = CreateSpan();
            _span.ApplyCPUMetrics(new List<BugsnagUnityPerformance.SystemMetricsSnapshot>());
            AssertOnlyFirstClass(_span);
        }

        [Test]
        public void ApplyCPUMetrics_EffectivelyZero_NoAdditionalAttributes()
        {
            _span = CreateSpan();
            var tiny = MakeCpuSnapshot(0.00005, 0.00003, DateTimeOffset.UtcNow);
            _span.ApplyCPUMetrics(new List<BugsnagUnityPerformance.SystemMetricsSnapshot> { tiny });
            AssertOnlyFirstClass(_span);
        }

        [Test]
        public void ApplyCPUMetrics_PopulatesExpectedKeys()
        {
            _span = CreateSpan();
            var ts     = DateTimeOffset.UtcNow;
            var snap1  = MakeCpuSnapshot(10, 5, ts);
            var snap2  = MakeCpuSnapshot(20, 6, ts.AddSeconds(1));

            _span.ApplyCPUMetrics(new List<BugsnagUnityPerformance.SystemMetricsSnapshot> { snap1, snap2 });

            var attrs = _span.GetAttributes();

            CollectionAssert.IsSubsetOf(
                new[]
                {
                    "bugsnag.system.cpu_measures_timestamps",
                    "bugsnag.system.cpu_measures_total",
                    "bugsnag.system.cpu_measures_main_thread",
                    "bugsnag.system.cpu_mean_total",
                    "bugsnag.system.cpu_mean_main_thread"
                },
                attrs.Keys);
        }

        [Test]
        public void RemoveSystemCPUMetrics_ClearsPreviouslySetKeys()
        {
            ApplyCPUMetrics_PopulatesExpectedKeys();  // sets metrics
            _span.RemoveSystemCPUMetrics();
            var keys = _span.GetAttributes().Keys;
            Assert.IsFalse(keys.Any(k => k.StartsWith("bugsnag.system.cpu")));
        }

        #endregion

        #region Remove helpers

        [Test]
        public void RemoveFrameRateMetrics_RemovesAllTargetKeys()
        {
            _span = CreateSpan();
            // Pretend attributes already exist
            foreach (var key in new[]
            {
                "bugsnag.rendering.frozen_frames",
                "bugsnag.rendering.slow_frames",
                "bugsnag.rendering.total_frames",
                "bugsnag.rendering.fps_maximum",
                "bugsnag.rendering.fps_minimum",
                "bugsnag.rendering.fps_average",
                "bugsnag.rendering.fps_target",
            })
            {
                _span.SetAttributeInternal(key, 1L);
            }

            _span.RemoveFrameRateMetrics();
            AssertOnlyFirstClass(_span);
        }

        [Test]
        public void RemoveSystemMemoryMetrics_RemovesAllTargetKeys()
        {
            _span = CreateSpan();
            foreach (var key in new[]
            {
                "bugsnag.device.physical_device_memory",
                "bugsnag.system.memory.timestamps",
                "bugsnag.system.memory.spaces.device.size",
                "bugsnag.system.memory.spaces.device.used",
                "bugsnag.system.memory.spaces.device.mean",
                "bugsnag.system.memory.spaces.art.size",
                "bugsnag.system.memory.spaces.art.used",
                "bugsnag.system.memory.spaces.art.mean"
            })
            {
                _span.SetAttributeInternal(key, 1L);
            }

            _span.RemoveSystemMemoryMetrics();
            AssertOnlyFirstClass(_span);
        }

        #endregion
    }
}