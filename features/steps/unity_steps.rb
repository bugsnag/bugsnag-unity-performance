require 'cgi'

def execute_command(action, scenario_name = '')
  command = {
    action: action,
    scenarioName: scenario_name
  }
  Maze::Server.commands.add command
end

When('I clear the Bugsnag cache') do
  case Maze::Helper.get_current_platform
  when 'macos', 'windows', 'android', 'ios', 'switch'
    execute_command('clear_cache')
  when 'browser'
    url = "http://localhost:#{Maze.config.port}/docs/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('clear_cache')
  else
    raise "Platform #{platform} has not been considered"
  end
end

When('I run the game in the {string} state') do |state|
  platform = Maze::Helper.get_current_platform
  case platform
  when 'macos','android', 'ios', 'switch', 'windows'
    execute_command('run_scenario', state)
  when 'browser'
    # WebGL in a browser
    url = "http://localhost:#{Maze.config.port}/docs/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('run_scenario', state)
  else
    raise "Platform #{platform} has not been considered"
  end
end

When('I wait for requests to persist') do
  sleep 2
end

Then("the span named {string} has a minimum duration of {int}") do |span_name,duration|

  spans = spans_from_request_list(Maze::Server.list_for("traces"))

  spans_with_name = spans.find_all { |span| span['name'].eql?(span_name) }
  raise Test::Unit::AssertionFailedError.new "No spans were found with the name #{span_name}" if spans_with_name.empty?

  span = spans_with_name.first

  start_time = Integer(span["startTimeUnixNano"])

  end_time = Integer(span["endTimeUnixNano"])

  found_duration = end_time - start_time

  print(found_duration)

  Maze.check.true(found_duration > duration);

end

Then("the span named {string} has a maximum duration of {int}") do |span_name,duration|

  spans = spans_from_request_list(Maze::Server.list_for("traces"))

  spans_with_name = spans.find_all { |span| span['name'].eql?(span_name) }
  raise Test::Unit::AssertionFailedError.new "No spans were found with the name #{span_name}" if spans_with_name.empty?

  span = spans_with_name.first

  start_time = Integer(span["startTimeUnixNano"])

  end_time = Integer(span["endTimeUnixNano"])

  found_duration = end_time - start_time

  print(found_duration)

  Maze.check.true(found_duration < duration);

end

Then('the span named {string} exists') do |span_name|
  spans = spans_from_request_list(Maze::Server.list_for("traces"))

  spans_with_name = spans.find_all { |span| span['name'].eql?(span_name) }

  Maze.check.true(spans_with_name.length() == 1);
end

Then('the span named {string} is the parent of the span named {string}') do |span1name, span2name|

  spans = spans_from_request_list(Maze::Server.list_for("traces"))

  span1 = spans.find_all { |span| span['name'].eql?(span1name) }.first

  span2 = spans.find_all { |span| span['name'].eql?(span2name) }.first

  Maze.check.true(span1['spanId'] == span2['parentSpanId']);

end

Then('the span named {string} has no parent') do |spanName|
  spans = spans_from_request_list(Maze::Server.list_for("traces"))

  span1 = spans.find_all { |span| span['name'].eql?(spanName) }.first

  Maze.check.true(span1['parentSpanId'] == nil);
end

Then('the span named {string} starts and ends before the span named {string} ends and lasts at least 1 second') do |span1_name, span2_name|
  spans = spans_from_request_list(Maze::Server.list_for("traces"))

  span1 = spans.find { |span| span['name'].eql?(span1_name) }
  span2 = spans.find { |span| span['name'].eql?(span2_name) }

  raise Test::Unit::AssertionFailedError.new "No span found with the name #{span1_name}" if span1.nil?
  raise Test::Unit::AssertionFailedError.new "No span found with the name #{span2_name}" if span2.nil?

  span1_start_time = Integer(span1["startTimeUnixNano"])
  span1_end_time = Integer(span1["endTimeUnixNano"])
  span2_end_time = Integer(span2["endTimeUnixNano"])

  duration_nanos = span1_end_time - span1_start_time
  min_duration_nanos = 1_000_000_000 # 1 second in nanoseconds

  Maze.check.true(span1_start_time < span2_end_time, "Expected span '#{span1_name}' to start before span '#{span2_name}' ends")
  Maze.check.true(span1_end_time < span2_end_time, "Expected span '#{span1_name}' to end before span '#{span2_name}' ends")
  Maze.check.true(duration_nanos >= min_duration_nanos, "Expected span '#{span1_name}' to last at least 1 second, but it lasted #{duration_nanos} nanoseconds")
end

