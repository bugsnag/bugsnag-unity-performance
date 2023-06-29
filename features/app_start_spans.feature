Feature: App Start

  Background:
    Given I clear the Bugsnag cache

  Scenario: Full App Start
    When I run the game in the "AppStartFull" state
    And I wait for 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:4"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.app_start.type" equals "UnityRuntime"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[AppStartPhase/LoadAssemblies]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "[AppStartPhase/SplashScreen]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.name" equals "[AppStartPhase/LoadFirstScene]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.name" equals "[AppStart/UnityRuntime]"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.spanId" is stored as the value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.spanId" is stored as the value "splash_span_id"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.parentSpanId" equals the stored value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.parentSpanId" equals the stored value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.parentSpanId" equals the stored value "splash_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.parentSpanId" is null

    * the span named "[AppStart/UnityRuntime]" has a maximum duration of 3000000000

Scenario: App Start Start Only
    When I run the game in the "AppStartStartOnly" state
    And I wait for 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:4"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[AppStartPhase/LoadAssemblies]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "[AppStartPhase/SplashScreen]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.name" equals "[AppStartPhase/LoadFirstScene]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.name" equals "[AppStart/UnityRuntime]"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.spanId" is stored as the value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.spanId" is stored as the value "splash_span_id"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.parentSpanId" equals the stored value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.parentSpanId" equals the stored value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.parentSpanId" equals the stored value "splash_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.parentSpanId" is null

    * the span named "[AppStart/UnityRuntime]" has a minimum duration of 9000000000

Scenario: App Start Off
    When I run the game in the "AppStartOff" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "AppStartOff"

