Feature: Rendering Metrics

  Background:
    Given I clear the Bugsnag cache

  Scenario: Frame Rate Metrics
    When I run the game in the "RenderMetrics" state
    And I wait for 2 spans
    
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "SlowFrames"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 7 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.framerate.frozen_frames" equals 1
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.framerate.slow_frames" is greater than 10
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.framerate.total_frames" is greater than 100

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "NoFrames"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.attributes" is an array with 4 elements



  Scenario: Disable Frame Rate Metrics
    When I run the game in the "ConfigureRenderMetrics" state
    And I wait for 1 span
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "BeforeStart"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "custom"









