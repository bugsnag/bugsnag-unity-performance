Feature: Scene Load Spans

  Background:
    Given I clear the Bugsnag cache

  @skip_webgl #Pending PLAT-12103
  Scenario: Load Scene By Name
    When I run the game in the "SceneLoadByName" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[ViewLoad/UnityScene]Scene1"

    * the span named "[ViewLoad/UnityScene]Scene1" is first class

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "view_load"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.type" equals "UnityScene"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.name" equals "Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true

  @skip_webgl #Pending PLAT-12103
  Scenario: Load Scene By Index
    When I run the game in the "SceneLoadByIndex" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[ViewLoad/UnityScene]Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "view_load"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.type" equals "UnityScene"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.name" equals "Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true

  @skip_webgl #Pending PLAT-12103
  Scenario: Load Scene Async
    When I run the game in the "SceneLoadAsync" state
    And I wait for 3 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:3"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[ViewLoad/UnityScene]Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "view_load"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.type" equals "UnityScene"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.name" equals "Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1" string attribute "bugsnag.view.name" equals "Scene2"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2" string attribute "bugsnag.view.name" equals "Scene3"

  @skip_webgl #Pending PLAT-12103
  Scenario: Manual Scene Span
    When I run the game in the "ManualSceneSpan" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[ViewLoad/UnityScene]ManualSceneSpan"
    * the span named "[ViewLoad/UnityScene]ManualSceneSpan" is first class
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "view_load"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.type" equals "UnityScene"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.name" equals "ManualSceneSpan"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true

