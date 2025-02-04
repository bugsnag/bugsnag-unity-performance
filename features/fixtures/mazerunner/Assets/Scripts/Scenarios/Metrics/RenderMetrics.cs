using System.Collections;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class RenderMetrics : Scenario
{
    bool _testStarted, _testEnded;
    int _slowFramesDone, _frozenFramesDone;
    Span _slowFrameSpan, _noFramesSpan, _disableInSpanOptionsSpan;

    int _frameCount;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(3);
        Configuration.AutoInstrumentRendering = true;
    }

    public override void Run()
    {
        base.Run();
        StartCoroutine(StartTest());
    }

    private IEnumerator StartTest()
    {
        yield return new WaitForSeconds(1);
        _slowFrameSpan = BugsnagPerformance.StartSpan("SlowFrames");
        _noFramesSpan = BugsnagPerformance.StartSpan("NoFrames", new SpanOptions { IsFirstClass = false });
        _disableInSpanOptionsSpan = BugsnagPerformance.StartSpan("DisableInSpanOptions", new SpanOptions { InstrumentRendering = false });
        _testStarted = true;
    }



    void Update()
    {
        if (!_testStarted || _testEnded)
        {
            return;
        }

        if (_slowFramesDone < 10)
        {
            float startTime = Time.realtimeSinceStartup;
            // Simulate a busy workload by blocking the main thread
            while (Time.realtimeSinceStartup < startTime + 0.25f){}
            _slowFramesDone++;
            return;
        }

        if (_frozenFramesDone < 1)
        {
            var startTime2 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < startTime2 + 5){}
            _frozenFramesDone++;
            return;
        }

        if (_frameCount < 100)
        {
            _frameCount++;
            return;
        }
        if (!_testEnded)
        {
            _testEnded = true;
            _slowFrameSpan.End();
            _noFramesSpan.End();
            _disableInSpanOptionsSpan.End();
        }
    }

}


