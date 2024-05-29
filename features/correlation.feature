Feature: Manual creation of spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: Simple Error Correlation
    When I run the game in the "SimpleErrorCorrelation" state
    * I wait to receive an error
    * I wait for 1 span

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.spanId" is stored as the value "context_span_id"
    * the error payload field "events.0.correlation.spanid" equals the stored value "context_span_id"