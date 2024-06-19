#!/bin/bash -e

# Check if the argument is provided
if [ $# -ne 1 ]; then
  echo "Usage: $0 <release|dev>"
  exit 1
fi

# Set the mode based on the argument
RUN_MODE=$1

if [ "$RUN_MODE" == "dev" ]; then
  ZIP_FILE="mazerunner_webgl_dev_${UNITY_PERFORMANCE_VERSION:0:4}.zip"
elif [ "$RUN_MODE" == "release" ]; then
  ZIP_FILE="mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4}.zip"
else
  echo "Invalid argument: $RUN_MODE. Use 'release' or 'dev'."
  exit 1
fi

pushd features/fixtures/mazerunner/
  unzip $ZIP_FILE
popd

bundle install

bundle exec maze-runner --farm=local --browser=firefox --fail-fast features
