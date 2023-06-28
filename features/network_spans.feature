Feature: Network Spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: Get Success
    When I run the game in the "NetworkGetSuccess" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "HTTP/GET"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "network"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "net.host.connection.type" equals "wifi"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.method" equals "GET"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.url" matches the regex "^http:\/\/\S*:\d{4}\/"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.status_code" equals "200"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "http.response_content_length" is greater than 0

  # This test sends 2 requests, 1 fails and one succeeds, so we should only get 1
  Scenario: Get Fail
    When I run the game in the "NetworkGetFail" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "HTTP/GET"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "network"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "net.host.connection.type" equals "wifi"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.method" equals "GET"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.url" matches the regex "^http:\/\/\S*:\d{4}\/"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.status_code" equals "200"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "http.response_content_length" is greater than 0

  Scenario: Post Success
    When I run the game in the "NetworkPostSuccess" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "HTTP/POST"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "network"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "net.host.connection.type" equals "wifi"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.method" equals "POST"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.url" matches the regex "^http:\/\/\S*:\d{4}\/"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.status_code" equals "200"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "http.response_content_length" is greater than 0

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "http.response_content_length" is greater than 0

  # This test sends 2 requests, 1 fails and one succeeds, so we should only get 1
  Scenario: Post Fail
    When I run the game in the "NetworkPostFail" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "HTTP/POST"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "network"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "net.host.connection.type" equals "wifi"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.method" equals "POST"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.url" matches the regex "^http:\/\/\S*:\d{4}\/"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "http.status_code" equals "200"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "http.response_content_length" is greater than 0

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "http.response_content_length" is greater than 0