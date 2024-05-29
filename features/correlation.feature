Feature: Manual creation of spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: Simple Error Correlation
    When I run the game in the "SimpleErrorCorrelation" state
    * I wait to receive an error
    * I wait for 1 span
    * the exception "message" equals "Simple Error Correlation"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.spanId" is stored as the value "context_span_id"
    * the error payload field "events.0.correlation.spanid" equals the stored value "context_span_id"

  Scenario: Correlation Should be null
    When I run the game in the "CorrelationShouldBeNull" state
    * I wait to receive an error
    * I wait for 1 span
    * the exception "message" equals "Correlation Should Be Null"
    * the error payload field "events.0.correlation.spanid" is null

  Scenario: Correlation On Different Thread
    When I run the game in the "CorrelationOnDifferentThread" state
    * I wait to receive 2 errors
    * I wait for 2 spans

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "Span From Background Thread"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.spanId" is stored as the value "background_span_id"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "Span From Main Thread"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.spanId" is stored as the value "main_span_id"

    * the exception "message" equals "Event From Main Thread"
    * the error payload field "events.0.correlation.spanid" equals the stored value "main_span_id"

    And I discard the oldest error

    * the exception "message" equals "Event From Background Thread"
    * the error payload field "events.0.correlation.spanid" equals the stored value "background_span_id"
