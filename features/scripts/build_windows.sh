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
    win_project_path=`wslpath -w "$project_path"`
    echo "WSL project path: $project_path"
    echo "Windows project path: $win_project_path"
  popd
popd

# remove post build job for iOS bitcode as namespace is not available and it doesn't seem possible to conditionally remove using preprocessor directives
rm "$project_path/Assets/Editor/DisablingBitcodeiOS.cs"
rm "$project_path/Assets/Editor/DisablingBitcodeiOS.cs.meta"

UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Editor/Unity.exe"
"$UNITY_PATH" $DEFAULT_CLI_ARGS \
  -projectPath "$win_project_path" \
  -executeMethod "Builder.WindowsRelease"

RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

# Zip up the built artifacts
cd features/fixtures/mazerunner/build
zip -r Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip Windows

