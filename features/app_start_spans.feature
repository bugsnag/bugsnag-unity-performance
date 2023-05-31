Feature: App Start

  Background:
    Given I clear the Bugsnag cache

  Scenario: Full App Start
    When I run the game in the "AppStartFull" state
    And I wait for 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

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


Scenario: App Start Start Only
    When I run the game in the "AppStartStartOnly" state
    And I wait for 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

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

Scenario: App Start Off
    When I run the game in the "AppStartOff" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "AppStartOff"

