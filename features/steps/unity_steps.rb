require 'cgi'

def execute_command(action, scenario_name = '')
  command = {
    action: action,
    scenarioName: scenario_name
  }
  Maze::Server.commands.add command

  # Ensure fixture has read the command
  count = 300
  sleep 0.1 until Maze::Server.commands.remaining.empty? || (count -= 1) < 1
  raise 'Test fixture did not GET /command' unless Maze::Server.commands.remaining.empty?
end

When('I clear the Bugsnag cache') do
  case Maze::Helper.get_current_platform
  when 'macos', 'webgl'
    # Call executable directly rather than use open, which flakes on CI
    log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner --args -logfile #{log} > /dev/null"
    Maze::Runner.run_command(command, blocking: false)
    execute_command('clear_cache')

  when 'windows'
    win_log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app} --args -logfile #{win_log}"
    Maze::Runner.run_command(command, blocking: false)
    execute_command('clear_cache')

  when 'android', 'ios'
    execute_command('clear_cache')
  when 'browser'
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('clear_cache')

  when 'switch'
    switch_run_on_target
    execute_command('clear_cache')

  else
    raise "Platform #{platform} has not been considered"
  end

  sleep 2

end

When('I run the game in the {string} state') do |state|
  platform = Maze::Helper.get_current_platform
  case platform
  when 'macos'
    # Call executable directly rather than use open, which flakes on CI
    log = File.join(Dir.pwd, "#{state}-mazerunner.log")
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner --args -logfile #{log} > /dev/null"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('run_scenario', state)

  when 'windows'
    win_log = File.join(Dir.pwd, 'mazerunner.log')
    command = "#{Maze.config.app} --args -logfile #{win_log}"
    Maze::Runner.run_command(command, blocking: false)

    execute_command('run_scenario', state)

  when 'android', 'ios'
    execute_command('run_scenario', state)

  when 'browser'
    # WebGL in a browser
    url = "http://localhost:#{Maze.config.document_server_port}/index.html"
    $logger.debug "Navigating to URL: #{url}"
    step("I navigate to the URL \"#{url}\"")
    execute_command('run_scenario', state)

  when 'switch'

    switch_run_on_target
    execute_command('run_scenario', state)

  else
    raise "Platform #{platform} has not been considered"
  end
end

When('I wait for requests to persist') do
  sleep 2
end

When('I relaunch the app') do

 platform = Maze::Helper.get_current_platform
  case platform
  when 'macos'
    execute_command('close_application')
    sleep 3
    log = File.join(Dir.pwd, "#{state}RESTART-mazerunner.log")
    command = "#{Maze.config.app}/Contents/MacOS/Mazerunner --args -logfile #{log} > /dev/null"
  when 'windows'
   
  when 'android', 'ios'
    Maze.driver.launch_app
  when 'browser'
   
  else
    raise "Platform #{platform} has not been considered"
  end

  sleep 3
end

When('I close the Unity app') do
  execute_command('close_application')
end

When("I clear any error dialogue") do
  click_if_present 'android:id/button1'
  click_if_present 'android:id/aerr_close'
  click_if_present 'android:id/aerr_restart'
end

def click_if_present(element)
  return false unless Maze.driver.wait_for_element(element, 1)

  Maze.driver.click_element_if_present(element)
rescue Selenium::WebDriver::Error::UnknownError
  # Ignore Appium errors (e.g. during an ANR)
  return false
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
