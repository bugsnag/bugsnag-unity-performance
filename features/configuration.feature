Feature: Configuration tests

  Background:
    Given I clear the Bugsnag cache

  Scenario: Custom Release Stage
    When I run the game in the "CustomReleaseStage" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.resource" string attribute "deployment.environment" equals "CustomReleaseStage"

  Scenario: Enabled Release Stage
    When I run the game in the "EnabledReleaseStages" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.resource" string attribute "deployment.environment" equals "EnabledReleaseStages"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "EnabledReleaseStages"

  Scenario: Disabled Release Stage
    When I run the game in the "DisabledReleaseStages" state
    Then I should receive no traces

  Scenario: Empty Release Stage
    When I run the game in the "EmptyReleaseStages" state
    Then I should receive no traces

  Scenario: Max Batch Size
    When I run the game in the "MaxBatchSize" state
    And I wait to receive a trace
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans" is an array with 3 elements

  Scenario: App Version
    When I run the game in the "AppVersion" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "AppVersion"
    * the trace payload field "resourceSpans.0.resource" string attribute "service.version" equals "1.2.3_AppVersion"

  @ios_only
  Scenario: Bundle Version
    When I run the game in the "BundleVersion" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "BundleVersion"
    * the trace payload field "resourceSpans.0.resource" string attribute "bugsnag.app.bundle_version" equals "1.2.3_BundleVersion"

  @android_only
  Scenario: Version Code
    When I run the game in the "VersionCode" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "VersionCode"
    * the trace payload field "resourceSpans.0.resource" string attribute "bugsnag.app.version_code" equals "123"