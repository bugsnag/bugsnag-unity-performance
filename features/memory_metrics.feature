Feature: Memory Metrics

  Background:
    Given I clear the Bugsnag cache

  Scenario: Memory Metrics
    When I run the game in the "MemoryMetrics" state
    And I wait for 1 span

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "MemoryMetrics"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 10 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.3.key" equals "bugsnag.system.memory.timestamps"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.3.value.arrayValue.values" is an array with 5 elements

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.device.physical_device_memory" is greater than 0

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.5.key" equals "bugsnag.system.memory.spaces.space_names"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.5.value.arrayValue.values" is an array with 1 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.5.value.arrayValue.values.0.stringValue" equals "device"

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.device.size" is greater than 0

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.7.key" equals "bugsnag.system.memory.spaces.device.used"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.7.value.arrayValue.values" is an array with 5 elements

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" integer attribute "bugsnag.system.memory.spaces.device.mean" is greater than 0


  Scenario: Disable Memory Metrics
    When I run the game in the "ConfigureMemoryMetrics" state
    And I wait for 1 span
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "BeforeStart"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "custom"