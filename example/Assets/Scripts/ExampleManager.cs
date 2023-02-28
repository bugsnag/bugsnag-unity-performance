using BugsnagUnityPerformance;
using UnityEngine;

public class ExampleManager : MonoBehaviour
{

    public string ApiKey;
    public string Endpoint;
    public string ReleaseStage;

    private Span _span;

    void Start()
    {
        var config = new PerformanceConfiguration(ApiKey);
        if (!string.IsNullOrEmpty(Endpoint))
        {
            config.Endpoint = Endpoint;
        }
        if (!string.IsNullOrEmpty(ReleaseStage))
        {
            config.ReleaseStage = ReleaseStage;
        }
        BugsnagPerformance.Start(config);
        _span = BugsnagPerformance.StartSpan("test");
        Invoke("EndSpan",1);
    }

    private void EndSpan()
    {
        _span.End();
    }
  
}
