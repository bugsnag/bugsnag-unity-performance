Feature: Sampling spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: Setting fixed sampling probability of 1 with dynamic probability of 0 should send all spans
    Given I set the sampling probability for the next traces to "0"
    And I enter unmanaged traces mode
    When I run the game in the "OverrideSampling1" state
    And I wait for 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    And the trace "Bugsnag-Span-Sampling" header is not present
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * a span name equals "ManualSpan1"
    * a span name equals "ManualSpan2"
    * a span name equals "ManualSpan3"
    * a span name equals "ManualSpan4"
    * every span field "spanId" matches the regex "^[A-Fa-f0-9]{16}$"
    * every span field "traceId" matches the regex "^[A-Fa-f0-9]{32}$"
    * every span field "startTimeUnixNano" matches the regex "^[0-9]+$"
    * every span field "endTimeUnixNano" matches the regex "^[0-9]+$"    

  Scenario: Setting fixed sampling probability of 0 with dynamic probability of 1 should send no spans
    Given I set the sampling probability for the next traces to "1"
    When I run the game in the "OverrideSampling0" state
    Then I should receive no traces

  Scenario: With dynamic probability of 1 and no fixed probability, all spans are sampled
    Given I set the sampling probability for the next traces to "1"
    When I run the game in the "ConfiguredSamplingRate1" state
    And I wait for 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    And the trace "Bugsnag-Span-Sampling" header equals "1:4"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * a span name equals "ManualSpan1"
    * a span name equals "ManualSpan2"
    * a span name equals "ManualSpan3"
    * a span name equals "ManualSpan4"
    * every span field "spanId" matches the regex "^[A-Fa-f0-9]{16}$"
    * every span field "traceId" matches the regex "^[A-Fa-f0-9]{32}$"
    * every span field "startTimeUnixNano" matches the regex "^[0-9]+$"
    * every span field "endTimeUnixNano" matches the regex "^[0-9]+$"    

  Scenario: With dynamic probability of 0 and no fixed probability, no spans are sampled
    Given I set the sampling probability for the next traces to "0"
    When I run the game in the "ConfiguredSamplingRate0" state
    Then I should receive no traces
