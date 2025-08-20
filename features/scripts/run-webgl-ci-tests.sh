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
elif [ "$BUILD_TYPE" == "release" ]; then
  ZIP_FILE="mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4}.zip"
else
  echo "Invalid argument: $BUILD_TYPE. Use 'release' or 'dev'."
  exit 1
fi

pushd features/fixtures/mazerunner/
  unzip $ZIP_FILE
popd

bundle install

if [ "$BUILD_TYPE" == "dev" ]; then
  bundle exec maze-runner --farm=local --browser=firefox --fail-fast features/dev.feature
else
  bundle exec maze-runner --farm=local --browser=firefox --fail-fast --exclude=features/dev.feature features
fi