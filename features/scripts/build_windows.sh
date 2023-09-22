#!/bin/bash -e

set -m

DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_windows.log"

# Determine the project path in Windows style to pass to the Unity exe
SCRIPT_DIR=$(dirname "$(realpath $0)")
pushd $SCRIPT_DIR
  pushd ../..
    root_path=`pwd`
  popd
  pushd ../fixtures
    project_path="$root_path/features/fixtures/mazerunner"
    project_path=`wslpath -w "$project_path"`
    echo "Project path: $project_path"
  popd
popd

UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Editor/Unity.exe"
"$UNITY_PATH" $DEFAULT_CLI_ARGS \
  -projectPath "$project_path" \
  -executeMethod "Builder.Windows"

RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/mazerunner_windows $project_path/mazerunner_windows_$UNITY_PERFORMANCE_VERSION
(cd $project_path && zip -q -r mazerunner_windows_$UNITY_PERFORMANCE_VERSION.zip mazerunner_windows_$UNITY_PERFORMANCE_VERSION)
