Feature: Scene Load Spans

  Background:
    Given I clear the Bugsnag cache

  Scenario: Load Scene By Name
    When I run the game in the "SceneLoadByName" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[ViewLoad/Scene]Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span_category" equals "view_load"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.type" equals "scene"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.name" equals "Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true



  Scenario: Load Scene By Index
    When I run the game in the "SceneLoadByIndex" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[ViewLoad/Scene]Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span_category" equals "view_load"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.type" equals "scene"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.name" equals "Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true


Scenario: Load Scene Async
    When I run the game in the "SceneLoadAsync" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[ViewLoad/Scene]Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span_category" equals "view_load"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" bool attribute "bugsnag.span.first_class" is true

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.type" equals "scene"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.view.name" equals "Scene1"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1" string attribute "bugsnag.view.name" equals "Scene2"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2" string attribute "bugsnag.view.name" equals "Scene3"


