using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class BugsnagPerformanceUtil
{
    private static readonly DateTimeOffset _unixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static long GetNanoSeconds(DateTimeOffset time)
    {
        var duration = time - _unixStart;
        return duration.Ticks * 100; // 1 tick = 100 nanoseconds
    }
}
