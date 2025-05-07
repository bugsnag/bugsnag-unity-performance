Feature: CPU Metrics

  Background:
    Given I clear the Bugsnag cache

  @android_only
  Scenario: Android CPU Metrics
    When I run the game in the "CpuMetrics" state
    And I wait for 1 span

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "CpuMetrics"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 9 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.key" equals "bugsnag.system.cpu_measures_timestamps"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.value.arrayValue.values" is an array with 5 elements

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.5.key" equals "bugsnag.system.cpu_measures_total"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.5.value.arrayValue.values" is an array with 5 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double array attribute "bugsnag.system.cpu_measures_total" contains valid percentages

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.6.key" equals "bugsnag.system.cpu_measures_main_thread"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.6.value.arrayValue.values" is an array with 5 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double array attribute "bugsnag.system.cpu_measures_main_thread" contains valid percentages

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double attribute "bugsnag.metrics.cpu_mean_total" is a valid percentage
    
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double attribute "bugsnag.system.cpu_mean_main_thread" is a valid percentage

  @ios_only
  Scenario: iOS CPU Metrics
    When I run the game in the "CpuMetrics" state
    And I wait for 1 span

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "CpuMetrics"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 9 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.key" equals "bugsnag.system.cpu_measures_timestamps"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.4.value.arrayValue.values" is an array with 5 elements

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.5.key" equals "bugsnag.system.cpu_measures_total"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.5.value.arrayValue.values" is an array with 5 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double array attribute "bugsnag.system.cpu_measures_total" contains valid percentages

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.6.key" equals "bugsnag.system.cpu_measures_main_thread"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes.6.value.arrayValue.values" is an array with 5 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double array attribute "bugsnag.system.cpu_measures_main_thread" contains valid percentages

    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double attribute "bugsnag.metrics.cpu_mean_total" is a valid percentage
    
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" double attribute "bugsnag.system.cpu_mean_main_thread" is a valid percentage


  Scenario: Disable Cpu Metrics
    When I run the game in the "ConfigureCpuMetrics" state
    And I wait for 1 span
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.name" equals "BeforeStart"
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0.attributes" is an array with 4 elements
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" boolean attribute "bugsnag.span.first_class" is true
    * the trace payload field "resourceSpans.0.scopeSpans.0.spans.0" string attribute "bugsnag.span.category" equals "custom"