#!/usr/bin/env bash

if [ -z "$UNITY_PERFORMANCE_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"

echo "\`Unity\` executable = $UNITY_PATH/Unity"


pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

DEFAULT_CLI_ARGS="-quit -nographics -batchmode -logFile generateXcodeProject.log"

project_path=`pwd`/mazerunner

# Generate the Xcode project for iOS
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.IosBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

