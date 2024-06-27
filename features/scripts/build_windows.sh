#!/bin/bash -e

set -m

DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_windows.log"

# Check if the argument is provided
if [ $# -ne 1 ]; then
  echo "Usage: $0 <release|dev>"
  exit 1
fi

# Set the mode based on the argument
RUN_MODE=$1

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

if [ "$RUN_MODE" == "dev" ]; then
  "$UNITY_PATH" $DEFAULT_CLI_ARGS \
    -projectPath "$win_project_path" \
    -executeMethod "Builder.WindowsDev"
  ZIP_FILE="Windows-dev-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
elif [ "$RUN_MODE" == "release" ]; then
  "$UNITY_PATH" $DEFAULT_CLI_ARGS \
    -projectPath "$win_project_path" \
    -executeMethod "Builder.WindowsRelease"
  ZIP_FILE="Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
else
  echo "Invalid argument: $RUN_MODE. Use 'release' or 'dev'."
  exit 1
fi

RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

# Zip up the built artifacts
cd features/fixtures/mazerunner/build
zip -r $ZIP_FILE Windows
