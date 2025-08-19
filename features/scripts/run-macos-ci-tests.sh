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
  unzip mazerunner_macos_dev_${UNITY_PERFORMANCE_VERSION:0:4}.zip
  APP_NAME="mazerunner_macos_dev_${UNITY_PERFORMANCE_VERSION:0:4}.app"
else
  unzip mazerunner_macos_${UNITY_PERFORMANCE_VERSION:0:4}.zip
  APP_NAME="mazerunner_macos_${UNITY_PERFORMANCE_VERSION:0:4}.app"
fi
popd

rm -rf Gemfile.lock
bundle install

if [ "$BUILD_TYPE" == "dev" ]; then
  bundle exec maze-runner --app=features/fixtures/mazerunner/$APP_NAME --os=macos --fail-fast features/dev.feature
else
  bundle exec maze-runner --app=features/fixtures/mazerunner/$APP_NAME --os=macos --fail-fast features
fi
