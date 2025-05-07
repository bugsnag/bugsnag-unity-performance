Feature: Memory Metrics

  Background:
    Given I clear the Bugsnag cache

  @android_only
  Scenario: Android Memory Metrics
    When I run the game in the "MemoryMetrics" state
    And I wait for 1 span

  # Basic checks
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "MemoryMetrics"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 12 elements
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.key" equals "bugsnag.system.memory.timestamps"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.value.arrayValue.values" is an array with 5 elements

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.device.physical_device_memory" is greater than 0

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.6.key" equals "bugsnag.system.memory.spaces.device.size"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.device.size" is greater than 0

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.7.key" equals "bugsnag.system.memory.spaces.device.used"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.7.value.arrayValue.values" is an array with 5 elements

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.8.key" equals "bugsnag.system.memory.spaces.device.mean"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.device.mean" is greater than 0

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.9.key" equals "bugsnag.system.memory.spaces.art.size"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.art.size" is greater than 0

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.10.key" equals "bugsnag.system.memory.spaces.art.used"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.10.value.arrayValue.values" is an array with 5 elements

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.11.key" equals "bugsnag.system.memory.spaces.art.mean"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.art.mean" is greater than 0

  @ios_only
  Scenario: iOS Memory Metrics
    When I run the game in the "MemoryMetrics" state
    And I wait for 1 span

  # Basic checks
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "MemoryMetrics"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 9 elements
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.key" equals "bugsnag.system.memory.timestamps"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.value.arrayValue.values" is an array with 5 elements

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.device.physical_device_memory" is greater than 0

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.6.key" equals "bugsnag.system.memory.spaces.device.size"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.device.size" is greater than 0

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.7.key" equals "bugsnag.system.memory.spaces.device.used"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.7.value.arrayValue.values" is an array with 5 elements

  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.8.key" equals "bugsnag.system.memory.spaces.device.mean"
  * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.device.mean" is greater than 0


  Scenario: Disable Memory Metrics
    When I run the game in the "ConfigureMemoryMetrics" state
    And I wait for 1 span
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "BeforeStart"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "custom"