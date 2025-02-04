using System.Collections;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class RenderMetrics : Scenario
{
    bool _testStarted, _testEnded, _testInitialised;
    int _slowFramesDone, _frozenFramesDone;
    Span _slowFrameSpan, _noFramesSpan, _disableInSpanOptionsSpan;

    const int NUM_SLOW_FRAMES_TO_DO = 10;
    const int NUM_FROZEN_FRAMES_TO_DO = 2;

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
        Application.targetFrameRate = 60;
        StartCoroutine(StartTest());
    }

    private IEnumerator StartTest()
    {
        yield return new WaitForSeconds(3); // wait for the app to settle and frame rate to stabilise
        _testStarted = true;
    }



    void Update()
    {
        if (!_testStarted || _testEnded)
        {
            return;
        }
        if(!_testInitialised)
        {
            _testInitialised = true;
             _slowFrameSpan = BugsnagPerformance.StartSpan("SlowFrames");
            _noFramesSpan = BugsnagPerformance.StartSpan("NoFrames", new SpanOptions { IsFirstClass = false });
            _disableInSpanOptionsSpan = BugsnagPerformance.StartSpan("DisableInSpanOptions", new SpanOptions { InstrumentRendering = false });
        }
        if (_slowFramesDone < NUM_SLOW_FRAMES_TO_DO)
        {
            float startTime = Time.realtimeSinceStartup;
            // Simulate a busy workload by blocking the main thread
            while (Time.realtimeSinceStartup < startTime + 0.25f){}
            _slowFramesDone++;
            return;
        }
        if (_frozenFramesDone < NUM_FROZEN_FRAMES_TO_DO)
        {
            var startTime2 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < startTime2 + 2){}
            _frozenFramesDone++;
            return;
        }
        if (_frameCount < (100 - NUM_SLOW_FRAMES_TO_DO - NUM_FROZEN_FRAMES_TO_DO))
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


