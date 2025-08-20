#!/usr/bin/env bash

set -e

if [ -z "$UNITY_PERFORMANCE_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

if [ -z "$1" ]
then
  echo "Build type must be specified (dev or release)"
  exit 1
fi

BUILD_TYPE=$1

if [ "$BUILD_TYPE" != "dev" ] && [ "$BUILD_TYPE" != "release" ]
then
  echo "Invalid build type specified. Use 'dev' or 'release'."
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"

# Check if Unity path exists
if [ ! -f "$UNITY_PATH/Unity" ]; then
  echo "Unity executable not found at $UNITY_PATH/Unity"
  exit 1
fi

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

project_path=`pwd`/mazerunner

# Run unity and immediately exit afterwards, log all output
if [ "$BUILD_TYPE" == "dev" ]; then
  DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_android_dev.log"
  EXECUTE_METHOD="Builder.AndroidDev"
  APK_NAME="mazerunner-dev.apk"
  FINAL_APK_NAME="mazerunner-dev_${UNITY_PERFORMANCE_VERSION:0:4}.apk"
else
  DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_android.log"
  EXECUTE_METHOD="Builder.AndroidRelease"
  APK_NAME="mazerunner.apk"
  FINAL_APK_NAME="mazerunner_${UNITY_PERFORMANCE_VERSION:0:4}.apk"
fi

# Clean up old build artifacts
echo "Cleaning up old build artifacts..."
if [ -f "$project_path/$FINAL_APK_NAME" ]; then
  echo "Removing old APK: $project_path/$FINAL_APK_NAME"
  rm -f "$project_path/$FINAL_APK_NAME"
fi

if [ -f "$project_path/$APK_NAME" ]; then
  echo "Removing previous build output: $project_path/$APK_NAME"
  rm -f "$project_path/$APK_NAME"
fi

echo "Building Android APK with Unity..."
echo "Unity path: $UNITY_PATH/Unity"
echo "Project path: $project_path"
echo "Execute method: $EXECUTE_METHOD"

# Build for Android
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $EXECUTE_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Unity build failed with exit code $RESULT"
  log_file="build_android.log"
  if [ "$BUILD_TYPE" == "dev" ]; then
    log_file="build_android_dev.log"
  fi
  if [ -f "$project_path/$log_file" ]; then
    echo "Last 50 lines of build log:"
    tail -50 "$project_path/$log_file"
  fi
  exit $RESULT
fi

# Check if the build output exists
if [ ! -f "$project_path/$APK_NAME" ]; then
  echo "Error: Expected build output not found: $project_path/$APK_NAME"
  exit 1
fi

echo "Renaming APK to final name..."
mv $project_path/$APK_NAME $project_path/$FINAL_APK_NAME

echo "Build completed successfully!"
echo "Output: $project_path/$FINAL_APK_NAME"

popd
