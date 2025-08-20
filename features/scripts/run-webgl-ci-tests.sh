#!/bin/bash -e

# Check if the argument is provided
if [ $# -ne 1 ]; then
  echo "Usage: $0 <release|dev>"
  exit 1
fi

# Set the mode based on the argument
BUILD_TYPE=$1

if [ "$BUILD_TYPE" == "dev" ]; then
  ZIP_FILE="mazerunner_webgl_dev_${UNITY_PERFORMANCE_VERSION:0:4}.zip"
  EXTRACT_DIR="mazerunner_webgl_dev_${UNITY_PERFORMANCE_VERSION:0:4}"
elif [ "$BUILD_TYPE" == "release" ]; then
  ZIP_FILE="mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4}.zip"
  EXTRACT_DIR="mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4}"
else
  echo "Invalid argument: $BUILD_TYPE. Use 'release' or 'dev'."
  exit 1
fi

pushd features/fixtures/mazerunner/

# Validate zip file exists
if [ ! -f "$ZIP_FILE" ]; then
  echo "Error: Zip file $ZIP_FILE not found"
  exit 1
fi

# Clean up any existing extracted directory
if [ -d "$EXTRACT_DIR" ]; then
  echo "Removing existing directory: $EXTRACT_DIR"
  rm -rf "$EXTRACT_DIR"
fi

# Extract with error handling
echo "Extracting $ZIP_FILE..."
if ! unzip -q "$ZIP_FILE"; then
  echo "Error: Failed to extract $ZIP_FILE"
  exit 1
fi

popd

echo "Installing bundle dependencies..."
if ! bundle install; then
  echo "Error: Bundle install failed"
  exit 1
fi

if [ "$BUILD_TYPE" == "dev" ]; then
  bundle exec maze-runner --farm=local --browser=firefox --fail-fast features/dev.feature
else
  bundle exec maze-runner --farm=local --browser=firefox --fail-fast --exclude=features/dev.feature features
fi