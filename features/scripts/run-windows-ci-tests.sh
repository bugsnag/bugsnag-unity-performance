#!/bin/bash -e

# Check if the argument is provided
if [ $# -ne 1 ]; then
  echo "Usage: $0 <release|dev>"
  exit 1
fi

# Set the mode based on the argument
RUN_MODE=$1

pushd features/fixtures/mazerunner/build

if [ "$RUN_MODE" == "dev" ]; then
  unzip Windows-dev-${UNITY_PERFORMANCE_VERSION:0:4}.zip
  EXE_FILE="mazerunner_windows_dev.exe"
elif [ "$RUN_MODE" == "release" ]; then
  unzip Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip
  EXE_FILE="mazerunner_windows.exe"
else
  echo "Invalid argument: $RUN_MODE. Use 'release' or 'dev'."
  exit 1
fi

popd

bundle install
bundle exec maze-runner --app=features/fixtures/mazerunner/build/Windows/$EXE_FILE --os=windows --fail-fast features
