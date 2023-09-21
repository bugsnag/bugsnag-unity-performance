#!/usr/bin/env bash

if [ -z "$UNITY_PERFORMANCE_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

UNITY_PATH="/mnt/c/PROGRA~1/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Editor/Unity.exe"


pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_windows.log"

project_path=`pwd`/mazerunner

# Build for Android

$UNITY_PATH $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.Windows
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/mazerunner_windows $project_path/mazerunner_windows_$UNITY_PERFORMANCE_VERSION

(cd $project_path && zip -q -r mazerunner_windows_$UNITY_PERFORMANCE_VERSION.zip mazerunner_windows_$UNITY_PERFORMANCE_VERSION)
