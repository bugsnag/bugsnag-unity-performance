require 'fileutils'

Before('@skip_unity_2018') do |_scenario|
  if ENV['UNITY_VERSION']
    unity_version = ENV['UNITY_VERSION'][0..3].to_i
    if unity_version == 2018
      skip_this_scenario('Skipping scenario on Unity 2018')
    end
  end
end


Before('@skip_webgl') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.browser.nil?
end

Before('@webgl_only') do |_scenario|
  skip_this_scenario('Skipping scenario') if Maze.config.browser.nil?
end


Before('@skip_macos') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze.config.os == 'macos'
end

Before('@macos_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'macos'
end


Before('@ios_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze::Helper.get_current_platform == 'ios'
end

Before('@cocoa_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'macos' || Maze::Helper.get_current_platform == 'ios'
end

Before('@skip_cocoa') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze.config.os == 'macos' || Maze::Helper.get_current_platform == 'ios'
end


Before('@windows_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'windows'
end
Before('@skip_windows') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze.config.os == 'windows'
end


Before('@switch_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze.config.os == 'switch'
end

Before('@skip_android') do |_scenario|
  skip_this_scenario("Skipping scenario") if Maze::Helper.get_current_platform == 'android'
end

Before('@android_only') do |_scenario|
  skip_this_scenario('Skipping scenario') unless Maze::Helper.get_current_platform == 'android'
end


BeforeAll do
  $api_key = 'a35a2a72bd230ac0aa0f52715bbdc6aa'
  Maze.config.enforce_bugsnag_integrity = false

  if Maze.config.os&.downcase == 'macos'
    # The default macOS Crash Reporter "#{app_name} quit unexpectedly" alert grabs focus which can cause tests to flake.
    # This option, which appears to have been introduced in macOS 10.11, displays a notification instead of the alert.
    Maze::Runner.run_command('defaults write com.apple.CrashReporter UseUNC 1')
  elsif Maze.config.os&.downcase == 'windows'
    # Allow the necessary environment variables to be passed from Ubuntu (under WSL) to the Windows test fixture
    ENV['WSLENV'] = 'BUGSNAG_SCENARIO:BUGSNAG_APIKEY:MAZE_ENDPOINT'
  elsif Maze.config.browser != nil # WebGL
    
    unity_version = ENV['UNITY_PERFORMANCE_VERSION']

    release_path = "features/fixtures/mazerunner/mazerunner_webgl_#{unity_version[0, 4]}"
    dev_path = "features/fixtures/mazerunner/mazerunner_webgl_dev_#{unity_version[0, 4]}"

    if File.exist?(release_path) && File.exist?(dev_path)
      raise "Both webgl builds exist: #{release_path} and #{dev_path}"
    elsif File.exist?(release_path)
      Maze.config.document_server_root = release_path
    elsif File.exist?(dev_path)
      Maze.config.document_server_root = dev_path
    else
      raise "Neither webgl build exists: #{release_path} or #{dev_path}"
    end

  elsif Maze.config.os&.downcase == 'switch'
    maze_ip = ENV['SWITCH_MAZE_IP']
    raise 'SWITCH_MAZE_IP must be set' unless maze_ip

    cache_type = ENV['SWITCH_CACHE_TYPE']
    case cache_type
    when nil, 'r'
      $logger.info 'Running tests for regular cache'
    when 'i'
      $logger.info 'Running tests for indexed cache'
      $switch_cache_type = 'i'
      $switch_cache_index = 3
      $switch_cache_mount_name = 'BugsnagCache'
    else
      raise "SWITCH_CACHE_TYPE must be 'r', or 'i', given: #{cache_type}"
    end


  elsif Maze.config.device.nil?
    raise '--browser (WebGL), --device (for Android/iOS) or --os (for desktop or switch) option must be set'
  end
end

Maze.hooks.before do
  if Maze.config.os == 'macos'
    support_dir = File.expand_path '~/Library/Application Support/com.bugsnag.Bugsnag'
    $logger.info "Clearing #{support_dir}"
    FileUtils.rm_rf(support_dir)
    $logger.info 'Clearing User defaults'
    begin
      Maze::Runner.run_command('defaults delete com.bugsnag.Mazerunner 2>/dev/null || exit 0', timeout: 10);
      Maze::Runner.run_command('defaults write com.bugsnag.Mazerunner ApplePersistenceIgnoreState YES', timeout: 10);

      # This is to get around a strange macOS bug where clearing prefs does not work 
      $logger.info 'Killing defaults service'
      Maze::Runner.run_command("killall -u #{ENV['USER']} cfprefsd 2>/dev/null || exit 0", timeout: 10)
      
      # Give the defaults service time to restart
      sleep 2
    rescue => e
      $logger.warn "Failed to clear macOS defaults: #{e.message}"
    end
  end
end

After do |scenario|
  next if scenario.status == :skipped

  case Maze::Helper.get_current_platform
  when 'macos'
    # Kill processes with better error handling
    system('killall Mazerunner 2>/dev/null')
    # Give processes time to terminate gracefully
    sleep 1
  when 'windows'
    # Kill processes with better error handling and timeout
    Maze::Runner.run_command("/mnt/c/Windows/system32/taskkill.exe /F /IM mazerunner_windows.exe 2>/dev/null || exit 0", timeout: 10)  
    Maze::Runner.run_command("/mnt/c/Windows/system32/taskkill.exe /F /IM mazerunner_windows_dev.exe 2>/dev/null || exit 0", timeout: 10)  
    # Give processes time to terminate
    sleep 1
  when 'webgl'
    begin
      execute_command('close_application')
    rescue => e
      $logger.warn "Failed to close WebGL application: #{e.message}"
    end
  when 'switch'
    begin
      # Terminate the app with timeout
      Maze::Runner.run_command('ControlTarget.exe terminate', timeout: 10)
    rescue => e
      $logger.warn "Failed to terminate Switch app: #{e.message}"
    end
  end
end

AfterAll do
  case Maze::Helper.get_current_platform
  when 'macos'
    app_name = Maze.config.app.gsub /\.app$/, ''
    Maze::Runner.run_command("log show --predicate '(process == \"#{app_name}\")' --style syslog --start '#{Maze.start_time}' > #{app_name}.log")
  end
end
