Feature: Configuration tests

  Background:
    Given I clear the Bugsnag cache

  Scenario: Custom Release Stage
    When I run the game in the "CustomReleaseStage" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace payload field "resourceSpans.0.resource" string attribute "deployment.environment" equals "CustomReleaseStage"

  Scenario: Max Batch Size
    When I run the game in the "MaxBatchSize" state
    And I wait to receive a trace
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans" is an array with 3 elements
