Feature: Nested Spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: Simple nested span
    When I run the game in the "BasicNestedSpan" state
    And I wait for 2 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:2"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "span1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "span2"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.spanId" is stored as the value "parent_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.parentSpanId" is null
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.parentSpanId" equals the stored value "parent_span_id"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.traceId" is stored as the value "parent_trace_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.traceId" equals the stored value "parent_trace_id"

  @skip_webgl # threaded code not supported in webgl
  Scenario: Pass Span Context
    When I run the game in the "PassSpanContext" state
    And I wait for 2 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:2"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "span1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "span2"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.spanId" is stored as the value "parent_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.parentSpanId" is null
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.parentSpanId" equals the stored value "parent_span_id"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.traceId" is stored as the value "parent_trace_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.traceId" equals the stored value "parent_trace_id"

  @skip_webgl # threaded code not supported in webgl
  Scenario: New Thread New Context
    When I run the game in the "NewThreadNewContext" state
    And I wait for 3 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:3"

    * the span named "span1" exists
    * the span named "span2" exists
    * the span named "span3" exists

    * the span named "span2" is the parent of the span named "span3"

    * the span named "span1" has no parent

    * the span named "span2" has no parent

  Scenario: Make Current Context
    When I run the game in the "MakeCurrentContext" state
    And I wait for 2 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:2"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.parentSpanId" is null
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.parentSpanId" is null

  Scenario: IsFirstClass
    When I run the game in the "IsFirstClass" state
    And I wait for 2 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:3"
   
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "FirstClass not set"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true
   
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "FirstClass true"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1" bool attribute "bugsnag.span.first_class" is true
   
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.name" equals "FirstClass false"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2" bool attribute "bugsnag.span.first_class" is false


  Scenario: Pass Context With Make Context False
    When I run the game in the "PassContextWithMakeContextFalse" state
    And I wait for 3 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:3"

    * the span named "span1" exists
    * the span named "span2" exists
    * the span named "span3" exists

    * the span named "span1" has no parent

    * the span named "span1" is the parent of the span named "span2"

    * the span named "span3" has no parent

