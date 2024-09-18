using System.Collections;
using BugsnagUnityPerformance;

public class ConfiguredSamplingRate0 : Scenario
{

    private Span _span;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(0);
    }

    public override void Run()
    {
       StartCoroutine(DoSpans());
    }

    private IEnumerator DoSpans(){
        yield return new UnityEngine.WaitForSeconds(4);
        BugsnagPerformance.StartSpan("ManualSpan1").End();
        BugsnagPerformance.StartSpan("ManualSpan2").End();
        BugsnagPerformance.StartSpan("ManualSpan3").End();
        BugsnagPerformance.StartSpan("ManualSpan4").End();
    }

}
