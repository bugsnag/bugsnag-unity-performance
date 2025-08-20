Feature: Test Dev Behaviour

  Background:
    Given I clear the Bugsnag cache

  Scenario: Test Dev Behaviour
    When I run the game in the "Dev" state
    And I wait to receive 2 traces
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "Span-1"
    * I discard the oldest trace
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "Span-2"

    #Resource attributes
    * the trace payload field "resourceSpans.0.resource" string attribute "deployment.environment" equals "development"
    * the trace payload field "resourceSpans.0.resource" string attribute "telemetry.sdk.name" equals "bugsnag.performance.unity"
    * the trace payload field "resourceSpans.0.resource" string attribute "telemetry.sdk.version" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "os.version" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "device.id" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "device.model.identifier" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "service.version" equals "1.0"
    * the trace payload field "resourceSpans.0.resource" integer attribute "device.screen_resolution.width" is greater than 100
    * the trace payload field "resourceSpans.0.resource" integer attribute "device.screen_resolution.height" is greater than 100
    * the trace payload field "resourceSpans.0.resource" string attribute "service.name" is one of:
      | com.bugsnag.fixtures.unity.performance.android |
      | com.bugsnag.fixtures.unity.performance.ios |
      | com.bugsnag.mazerunner |
      | mazerunner |
      | unknown_service |
    * the trace payload field "resourceSpans.0.resource" string attribute "bugsnag.app.platform" is one of:
      | Android |
      | iOS |
      | MacOS |
      | WebGL |
      | Windows |
    * the trace payload field "resourceSpans.0.resource" string attribute "bugsnag.runtime_versions.unity" exists
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true
