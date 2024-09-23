using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class CustomAttributeLimits : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.AttributeStringValueLimit = 10;
        Configuration.AttributeArrayLengthLimit = 2;
        Configuration.AttributeCountLimit = 8;
    }

    public override void Run()
    {
        base.Run();
        var span = BugsnagPerformance.StartSpan("CustomAttributeLimits");

        var tooLongKey = string.Empty;
        for (int i = 0; i < 129; i++)
        {
            tooLongKey += "a";
        }
        span.SetAttribute(tooLongKey, "1234");
        span.SetAttribute("control", "1234");

        var tooLongString = string.Empty;
        for (int i = 0; i < 1025; i++)
        {
            tooLongString += "a";
        }
        span.SetAttribute("too long string", tooLongString);
        
        var tooLongArray = new string[20000];
        for (int i = 0; i < 20000; i++)
        {
            tooLongArray[i] = "a";
        }
        tooLongArray[0] = tooLongString;
        span.SetAttribute("tooLongArray", tooLongArray);

        for (int i = 0; i < 200; i++)
        {
            span.SetAttribute("int" + i, i);
        }
        span.SetAttribute("int1", 999);
       
        span.End();
    }

}
