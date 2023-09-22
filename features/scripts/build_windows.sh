#!/bin/bash -e

set -m UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Editor/Unity.exe"

DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_windows.log"

project_path=`pwd`/features/fixtures/mazerunner

echo $project_path

$UNITY_PATH $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.Windows
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi
  
mv $project_path/mazerunner_windows $project_path/mazerunner_windows_$UNITY_PERFORMANCE_VERSION
(cd $project_path && zip -q -r mazerunner_windows_$UNITY_PERFORMANCE_VERSION.zip mazerunner_windows_$UNITY_PERFORMANCE_VERSION)