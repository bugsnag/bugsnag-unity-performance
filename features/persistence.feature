Feature: Trace persistence

  Background:
    Given I clear the Bugsnag cache

  Scenario: Receive a persisted trace
    When I set the HTTP status code for the next requests to "408"
    And I run the game in the "PersistTrace" state
    And I wait to receive a trace
    And I wait for requests to persist
    And I discard the oldest trace
    And I close the Unity app
    And I relaunch the app
    And I run the game in the "StartSDK" state
    And I wait to receive a trace
    And the trace payload field "resourceSpans.0.scopeSpans.0.spans" is an array with 3 elements

  Scenario: Max Batch Age
    When I set the HTTP status code for the next requests to "408"
    And I run the game in the "PersistTrace" state
    And I wait to receive a trace
    And I wait for requests to persist
    And I discard the oldest trace
    And I close the Unity app
    And I relaunch the app
    And I run the game in the "MaxBatchAge" state
    And I should receive no trace
