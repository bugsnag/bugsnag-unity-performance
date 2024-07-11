using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class BasicAttributes : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        var span = BugsnagPerformance.StartSpan("BasicAttributes");
        span.SetAttribute("string", "value1");
        span.SetAttribute("string", "value2"); // This should overwrite the previous value

        span.SetAttribute("string array", new string[] { "value1", "value2" });
        span.SetAttribute("string array", new string[] { "value3", "value4" });
        
        span.SetAttribute("int", 1);
        span.SetAttribute("int", 2); 

        span.SetAttribute("int array", new int[] { 1, 2 });
        span.SetAttribute("int array", new int[] { 3, 4 });

        span.SetAttribute("double", 1.0);
        span.SetAttribute("double", 2.0);

        span.SetAttribute("double array", new double[] { 1.0, 2.0 });
        span.SetAttribute("double array", new double[] { 3.0, 4.0 });

        span.SetAttribute("bool", true);
        span.SetAttribute("bool", false);

        span.SetAttribute("bool array", new bool[] { true, false });
        span.SetAttribute("bool array", new bool[] { false, true });

        span.End();
    }

}
