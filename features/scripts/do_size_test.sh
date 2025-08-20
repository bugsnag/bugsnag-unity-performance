#!/usr/bin/env bash

set -e

if [ -z "$UNITY_PERFORMANCE_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"

# Check if Unity executable exists
if [ ! -f "$UNITY_PATH/Unity" ]; then
  echo "Unity executable not found at $UNITY_PATH/Unity"
  exit 1
fi

DEFAULT_CLI_ARGS="-quit -batchmode -nographics"

project_path=features/fixtures/minimalapp
package_path=upm-package.zip
package_destination=features/fixtures/minimalapp/Packages

# Validate package exists
if [ ! -f "$package_path" ]; then
  echo "Package not found: $package_path"
  exit 1
fi

# Validate project path exists
if [ ! -d "$project_path" ]; then
  echo "Project path not found: $project_path"
  exit 1
fi

echo "Removing existing packages..."
rm -rf "$package_destination"

echo "Building Android without bugsnag..."
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildAndroidWithout -logFile build_android_minimal_without.log
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Android build without bugsnag failed with exit code $RESULT"
  if [ -f "build_android_minimal_without.log" ]; then
    echo "Last 30 lines of build log:"
    tail -30 "build_android_minimal_without.log"
  fi
  exit $RESULT
fi

echo "Building iOS without bugsnag..."
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildIosWithout -logFile export_ios_xcode_project_minimal_without.log
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "iOS build without bugsnag failed with exit code $RESULT"
  if [ -f "export_ios_xcode_project_minimal_without.log" ]; then
    echo "Last 30 lines of build log:"
    tail -30 "export_ios_xcode_project_minimal_without.log"
  fi
  exit $RESULT
fi

echo "Building Xcode project without bugsnag..."
if ! source ./features/scripts/build_xcode_project.sh features/fixtures/minimalapp/minimal_without_xcode without_bugsnag; then
  echo "Xcode build without bugsnag failed"
  exit 1
fi

echo "Importing package..."
if ! unzip -q "$package_path" -d "$package_destination"; then
  echo "Failed to extract package: $package_path"
  exit 1
fi

echo "Building Android with bugsnag..."
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildAndroidWith -logFile build_android_minimal_with.log
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Android build with bugsnag failed with exit code $RESULT"
  if [ -f "build_android_minimal_with.log" ]; then
    echo "Last 30 lines of build log:"
    tail -30 "build_android_minimal_with.log"
  fi
  exit $RESULT
fi

echo "Building iOS with bugsnag..."
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildIosWith -logFile export_ios_xcode_project_minimal_with.log
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "iOS build with bugsnag failed with exit code $RESULT"
  if [ -f "export_ios_xcode_project_minimal_with.log" ]; then
    echo "Last 30 lines of build log:"
    tail -30 "export_ios_xcode_project_minimal_with.log"
  fi
  exit $RESULT
fi

echo "Building Xcode project with bugsnag..."
if ! source ./features/scripts/build_xcode_project.sh features/fixtures/minimalapp/minimal_with_xcode with_bugsnag; then
  echo "Xcode build with bugsnag failed"
  exit 1
fi

cd features/fixtures/minimalapp

echo "Installing bundle dependencies for size test..."
if ! bundle install; then
  echo "Bundle install failed for size test"
  exit 1
fi

echo "Running danger for size comparison..."
if ! bundle exec danger; then
  echo "Danger execution failed"
  exit 1
fi

echo "Size test completed successfully!"