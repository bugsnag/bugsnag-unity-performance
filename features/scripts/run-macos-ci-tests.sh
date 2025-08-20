#!/bin/bash -e

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

pushd features/fixtures/mazerunner/
if [ "$BUILD_TYPE" == "dev" ]; then
  ZIP_FILE="mazerunner_macos_dev_${UNITY_PERFORMANCE_VERSION:0:4}.zip"
  APP_NAME="mazerunner_macos_dev_${UNITY_PERFORMANCE_VERSION:0:4}.app"
else
  ZIP_FILE="mazerunner_macos_${UNITY_PERFORMANCE_VERSION:0:4}.zip"
  APP_NAME="mazerunner_macos_${UNITY_PERFORMANCE_VERSION:0:4}.app"
fi

# Validate zip file exists
if [ ! -f "$ZIP_FILE" ]; then
  echo "Error: Zip file $ZIP_FILE not found"
  exit 1
fi

# Clean up any existing app directory before extraction
if [ -d "$APP_NAME" ]; then
  echo "Removing existing app directory: $APP_NAME"
  rm -rf "$APP_NAME"
fi

# Extract with error handling
echo "Extracting $ZIP_FILE..."
if ! unzip -q "$ZIP_FILE"; then
  echo "Error: Failed to extract $ZIP_FILE"
  exit 1
fi
popd

rm -rf Gemfile.lock
echo "Installing bundle dependencies..."
if ! bundle install; then
  echo "Error: Bundle install failed"
  exit 1
fi

if [ "$BUILD_TYPE" == "dev" ]; then
  bundle exec maze-runner --app=features/fixtures/mazerunner/$APP_NAME --os=macos --fail-fast features/dev.feature
else
  bundle exec maze-runner --app=features/fixtures/mazerunner/$APP_NAME --os=macos --fail-fast --exclude=features/dev.feature features
fi
