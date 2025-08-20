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

# Check if Unity path exists
if [ ! -f "$UNITY_PATH/Unity" ]; then
  echo "Unity executable not found at $UNITY_PATH/Unity"
  exit 1
fi

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

echo "Cleaning up old build artifacts..."
if [ -d "$old_app_path" ]; then
  echo "Removing old app: $old_app_path"
  rm -rf "$old_app_path"
fi

if [ -f "$old_zip_path" ]; then
  echo "Removing old zip: $old_zip_path"
  rm -f "$old_zip_path"
fi

# Also clean up the immediate build output
if [ -d "$project_path/${APP_NAME}.app" ]; then
  echo "Removing previous build output: $project_path/${APP_NAME}.app"
  rm -rf "$project_path/${APP_NAME}.app"
fi

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_macos.log"

echo "Building macOS app with Unity..."
echo "Unity path: $UNITY_PATH/Unity"
echo "Project path: $project_path"
echo "Execute method: $EXECUTE_METHOD"

# Build for MacOS
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $EXECUTE_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Unity build failed with exit code $RESULT"
  if [ -f "$project_path/build_macos.log" ]; then
    echo "Last 50 lines of build log:"
    tail -50 "$project_path/build_macos.log"
  fi
  exit $RESULT
fi

# Check if the build output exists
if [ ! -d "$project_path/${APP_NAME}.app" ]; then
  echo "Error: Expected build output not found: $project_path/${APP_NAME}.app"
  exit 1
fi

echo "Renaming build output..."
mv $project_path/${APP_NAME}.app $project_path/${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.app

echo "Creating zip archive..."
(cd $project_path && zip -q -r ${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.zip ${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.app)

# Verify the zip was created successfully
if [ ! -f "$project_path/${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.zip" ]; then
  echo "Error: Failed to create zip archive"
  exit 1
fi

echo "Build completed successfully!"
echo "Output: $project_path/${APP_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.zip"

popd
