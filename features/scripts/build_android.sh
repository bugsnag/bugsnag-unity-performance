#!/usr/bin/env bash

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

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output
if [ "$BUILD_TYPE" == "dev" ]; then
  DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_android_dev.log"
  EXECUTE_METHOD="Builder.AndroidDev"
  APK_NAME="mazerunner-dev.apk"
else
  DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_android.log"
  EXECUTE_METHOD="Builder.AndroidRelease"
  APK_NAME="mazerunner.apk"
fi

project_path=`pwd`/mazerunner

# Build for Android

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $EXECUTE_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

if [ ! -f $project_path/$APK_NAME ]; then
  echo "APK not found at $project_path/$APK_NAME"
  exit 1
fi

mv $project_path/$APK_NAME $project_path/${APK_NAME%.apk}_${UNITY_PERFORMANCE_VERSION:0:4}.apk

popd