def check_span_first_class(span_name, expected)
  spans = spans_from_request_list(Maze::Server.list_for("traces"))
  span = spans.find { |s| s['name'].eql?(span_name) }
  raise Test::Unit::AssertionFailedError.new "No span found with the name #{span_name}" if span.nil?

  first_class_attr = span['attributes'].find { |attr| attr['key'] == 'bugsnag.span.first_class' }
  raise Test::Unit::AssertionFailedError.new "No attribute 'bugsnag.span.first_class' found for #{span_name}" if first_class_attr.nil?

  Maze.check.true(
    first_class_attr['value']['boolValue'] == expected,
    "Expected '#{span_name}' to be first class: #{expected}, but it was not."
  )
end

Then('the span named {string} is first class') do |span_name|
  check_span_first_class(span_name, true)
end

Then('the span named {string} is not first class') do |span_name|
  check_span_first_class(span_name, false)
end

def check_valid_percentage(value, field_name, attribute_name, request_type)
  Maze.check.operator(
    value,
    :>=,
    0.0,
    "Attribute '#{attribute_name}' in '#{field_name}' is less than 0% for request type '#{request_type}': #{value}"
  )
  Maze.check.operator(
    value,
    :<=,
    100.0,
    "Attribute '#{attribute_name}' in '#{field_name}' is greater than 100% for request type '#{request_type}': #{value}"
  )
end

When('the {request_type} payload field {string} double attribute {string} is a valid percentage') do |request_type, field_name, attribute_name|
  list = Maze::Server.list_for(request_type)
  attributes = Maze::Helper.read_key_path(list.current[:body], "#{field_name}.attributes")
  Maze.check.not_nil(attributes, "No attributes found for '#{field_name}' in request type '#{request_type}'.")

  attr = attributes.find { |a| a['key'] == attribute_name }
  Maze.check.not_nil(
    attr,
    "No attribute named '#{attribute_name}' was found in '#{field_name}' for request type '#{request_type}'."
  )

  value = attr.dig('value', 'doubleValue')
  Maze.check.not_nil(
    value,
    "Attribute '#{attribute_name}' in '#{field_name}' is missing a doubleValue."
  )

  float_value = value.to_f
  check_valid_percentage(float_value, field_name, attribute_name, request_type)
end

When('the {request_type} payload field {string} double array attribute {string} contains valid percentages') do |request_type, field_name, attribute_name|
  list = Maze::Server.list_for(request_type)
  attributes = Maze::Helper.read_key_path(list.current[:body], "#{field_name}.attributes")
  Maze.check.not_nil(attributes, "No attributes found for '#{field_name}' in request type '#{request_type}'.")

  attr = attributes.find { |a| a['key'] == attribute_name }
  Maze.check.not_nil(
    attr,
    "No attribute named '#{attribute_name}' was found in '#{field_name}' for request type '#{request_type}'."
  )

  array_value = attr.dig('value', 'arrayValue', 'values')
  Maze.check.not_nil(
    array_value,
    "Attribute '#{attribute_name}' in '#{field_name}' is not an arrayValue or is missing 'values' key."
  )

  # Check each element is in the valid percentage range
  array_value.each do |element|
    double_val_raw = element['doubleValue']
    Maze.check.not_nil(double_val_raw, "Expected a 'doubleValue' in the array but got: #{element}")

    float_value = double_val_raw.to_f
    check_valid_percentage(float_value, field_name, attribute_name, request_type)
  end
end

When('the {request_type} payload field {string} is an array with at least {int} elements') do |request_type, field_name, min_count|
  list = Maze::Server.list_for(request_type)
  array_value = Maze::Helper.read_key_path(list.current[:body], field_name)

  Maze.check.not_nil(
    array_value,
    "Field '#{field_name}' not found in request type '#{request_type}'"
  )

  Maze.check.true(
    array_value.is_a?(Array),
    "Field '#{field_name}' is not an array in request type '#{request_type}', got: #{array_value.class}"
  )

  actual_count = array_value.length
  Maze.check.operator(
    actual_count,
    :>=,
    min_count,
    "Expected '#{field_name}' to have at least #{min_count} elements in request type '#{request_type}', but got #{actual_count}"
  )
end

When('I stop the Unity app') do
  stop_app
end

When('I start the Unity app') do
  start_app
end

def start_app
  platform = Maze::Helper.get_current_platform
  case platform
  when 'macos'
    # Open the fixture - call executable directly rather than use open, which flakes on CI
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner --args > /dev/null"
    Maze::Runner.run_command(command, blocking: false)
  when 'windows'
    command = "#{Maze.config.app}"
    Maze::Runner.run_command(command, blocking: false)
  when 'android', 'ios'
    manager = Maze::Api::Appium::AppManager.new
    manager.activate
  when 'switch'
    switch_run_on_target
  when 'browser'
    # WebGL - do nothing
  else
    raise "Platform #{platform} has not been considered"
  end
end

