Feature: Rendering Metrics

  Background:
    Given I clear the Bugsnag cache

  Scenario: Frame Rate Metrics
    When I run the game in the "RenderMetrics" state
    And I wait for 4 spans

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "FrozenFrame"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is false
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "frozen_frame"
    * the span named "SlowFrames" is the parent of the span named "FrozenFrame"
    * the span named "FrozenFrame" starts and ends before the span named "SlowFrames" ends and lasts at least 1 second

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "SlowFrames"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.attributes" is an array with 7 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1" integer attribute "bugsnag.rendering.frozen_frames" equals 1
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1" integer attribute "bugsnag.rendering.slow_frames" is greater than 9
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1" integer attribute "bugsnag.rendering.total_frames" equals 100

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.name" equals "NoFrames"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2" boolean attribute "bugsnag.span.first_class" is false
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2" string attribute "bugsnag.span.category" equals "custom"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.name" equals "DisableInSpanOptions"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3" boolean attribute "bugsnag.span.first_class" is true
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3" string attribute "bugsnag.span.category" equals "custom"



  Scenario: Disable Frame Rate Metrics
    When I run the game in the "ConfigureRenderMetrics" state
    And I wait for 1 span
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "BeforeStart"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "custom"