#!/usr/bin/env bash

set -e

if [ -z "$UNITY_PERFORMANCE_VERSION" ]; then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

if [ -z "$1" ]; then
  echo "Build type must be specified (dev or release)"
  exit 1
fi

BUILD_TYPE=$1

if [ "$BUILD_TYPE" != "dev" ] && [ "$BUILD_TYPE" != "release" ]; then
  echo "Invalid build type specified. Use 'dev' or 'release'."
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"

# Check if Unity executable exists
if [ ! -f "$UNITY_PATH/Unity" ]; then
  echo "Unity executable not found at $UNITY_PATH/Unity"
  exit 1
fi

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

DEFAULT_CLI_ARGS="-quit -nographics -batchmode -logFile generateXcodeProject.log"

project_path=`pwd`/mazerunner

# Validate project path
if [ ! -d "$project_path" ]; then
  echo "Project path not found: $project_path"
  exit 1
fi

# Generate the Xcode project for iOS
if [ "$BUILD_TYPE" == "dev" ]; then
  EXECUTE_METHOD="Builder.IosDev"
  XCODE_OUTPUT_DIR="$project_path/mazerunner_dev_xcode"
else
  EXECUTE_METHOD="Builder.IosRelease"
  XCODE_OUTPUT_DIR="$project_path/mazerunner_xcode"
fi

# Clean up old Xcode project if it exists
if [ -d "$XCODE_OUTPUT_DIR" ]; then
  echo "Removing existing Xcode project: $XCODE_OUTPUT_DIR"
  rm -rf "$XCODE_OUTPUT_DIR"
fi

echo "Generating Xcode project for iOS ($BUILD_TYPE)..."
echo "Unity path: $UNITY_PATH/Unity"
echo "Project path: $project_path"
echo "Execute method: $EXECUTE_METHOD"
echo "Expected output: $XCODE_OUTPUT_DIR"

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $EXECUTE_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Xcode project generation failed with exit code $RESULT"
  if [ -f "generateXcodeProject.log" ]; then
    echo "Last 50 lines of generation log:"
    tail -50 "generateXcodeProject.log"
  fi
  exit $RESULT
fi

# Verify the Xcode project was created
if [ ! -d "$XCODE_OUTPUT_DIR" ]; then
  echo "ERROR: Expected Xcode project directory not found: $XCODE_OUTPUT_DIR"
  echo "Contents of project directory:"
  ls -la "$project_path/"
  exit 1
fi

if [ ! -d "$XCODE_OUTPUT_DIR/Unity-iPhone.xcodeproj" ]; then
  echo "ERROR: Unity-iPhone.xcodeproj not found in: $XCODE_OUTPUT_DIR"
  echo "Contents of Xcode directory:"
  ls -la "$XCODE_OUTPUT_DIR/"
  exit 1
fi

echo "✅ Xcode project generation completed successfully!"
echo "Project location: $XCODE_OUTPUT_DIR"

popd
