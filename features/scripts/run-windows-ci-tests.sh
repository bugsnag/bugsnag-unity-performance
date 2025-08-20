#!/bin/bash -e

# Check if the argument is provided
if [ $# -ne 1 ]; then
  echo "Usage: $0 <release|dev>"
  exit 1
fi

# Set the mode based on the argument
BUILD_TYPE=$1

pushd features/fixtures/mazerunner/build

if [ "$BUILD_TYPE" == "dev" ]; then
  ZIP_FILE="Windows-dev-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
  EXE_FILE="mazerunner_windows_dev.exe"
elif [ "$BUILD_TYPE" == "release" ]; then
  ZIP_FILE="Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
  EXE_FILE="mazerunner_windows.exe"
else
  echo "Invalid argument: $BUILD_TYPE. Use 'release' or 'dev'."
  exit 1
fi

# Validate zip file exists
if [ ! -f "$ZIP_FILE" ]; then
  echo "Error: Zip file $ZIP_FILE not found"
  exit 1
fi

# Clean up any existing Windows directory before extraction
if [ -d "Windows" ]; then
  echo "Removing existing Windows directory"
  rm -rf Windows
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
  bundle exec maze-runner --app=features/fixtures/mazerunner/build/Windows/$EXE_FILE --os=windows --fail-fast features/dev.feature
else
  bundle exec maze-runner --app=features/fixtures/mazerunner/build/Windows/$EXE_FILE --os=windows --fail-fast --exclude=features/dev.feature features
fi