def stop_app
  platform = Maze::Helper.get_current_platform
  case platform
  when 'macos'
    `killall Mazerunner`
  when 'windows'
    # This assumes Maze Runner is being run under WSL
    process_name = File.basename(Maze.config.app)
    `/mnt/c/Windows/system32/taskkill.exe /F /IM #{process_name}`
  when 'android', 'ios'
    manager = Maze::Api::Appium::AppManager.new
    manager.terminate
  when 'switch'
    # Terminate the app
    Maze::Runner.run_command('ControlTarget.exe terminate')
  when 'browser'
    # WebGL - do nothing
  else
    raise "Platform #{platform} has not been considered"
  end
end

def spans_from_request_list(list)
  list.remaining
      .flat_map { |req| req[:body]['resourceSpans'] }
      .flat_map { |r| r['scopeSpans'] }
      .flat_map { |s| s['spans'] }
      .select { |s| !s.nil? }
end

#
# Find a span by name and return it
#
def find_span_by_name(span_name)
  spans = spans_from_request_list(Maze::Server.list_for("traces"))
  span = spans.find { |s| s['name'] == span_name }
  raise Test::Unit::AssertionFailedError, "No span found with the name #{span_name}" if span.nil?
  span
end

#
# Find an attribute on a span by key
#
def find_span_attr(span, key)
  attr = span['attributes']&.find { |a| a['key'] == key }
  raise Test::Unit::AssertionFailedError, "No attribute '#{key}' found on span '#{span['name']}'" if attr.nil?
  attr
end

Then('the span named {string} has an attribute {string}') do |span_name, attr_key|
  span = find_span_by_name(span_name)
  find_span_attr(span, attr_key) # will raise if missing
end

Then('the span named {string} has string attribute {string} equal to {string}') do |span_name, attr_key, expected|
  span = find_span_by_name(span_name)
  attr = find_span_attr(span, attr_key)
  actual = attr.dig('value', 'stringValue')
  Maze.check.equal(expected, actual, "Expected #{span_name}.#{attr_key} to be '#{expected}' but was '#{actual}'")
end

Then('the span named {string} has integer attribute {string} greater than {int}') do |span_name, attr_key, min_value|
  span = find_span_by_name(span_name)
  attr = find_span_attr(span, attr_key)
  actual = attr.dig('value', 'intValue').to_i
  Maze.check.operator(actual, :>, min_value, "Expected #{span_name}.#{attr_key} to be > #{min_value} but was #{actual}")
end

Then('the span named {string} has boolean attribute {string} equal to {word}') do |span_name, attr_key, expected_str|
  expected = expected_str == 'true'
  span = find_span_by_name(span_name)
  attr = find_span_attr(span, attr_key)
  actual = attr.dig('value', 'boolValue')
  Maze.check.equal(expected, actual, "Expected #{span_name}.#{attr_key} to be #{expected} but was #{actual}")
end

Then('the span named {string} has array attribute {string} with at least {int} elements') do |span_name, attr_key, min_count|
  span = find_span_by_name(span_name)
  attr = find_span_attr(span, attr_key)
  arr = attr.dig('value', 'arrayValue', 'values')
  raise Test::Unit::AssertionFailedError, "Attribute '#{attr_key}' on '#{span_name}' is not an array" if arr.nil?
  Maze.check.operator(arr.length, :>=, min_count, "Expected at least #{min_count} elements but got #{arr.length}")
end

Then('the span named {string} has double attribute {string} that is a valid percentage') do |span_name, attr_key|
  span = find_span_by_name(span_name)
  attr = find_span_attr(span, attr_key)
  val = attr.dig('value', 'doubleValue')
  raise Test::Unit::AssertionFailedError, "Attribute '#{attr_key}' on '#{span_name}' has no doubleValue" if val.nil?
  num = val.to_f
  Maze.check.operator(num, :>=, 0.0, "Expected #{attr_key} >= 0.0, got #{num}")
  Maze.check.operator(num, :<=, 100.0, "Expected #{attr_key} <= 100.0, got #{num}")
end

Then('the span named {string} has double array attribute {string} containing valid percentages') do |span_name, attr_key|
  span = find_span_by_name(span_name)
  attr = find_span_attr(span, attr_key)
  arr = attr.dig('value', 'arrayValue', 'values')
  raise Test::Unit::AssertionFailedError, "Attribute '#{attr_key}' on '#{span_name}' is not an array" if arr.nil?
  arr.each do |el|
    val = el['doubleValue']
    raise Test::Unit::AssertionFailedError, "Expected doubleValue in #{attr_key} array but got #{el}" if val.nil?
    num = val.to_f
    Maze.check.operator(num, :>=, 0.0, "Expected value in #{attr_key} >= 0.0, got #{num}")
    Maze.check.operator(num, :<=, 100.0, "Expected value in #{attr_key} <= 100.0, got #{num}")
  end
end
