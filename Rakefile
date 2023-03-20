require "open3"
require "rbconfig"

HOST_OS = RbConfig::CONFIG['host_os']

##
#
# Find the directory that Unity has been installed in. This can be set via
# an ENV variable, if this has not been set then it will look in the default
# install location for both windows and mac.
#
def unity_directory

  if ENV.has_key? 'UNITY_VERSION'
    "/Applications/Unity/Hub/Editor/#{ENV['UNITY_VERSION']}"
  elsif ENV.has_key? "UNITY_DIR"
    ENV["UNITY_DIR"]
  else
    raise 'No unity directory set - use $UNITY_DIR or $UNITY_VERSION'
  end
end

##
#
# Find the Unity executable based on the unity directory.
#
def unity_executable dir=unity_directory
  [File.join(dir, "Unity.app", "Contents", "MacOS", "Unity"),
   File.join(dir, "Editor", "Unity"),
   File.join(dir, "Editor", "Unity.exe")].find do |unity|
    File.exists? unity
  end
end

def unity_dll_location
  [File.join(unity_directory, "Unity.app", "Contents", "Managed"), File.join(unity_directory, "Editor", "Data", "Managed")].find do |unity|
    File.exists? unity
  end
end

##
# Get existing unity executable path or exit with error
#
# Returns pair containing unity path and executable
def get_required_unity_paths
  dir = unity_directory
  exe = unity_executable(dir)
  raise "No unity executable found in '#{dir}'" if exe.nil?
  unless File.exists? exe
    raise "Unity not found at path '#{exe}' - set $UNITY_DIR (full path) or $UNITY_VERSION (loaded via hub) to customize"
  end
  [dir, exe]
end

##
#
# Run a command with the unity executable and the default command line parameters
# that we apply
#
def unity(*cmd, force_free: true, no_graphics: true)
  raise "Unable to locate Unity executable in #{unity_directory}" unless unity_executable

  cmd_prepend = [unity_executable, "-force-free", "-batchmode", "-nographics", "-logFile", "unity.log", "-quit"]
  unless force_free
    cmd_prepend = cmd_prepend - ["-force-free"]
  end
  unless no_graphics
    cmd_prepend = cmd_prepend - ["-nographics"]
  end
  cmd = cmd.unshift(*cmd_prepend)
  sh *cmd do |ok, res|
    if !ok
      puts File.read("unity.log") if File.exists?("unity.log")

      raise "unity error: #{res}"
    end
  end
end

def current_directory
  File.dirname(__FILE__)
end

def project_path
  File.join(current_directory, "package-project")
end

def assets_path
  File.join(project_path, "Assets", "BugsnagPerformance/Plugins")
end

def export_package
  package_output = File.join(current_directory, "BugsnagPerformance.unitypackage")
  rm_f package_output
  unity "-projectPath", project_path, "-exportPackage", "Assets/BugsnagPerformance", package_output, force_free: false
end

namespace :plugin do
  namespace :build do

    task all: [:clean, :assets, :csharp, :export]

    desc "Delete all build artifacts"
    task :clean do
      # remove any leftover artifacts from the package generation directory
      sh "git", "clean", "-dfx", "package-project"      
    end
    
    desc "Compile and copy csharp lib dlls to packaging project"
    task :csharp do
      env = { "UnityDir" => unity_dll_location }
      unless system env, "./build.sh"
        raise "Failed to build csharp plugin"
      end
      cd File.join("src", "BugsnagPerformance", "BugsnagPerformance", "bin", "Release") do
        cp File.realpath("BugsnagPerformance.dll"), assets_path
      end
    end

    desc "Export unitypackage"
    task :export do
      export_package
    end

  end
end

namespace :test do
  
  namespace :android do
    task :build do
      # Check that a Unity version has been selected and the path exists before calling the build script
      unity_path, unity = get_required_unity_paths

      # Prepare the test fixture project by importing the plugins
      env = { "UNITY_PATH" => File.dirname(unity) }
      script = File.join("features", "scripts", "import_package.sh")
      unless system env, script
        raise 'import package failed'
      end

      # Build the Android APK
      script = File.join("features", "scripts", "build_android.sh")
      unless system env, script
        raise 'Android APK build failed'
      end
    end
  end

  namespace :ios do
    task :generate_xcode do
      # Check that a Unity version has been selected and the path exists before calling the build script
      unity_path, unity = get_required_unity_paths

      # Prepare the test fixture project by importing the plugins
      env = { "UNITY_PATH" => File.dirname(unity) }
      script = File.join("features", "scripts", "import_package.sh")
      unless system env, script
        raise 'import_package failed'
      end

      # Generate the Xcode project
      cd "features" do
        script = File.join("scripts", "generate_xcode_project.sh")
        unless system env, script
          raise 'generate_xcode_project failed'
        end
      end
    end

    task :build_xcode do
      # Build and archive from the Xcode project
      cd "features" do
        script = File.join("scripts", "build_ios.sh")
        unless system script
          raise 'IPA build failed'
        end
      end
    end

    task build: %w[test:ios:generate_xcode test:ios:build_xcode] do
    end
  end

end
