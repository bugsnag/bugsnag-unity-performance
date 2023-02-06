using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class ManualSpan : Scenario
{

    private Span _span;

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartSpan("ManualSpan");
        Invoke("EndSpan",1);
    }

    private void EndSpan()
    {
        _span.End();
    }
}
