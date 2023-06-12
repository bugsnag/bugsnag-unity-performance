Feature: Sampling spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: With probability 1, all spans are sampled
    Given I set the sampling probability for the next traces to "1"
    When I run the game in the "ConfiguredSamplingRate1" state
    And I wait for 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    And the trace "Bugsnag-Span-Sampling" header equals "1:4"
    * a span name equals "ManualSpan1"
    * a span name equals "ManualSpan2"
    * a span name equals "ManualSpan3"
    * a span name equals "ManualSpan4"
    * every span field "spanId" matches the regex "^[A-Fa-f0-9]{16}$"
    * every span field "traceId" matches the regex "^[A-Fa-f0-9]{32}$"
    * every span field "startTimeUnixNano" matches the regex "^[0-9]+$"
    * every span field "endTimeUnixNano" matches the regex "^[0-9]+$"    

  Scenario: With probability 0, no spans are sampled
    Given I set the sampling probability for the next traces to "0"
    When I run the game in the "ConfiguredSamplingRate0" state
    Then I should receive no traces
