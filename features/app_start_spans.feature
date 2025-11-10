Feature: App Start

  Background:
    Given I clear the Bugsnag cache

  Scenario: Full App Start
    When I run the game in the "AppStartFull" state
    And I wait to receive at least 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.app_start.type" equals "UnityRuntime"

    * the span named "[AppStartPhase/LoadAssemblies]" exists
    * the span named "[AppStartPhase/SplashScreen]" exists
    * the span named "[AppStartPhase/LoadFirstScene]" exists
    * the span named "[AppStart/UnityRuntime]" exists

    * the span named "[AppStart/UnityRuntime]" has no parent
    * the span named "[AppStart/UnityRuntime]" is the parent of the span named "[AppStartPhase/LoadAssemblies]"
    * the span named "[AppStart/UnityRuntime]" is the parent of the span named "[AppStartPhase/SplashScreen]"
    * the span named "[AppStartPhase/SplashScreen]" is the parent of the span named "[AppStartPhase/LoadFirstScene]"
    * the span named "[AppStart/UnityRuntime]" has a maximum duration of 3000000000

    * the span named "[AppStart/UnityRuntime]" is first class
    * the span named "[AppStartPhase/LoadFirstScene]" is not first class
    * the span named "[AppStartPhase/SplashScreen]" is not first class
    * the span named "[AppStartPhase/LoadAssemblies]" is not first class

Scenario: App Start Start Only
    When I run the game in the "AppStartStartOnly" state
    And I wait to receive at least 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Sent-At" header matches the regex "^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d\d\dZ$"
    * the trace "Bugsnag-Span-Sampling" header equals "1:4"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "[AppStartPhase/LoadAssemblies]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.name" equals "[AppStartPhase/SplashScreen]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.name" equals "[AppStartPhase/LoadFirstScene]"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.name" equals "[AppStart/UnityRuntime]"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.spanId" is stored as the value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.spanId" is stored as the value "splash_span_id"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.parentSpanId" equals the stored value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.1.parentSpanId" equals the stored value "root_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.2.parentSpanId" equals the stored value "splash_span_id"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.parentSpanId" is null

    * the span named "[AppStart/UnityRuntime]" has a minimum duration of 8000000000

Scenario: App Start Off
    When I run the game in the "AppStartOff" state
    And I wait to receive at least 1 span
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Span-Sampling" header equals "1:1"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "AppStartOff"

Scenario: App Start Customisation
    When I run the game in the "AppStartCustomisation" state
    And I wait to receive at least 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Span-Sampling" header equals "1:4"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.name" equals "[AppStart/UnityRuntime]ColdStart"

Scenario: App Start Clear Customisation
    When I run the game in the "AppStartClearCustomisation" state
    And I wait to receive at least 4 spans
    Then the trace Bugsnag-Integrity header is valid
    And the trace "Bugsnag-Api-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
    * the trace "Bugsnag-Span-Sampling" header equals "1:4"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.3.name" equals "[AppStart/UnityRuntime]"

@mobile_only
Scenario: App Start with metrics
  When I run the game in the "AppStartWithMetrics" state
  And I wait to receive at least 4 spans

  # basic structure – same as the other app start tests
  * the span named "[AppStartPhase/LoadAssemblies]" exists
  * the span named "[AppStartPhase/SplashScreen]" exists
  * the span named "[AppStartPhase/LoadFirstScene]" exists
  * the span named "[AppStart/UnityRuntime]" exists

  # root app-start metadata
  * the span named "[AppStart/UnityRuntime]" has string attribute "bugsnag.span.category" equal to "app_start"
  * the span named "[AppStart/UnityRuntime]" has string attribute "bugsnag.app_start.type" equal to "UnityRuntime"
  * the span named "[AppStart/UnityRuntime]" has boolean attribute "bugsnag.span.first_class" equal to true

  # rendering metrics on the app-start span
  * the span named "[AppStart/UnityRuntime]" has integer attribute "bugsnag.rendering.total_frames" greater than 0
  * the span named "[AppStart/UnityRuntime]" has integer attribute "bugsnag.rendering.fps_average" greater than 0
  * the span named "[AppStart/UnityRuntime]" has integer attribute "bugsnag.rendering.fps_maximum" greater than 0
  * the span named "[AppStart/UnityRuntime]" has integer attribute "bugsnag.rendering.fps_minimum" greater than 0

  # memory metrics on the app-start span (arrayed samples)
  * the span named "[AppStart/UnityRuntime]" has array attribute "bugsnag.system.memory.timestamps" with at least 5 elements
  * the span named "[AppStart/UnityRuntime]" has integer attribute "bugsnag.device.physical_device_memory" greater than 0
  * the span named "[AppStart/UnityRuntime]" has integer attribute "bugsnag.system.memory.spaces.device.size" greater than 0
  * the span named "[AppStart/UnityRuntime]" has array attribute "bugsnag.system.memory.spaces.device.used" with at least 5 elements
  * the span named "[AppStart/UnityRuntime]" has integer attribute "bugsnag.system.memory.spaces.art.size" greater than 0
  * the span named "[AppStart/UnityRuntime]" has array attribute "bugsnag.system.memory.spaces.art.used" with at least 5 elements

  # CPU metrics on the app-start span
  * the span named "[AppStart/UnityRuntime]" has array attribute "bugsnag.system.cpu_measures_timestamps" with at least 5 elements
  * the span named "[AppStart/UnityRuntime]" has array attribute "bugsnag.system.cpu_measures_total" with at least 5 elements
  * the span named "[AppStart/UnityRuntime]" has double array attribute "bugsnag.system.cpu_measures_total" containing valid percentages
  * the span named "[AppStart/UnityRuntime]" has array attribute "bugsnag.system.cpu_measures_main_thread" with at least 5 elements
  * the span named "[AppStart/UnityRuntime]" has double array attribute "bugsnag.system.cpu_measures_main_thread" containing valid percentages
  * the span named "[AppStart/UnityRuntime]" has double attribute "bugsnag.system.cpu_mean_total" that is a valid percentage
  * the span named "[AppStart/UnityRuntime]" has double attribute "bugsnag.system.cpu_mean_main_thread" that is a valid percentage

  # phase spans should also carry the memory metrics
  * the span named "[AppStartPhase/LoadAssemblies]" has integer attribute "bugsnag.device.physical_device_memory" greater than 0
  * the span named "[AppStartPhase/SplashScreen]" has integer attribute "bugsnag.device.physical_device_memory" greater than 0
  * the span named "[AppStartPhase/LoadFirstScene]" has integer attribute "bugsnag.device.physical_device_memory" greater than 0
