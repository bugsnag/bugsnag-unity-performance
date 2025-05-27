require "open3"
require "rbconfig"

HOST_OS = RbConfig::CONFIG['host_os']

def unity_directory
  if ENV.has_key? 'UNITY_PERFORMANCE_VERSION'
    "/Applications/Unity/Hub/Editor/#{ENV['UNITY_PERFORMANCE_VERSION']}"
  else
    raise 'No unity version set - use $UNITY_PERFORMANCE_VERSION'
  end
end

##
#
# Find the Unity executable based on the unity directory.
#
def unity_executable dir=unity_directory
  [File.join(dir, "Unity.app", "Contents", "MacOS", "Unity"),
   File.join(dir, "Editor", "Unity")].find do |unity|
    File.exist? unity
  end
end


##
#
# Run a command with the unity executable and the default command line parameters
# that we apply
#
def unity(*cmd, force_free: true, no_graphics: true)
  raise "Unable to locate Unity executable in #{unity_directory}" unless unity_executable

  cmd_prepend = [unity_executable, "-force-free"]
  unless force_free
    cmd_prepend = cmd_prepend - ["-force-free"]
  end
  unless no_graphics
    cmd_prepend = cmd_prepend - ["-nographics"]
  end
  cmd = cmd.unshift(*cmd_prepend)
  sh *cmd do |ok, res|
    if !ok
      puts File.read("unity.log") if File.exist?("unity.log")

      raise "unity error: #{res}"
    end
  end
end

def current_directory
  File.dirname(__FILE__)
end

def dev_project_path
  File.join(current_directory, "BugsnagPerformance")
end

def build_upm_package
  version_file_path = File.join("BugsnagPerformance", "Assets", "BugsnagPerformance", "Scripts", "Internal", "Version.cs")
  version_match = File.read(version_file_path).match(/public const string VersionString = "(\d+\.\d+\.\d+)"/)

  unless version_match
    raise "Could not extract version from #{version_file_path}"
  end

  version = version_match[1]

  script = File.join("upm", "scripts", "build-upm-package.sh")
  command = "#{script} #{version}"
  unless system command
    raise 'build upm package failed'
  end
end

def run_unit_tests
  unity "-runTests", "-batchmode", "-projectPath", dev_project_path, "-testPlatform", "EditMode", "-testResults", File.join(current_directory, "testResults.xml") , force_free: false
end

namespace :plugin do
  namespace :build do

    desc "Build UPM package"
    task :export do
      run_unit_tests
      build_upm_package
    end

  end
end

namespace :test do

  namespace :android do
    task :build do

      # Prepare the test fixture project by importing the upm package
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import package failed'
      end

      # Build the Android APK
      script = File.join("features", "scripts", "build_android.sh release")
      unless system script
        raise 'Android APK build failed'
      end
    end
     task :build_dev do
      # Prepare the test fixture project by importing the upm package
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import package failed'
      end

      # Build the Android APK
      script = File.join("features", "scripts", "build_android.sh dev")
      unless system script
        raise 'Android Dev APK build failed'
      end
    end
  end

  namespace :macos do
    task :build do

      # Prepare the test fixture project by importing the upm package
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import package failed'
      end

      # Build the Mac App
      script = File.join("features", "scripts", "build_macos.sh release")
      unless system script
        raise 'macos build failed'
      end
    end

    task :build_dev do

      # Prepare the test fixture project by importing the upm package
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import package failed'
      end

      # Build the Mac App
      script = File.join("features", "scripts", "build_macos.sh dev")
      unless system script
        raise 'macos build failed'
      end
    end
  end

   namespace :webgl do
    task :build do

      # Prepare the test fixture project by importing the upm package
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import package failed'
      end

      # Build the webgl App
      script = File.join("features", "scripts", "build_webgl.sh release")
      unless system script
        raise 'webgl build failed'
      end
    end

    task :build_dev do

      # Prepare the test fixture project by importing the upm package
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import package failed'
      end

      # Build the webgl App
      script = File.join("features", "scripts", "build_webgl.sh dev")
      unless system script
        raise 'webgl build failed'
      end
    end
  end

  namespace :ios do
    task :generate_xcode do

      # Prepare the test fixture project by importing the plugins
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import_package failed'
      end

      # Generate the Xcode project
      cd "features" do
        script = File.join("scripts", "generate_xcode_project.sh release")
        unless system script
          raise 'generate_xcode_project failed'
        end
      end
    end

    task :generate_xcode_dev do

      # Prepare the test fixture project by importing the plugins
      script = File.join("features", "scripts", "import_package.sh")
      unless system script
        raise 'import_package failed'
      end

      # Generate the Xcode project
      cd "features" do
        script = File.join("scripts", "generate_xcode_project.sh dev")
        unless system script
          raise 'generate_xcode_project_dev failed'
        end
      end
    end

    task :build_xcode do
      # Build and archive from the Xcode project
      cd "features" do
        script = File.join("scripts", "build_ios.sh release")
        unless system script
          raise 'IPA build failed'
        end
      end
    end

     task :build_xcode_dev do
      # Build and archive from the Xcode project
      cd "features" do
        script = File.join("scripts", "build_ios.sh dev")
        unless system script
          raise 'IPA build failed'
        end
      end
    end

    task build: %w[test:ios:generate_xcode test:ios:build_xcode] do
    end
  end

end

namespace :code do
  desc "Verify code formatting (dotnet format --verify-no-changes)"
  task :verify do
    # Call your script with the --verify argument
    sh "scripts/code_format.sh --verify"
  end

  desc "Apply code formatting (dotnet format)"
  task :format do
    # Call your script without the --verify argument
    sh "scripts/code_format.sh"
  end
end
