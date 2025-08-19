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
  unzip Windows-dev-${UNITY_PERFORMANCE_VERSION:0:4}.zip
  EXE_FILE="mazerunner_windows_dev.exe"
elif [ "$BUILD_TYPE" == "release" ]; then
  unzip Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip
  EXE_FILE="mazerunner_windows.exe"
else
  echo "Invalid argument: $BUILD_TYPE. Use 'release' or 'dev'."
  exit 1
fi

popd

bundle install

if [ "$BUILD_TYPE" == "dev" ]; then
  bundle exec maze-runner --app=features/fixtures/mazerunner/build/Windows/$EXE_FILE --os=windows --fail-fast features/dev.feature
else
  bundle exec maze-runner --app=features/fixtures/mazerunner/build/Windows/$EXE_FILE --os=windows --fail-fast --exclude=features/dev.feature features
fi