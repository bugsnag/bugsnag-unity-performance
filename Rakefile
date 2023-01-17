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

    if is_mac?
    "/Applications/Unity/Hub/Editor/#{ENV['UNITY_VERSION']}"
    elsif is_windows?
      "C:\\Program Files\\Unity\\Hub\\Editor\\#{ENV['UNITY_VERSION']}"
    end

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

def export_package name="BugsnagPerformance.unitypackage"
  package_output = File.join(current_directory, name)
  rm_f package_output
  unity "-projectPath", project_path, "-exportPackage", "Assets/BugsnagPerformance", package_output, force_free: false
end

namespace :plugin do
  namespace :build do

    task all: [:assets, :csharp]

    desc "Delete all build artifacts"
    task :clean do
      # remove any leftover artifacts from the package generation directory
      sh "git", "clean", "-dfx", "package-project"      
    end

    task :assets do
     # cp_r File.join(current_directory, "src", "Assets"), project_path, preserve: true
    end

    
    task :csharp do
      env = { "UnityDir" => unity_dll_location }
      unless system env, "./build.sh"
        raise "Failed to build csharp plugin"
      end

      cd File.join("src", "BugsnagPerformance", "BugsnagPerformance", "bin", "Release", "net35") do
        cp File.realpath("BugsnagPerformance.dll"), assets_path
      end
    end


  end

  task :export_package do
    export_package
  end

  desc "Generate release artifacts"
  task export: ["plugin:build:clean"] do
    export_package("BugsnagPerformance.unitypackage")
  end
 
end


