Feature: Trace and state persistence

  Background:
    Given I clear the Bugsnag cache

  @skip_webgl #Pending PLAT-8151
  Scenario: Receive a persisted trace
    When I set the HTTP status code to 408
    And I run the game in the "PersistTrace" state
    And I wait for 3 spans
    And the trace "Bugsnag-Span-Sampling" header equals "1:3"
    And I wait for requests to persist
    And I discard the oldest trace
    And I close the Unity app
    Then I set the HTTP status code to 200
    And I relaunch the app
    And I run the game in the "StartSDK" state
    And I wait for 3 spans
    And the trace "Bugsnag-Span-Sampling" header equals "1:3"
    And the trace payload field "resourceSpans.0.scopeSpans.0.spans" is an array with 3 elements

  @skip_webgl #Pending PLAT-8151
  Scenario: Max Batch Age
    When I set the HTTP status code to 408
    And I run the game in the "PersistTrace" state
    And I wait for 3 spans
    And the trace "Bugsnag-Span-Sampling" header equals "1:3"
    And I wait for requests to persist
    And I discard the oldest trace
    And I close the Unity app
    Then I set the HTTP status code to 200
    And I relaunch the app
    And I run the game in the "MaxBatchAge" state
    And I should receive no trace

  @skip_webgl #Pending PLAT-8151
  @skip_windows #Pending PLAT-10969
  Scenario: P value response 0.0
    Given I set the sampling probability for the next traces to "0"
    And I run the game in the "PValueUpdate" state
    And I wait to receive a sampling request
    Then I should receive no traces
    Then I close the Unity app
    # Block the next initial P value request so that it keeps its 0.0 P value
    And I set the HTTP status code for the next request to 404
    And I relaunch the app
    And I run the game in the "PValueUpdate" state
    Then I should receive no traces

  @skip_webgl #Pending PLAT-8151 
  @skip_macos #Pending PLAT-12011
  Scenario: P value response 0.0 then 1.0
    Given I set the sampling probability for the next traces to "0"
    And I run the game in the "PValueUpdate" state
    And I wait to receive a sampling request
    Then I should receive no traces
    Then I close the Unity app
    And I set the sampling probability for the next traces to "1"
    And I relaunch the app
    And I run the game in the "PValueUpdate" state
    And I wait for 1 span
    And the trace "Bugsnag-Span-Sampling" header equals "1:1"

  @skip_webgl #Pending PLAT-8151
  Scenario: P value response 1.0
    Given I set the sampling probability for the next traces to "1"
    And I run the game in the "PValueUpdate" state
    And I wait to receive a sampling request
    And I wait for 1 span
    And the trace "Bugsnag-Span-Sampling" header equals "1:1"
    And I wait for requests to persist
    And I discard the oldest trace
    Then I close the Unity app
    # Block the next initial P value request so that it keeps its 1.0 P value
    And I set the HTTP status code for the next request to 404
    And I relaunch the app
    And I run the game in the "PValueUpdate" state
    And I wait for 1 span
    And the trace "Bugsnag-Span-Sampling" header equals "1:1"

  @skip_webgl #Pending PLAT-8151
  Scenario: P value response 1.0 then 0.0
    Given I set the sampling probability for the next traces to "1"
    And I run the game in the "PValueUpdate" state
    And I wait to receive a sampling request
    And I wait for 1 span
    And the trace "Bugsnag-Span-Sampling" header equals "1:1"
    And I wait for requests to persist
    And I discard the oldest trace
    Then I close the Unity app
    And I set the sampling probability for the next traces to "0"
    And I relaunch the app
    And I run the game in the "PValueUpdate" state
    Then I should receive no traces

  @skip_webgl #Pending PLAT-8151
  Scenario: P value expiry
    Given I run the game in the "PValueExpiry" state
    And I wait to receive a sampling request
    # https://smartbear.atlassian.net/browse/PLAT-10274
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request

  @skip_webgl #Pending PLAT-8151
  Scenario: P value retry (success case)
    Given I run the game in the "PValueRetry" state
    And I wait to receive a sampling request
    And I discard the oldest sampling request
    And I should receive no sampling request

  @skip_webgl #Pending PLAT-8151
  Scenario: P value retry (error case)
    When I set the HTTP status code to 408
    And I run the game in the "PValueExpiry" state
    And I wait to receive a sampling request
    # https://smartbear.atlassian.net/browse/PLAT-10274
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request
    # And I discard the oldest sampling request
    # And I wait to receive a sampling request

  @skip_webgl #Pending PLAT-8151
  Scenario: P value response parsing
    Given I set the sampling probability for the next traces to "0.9999999"
    And I run the game in the "PValueUpdate" state
    And I wait to receive a sampling request
    And I wait for 1 span
    And the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.2.value.doubleValue" equals 0.9999999
