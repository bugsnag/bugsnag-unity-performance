using System.Collections;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class RenderMetrics : Scenario
{
    bool _doTenSlowFrames;
    int _slowFramesDone;
    Span _slowFrameSpan, _noFramesSpan, _disableInSpanOptionsSpan;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(3);
        Configuration.EnabledMetrics.Rendering = true;
    }

    public override void Run()
    {
        base.Run();
        StartCoroutine(StartTest());
    }

    private IEnumerator StartTest()
    {
        yield return new WaitForSeconds(3);
        _slowFrameSpan = BugsnagPerformance.StartSpan("SlowFrames");
        _noFramesSpan = BugsnagPerformance.StartSpan("NoFrames", new SpanOptions { IsFirstClass = false });
        _disableInSpanOptionsSpan = BugsnagPerformance.StartSpan("DisableInSpanOptions", new SpanOptions { InstrumentRendering = false });
        yield return new WaitForSeconds(1);
        _doTenSlowFrames = true;
    }

    private IEnumerator EndTest()
    {
        yield return new WaitForSeconds(1);
        _slowFrameSpan.End();
        _noFramesSpan.End();
        _disableInSpanOptionsSpan.End();
    }

    void Update()
    {
        if (_doTenSlowFrames)
        {
            if (_slowFramesDone < 10)
            {
                float startTime = Time.realtimeSinceStartup;

                // Simulate a busy workload by blocking the main thread
                while (Time.realtimeSinceStartup < startTime + 0.25f)
                {
                    // Do nothing, just burn CPU cycles
                }
                _slowFramesDone++;
                return;
            }
            else
            {
                float startTime = Time.realtimeSinceStartup;

                // Simulate a busy workload by blocking the main thread
                while (Time.realtimeSinceStartup < startTime + 5)
                {
                    // Do nothing, just burn CPU cycles
                }
                // Thread.Sleep(1000); // frozen frame
                _doTenSlowFrames = false;
                StartCoroutine(EndTest());
            }
        }
    }

}


