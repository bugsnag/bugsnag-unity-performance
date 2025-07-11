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

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Remove old build artifacts
project_path=`pwd`/mazerunner
if [ "$BUILD_TYPE" == "dev" ]; then
  APP_NAME="mazerunner_macos_dev"
  EXECUTE_METHOD="Builder.MacOSDev"
else
  APP_NAME="mazerunner_macos"
  EXECUTE_METHOD="Builder.MacOSRelease"
fi

old_app_path="$project_path/${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.app"
old_zip_path="$project_path/${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.zip"

if [ -d "$old_app_path" ]; then
  rm -rf "$old_app_path"
fi

if [ -f "$old_zip_path" ]; then
  rm -f "$old_zip_path"
fi

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_macos.log"

# Build for MacOS
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $EXECUTE_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/${APP_NAME}.app $project_path/${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.app

(cd $project_path && zip -q -r ${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.zip ${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.app)

popd
