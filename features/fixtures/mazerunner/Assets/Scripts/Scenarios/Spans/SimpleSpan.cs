using BugsnagUnityPerformance;

public class SimpleSpan : Scenario
{

    private Span _span;

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartSpan("SimpleSpan");
        Invoke("EndSpan",1);
    }

    private void EndSpan()
    {
        _span.End();
    }

}
