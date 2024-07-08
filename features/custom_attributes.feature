Feature: Custom Attributes

  Background:
    Given I clear the Bugsnag cache

  Scenario: Basic Custom Attributes
    When I run the game in the "BasicAttributes" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "BasicAttributes"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "string" equals "value2"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "int" equals 2
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double attribute "double" equals 2.0
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bool" is false

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string array attribute "string array" equals the array:
      | value3 |
      | value4 |

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer array attribute "int array" equals the array:
      | 3 |
      | 4 |

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double array attribute "double array" equals the array:
      | 3.0 |
      | 4.0 |

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean array attribute "bool array" equals the array:
      | false |
      | true  |

  Scenario: Custom Attributes In Callbacks
    When I run the game in the "AddAttributesInCallbacks" state
    And I wait for 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "AddAttributesInCallbacks"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "config-callback" is true
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "after-start-callback" is true
