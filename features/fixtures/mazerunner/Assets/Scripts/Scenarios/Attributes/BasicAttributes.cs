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
        span.AddAttribute("string", "value1");
        span.AddAttribute("string", "value2"); // This should overwrite the previous value

        span.AddAttribute("string array", new string[] { "value1", "value2" });
        span.AddAttribute("string array", new string[] { "value3", "value4" });
        
        span.AddAttribute("int", 1);
        span.AddAttribute("int", 2); 

        span.AddAttribute("int array", new int[] { 1, 2 });
        span.AddAttribute("int array", new int[] { 3, 4 });

        span.AddAttribute("double", 1.0);
        span.AddAttribute("double", 2.0);

        span.AddAttribute("double array", new double[] { 1.0, 2.0 });
        span.AddAttribute("double array", new double[] { 3.0, 4.0 });

        span.AddAttribute("bool", true);
        span.AddAttribute("bool", false);

        span.AddAttribute("bool array", new bool[] { true, false });
        span.AddAttribute("bool array", new bool[] { false, true });

        span.End();
    }

}
