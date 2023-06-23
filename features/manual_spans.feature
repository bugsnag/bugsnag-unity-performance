Feature: Manual creation of spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: Manual spans can be logged
    When I run the game in the "ManualSpan" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "ManualSpan"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.spanId" matches the regex "^[A-Fa-f0-9]{16}$"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.traceId" matches the regex "^[A-Fa-f0-9]{32}$"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.kind" equals 1
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.startTimeUnixNano" matches the regex "^[0-9]+$"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.endTimeUnixNano" matches the regex "^[0-9]+$"    
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "custom"


    #Resource attributes
    * the trace payload field "resourceSpans.0.resource" string attribute "deployment.environment" equals "production"
    * the trace payload field "resourceSpans.0.resource" string attribute "telemetry.sdk.name" equals "bugsnag.performance.unity"
    * the trace payload field "resourceSpans.0.resource" string attribute "telemetry.sdk.version" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "os.version" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "device.id" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "device.model.identifier" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "service.version" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "bugsnag.app.platform" is one of:
      | Android   |
      | iOS |
    * the trace payload field "resourceSpans.0.resource" string attribute "device.manufacturer" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "host.arch" exists
    * the trace payload field "resourceSpans.0.resource" string attribute "bugsnag.runtime_versions.unity" exists

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true


  @android_only
  Scenario: Android Specific Resource Attributes
    When I run the game in the "ManualSpan" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"  

    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.resource" string attribute "device.version_code" exists

 @ios_only
  Scenario: iOS Specific Resource Attributes
    When I run the game in the "ManualSpan" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"  

    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.resource" string attribute "device.bundle_version" exists
