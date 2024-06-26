﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppStartFull : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchAgeSeconds(5);
        Configuration.AutoInstrumentAppStart = AutoInstrumentAppStartSetting.FULL;
    }
}
